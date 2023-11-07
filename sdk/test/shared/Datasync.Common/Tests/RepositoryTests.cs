// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Datasync;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Datasync.Common.Tests;

/// <summary>
/// A set of common tests for the <see cref="IRepository{TEntity}"/> implementations.
/// </summary>
/// <typeparam name="TEntity">The type of entity being tested.</typeparam>
[ExcludeFromCodeCoverage]
public abstract class RepositoryTests<TEntity> where TEntity : class, ITableData, IMovie, new()
{
    /// <summary>
    /// The time that the current test started.
    /// </summary>
    protected DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Returns true if all the requirements for live tests are met.
    /// </summary>
    protected virtual bool CanRunLiveTests() => true;

    /// <summary>
    /// The actual test class must provide an implementation that retrieves the entity through
    /// the backing data store.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <returns>Either <c>null</c> if the entity does not exist, or the entity.</returns>
    protected abstract Task<TEntity> GetEntityAsync(string id);

    /// <summary>
    /// The actual test class must provide an implementation that retrieves the entity count in
    /// the backing data store.
    /// </summary>
    /// <returns>The number of entities in the store.</returns>
    protected abstract Task<int> GetEntityCountAsync();

    /// <summary>
    /// Retrieves a populated repository for testing.
    /// </summary>
    protected abstract Task<IRepository<TEntity>> GetPopulatedRepositoryAsync();

    /// <summary>
    /// Retrieves a random ID from the database for testing.
    /// </summary>
    protected abstract Task<string> GetRandomEntityIdAsync(bool exists);

    #region AsQueryableAsync
    [SkippableFact]
    public async Task AsQueryableAsync_ReturnsQueryable()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();

        var sut = await Repository.AsQueryableAsync();

        sut.Should().NotBeNull().And.BeAssignableTo<IQueryable<TEntity>>();
    }

    [SkippableFact]
    public async Task AsQueryableAsync_CanRetrieveSingleItems()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);
        TEntity expected = await GetEntityAsync(id);

        TEntity actual = (await Repository.AsQueryableAsync()).Single(m => m.Id == id);

        actual.Should().BeEquivalentTo(expected);
    }

    [SkippableFact]
    public async Task AsQueryableAsync_CanRetrieveFilteredLists()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        int expected = Movies.Count<TEntity>(m => m.Rating == MovieRating.R);

        List<TEntity> actual = (await Repository.AsQueryableAsync()).Where(m => m.Rating == MovieRating.R).ToList();

        actual.Should().HaveCount(expected);
    }

    [SkippableFact]
    public async Task AsQueryableAsync_CanSelectFromList()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        int expected = Movies.Count<TEntity>(m => m.Rating == MovieRating.R);

        var actual = (await Repository.AsQueryableAsync()).Where(m => m.Rating == MovieRating.R).Select(m => new { m.Id, m.Title }).ToList();

        actual.Should().HaveCount(expected);
    }

    [SkippableFact]
    public async Task AsQueryableAsync_CanUseTopAndSkip()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();

        var actual = (await Repository.AsQueryableAsync()).Where(m => m.Rating == MovieRating.R).Skip(5).Take(20).ToList();

        actual.Should().HaveCount(20);
    }
    #endregion

    #region CreateAsync
    [SkippableFact]
    public async Task CreateAsync_CreatesNewEntity_WithSpecifiedId()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(false);
        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();

        await Repository.CreateAsync(sut);

        TEntity actual = await GetEntityAsync(id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.Should().NotBeEquivalentTo<ITableData>(addition).And.HaveEquivalentMetadataTo(sut);
        actual.Id.Should().Be(id);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateAsync_CreatesNewEntity_WithNullId(string id)
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther);
        addition.Id = id;
        TEntity sut = addition.Clone();

        await Repository.CreateAsync(sut);

        sut.Id.Should().NotBeNullOrEmpty();
        TEntity actual = await GetEntityAsync(sut.Id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [SkippableFact]
    public async Task CreateAsync_ThrowsConflict()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();
        TEntity expected = await GetEntityAsync(id);

        Func<Task> act = async () => await Repository.CreateAsync(sut);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409).And.WithPayload(expected);
    }

    [SkippableFact]
    public async Task CreateAsync_UpdatesMetadata()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(false);

        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();
        sut.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        byte[] expectedVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        sut.Version = expectedVersion.ToArray();

        await Repository.CreateAsync(sut);

        TEntity actual = await GetEntityAsync(id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.Should().NotBeEquivalentTo<ITableData>(addition).And.HaveEquivalentMetadataTo(sut);
        actual.Id.Should().Be(id);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        actual.Version.Should().NotBeEquivalentTo(expectedVersion);
    }

    [SkippableFact]
    public async Task CreateAsync_StoresDisconnectedEntity()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(false);

        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();
        sut.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        byte[] expectedVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        sut.Version = expectedVersion.ToArray();

        await Repository.CreateAsync(sut);

        TEntity actual = await GetEntityAsync(id);
        actual.Should().NotBeSameAs(sut);
    }
    #endregion

    #region DeleteAsync
    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    public async Task DeleteAsync_Throws_OnBadIds(string id)
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();

        Func<Task> act = async () => await Repository.DeleteAsync(id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(400);
        (await GetEntityCountAsync()).Should().Be(Movies.Count<TEntity>());
    }

    [SkippableFact]
    public async Task DeleteAsync_Throws_OnMissingIds()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(false);

        Func<Task> act = async () => await Repository.DeleteAsync(id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
        (await GetEntityCountAsync()).Should().Be(Movies.Count<TEntity>());
    }

    [SkippableFact]
    public async Task DeleteAsync_Throws_WhenVersionMismatch()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        TEntity expected = await GetEntityAsync(id);
        byte[] version = Guid.NewGuid().ToByteArray();

        Func<Task> act = async () => await Repository.DeleteAsync(id, version);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(expected);
    }

    [SkippableFact]
    public async Task DeleteAsync_Deletes_WhenVersionMatch()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        TEntity expected = await GetEntityAsync(id);
        byte[] version = expected.Version.ToArray();

        await Repository.DeleteAsync(id, version);

        TEntity actual = await GetEntityAsync(id);
        actual.Should().BeNull();
    }

    [SkippableFact]
    public async Task DeleteAsync_Deletes_WhenNoVersion()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        await Repository.DeleteAsync(id);

        TEntity actual = await GetEntityAsync(id);
        actual.Should().BeNull();
    }
    #endregion

    #region ReadAsync
    [SkippableFact]
    public async Task ReadAsync_ReturnsDisconnectedEntity()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        TEntity expected = await GetEntityAsync(id);

        TEntity actual = await Repository.ReadAsync(id);

        actual.Should().BeEquivalentTo(expected).And.NotBeSameAs(expected);
    }

    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ReadAsync_Throws_OnBadId(string id)
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();

        Func<Task> act = async () => _ = await Repository.ReadAsync(id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(400);
    }

    [SkippableFact]
    public async Task ReadAsync_Throws_OnMissingId()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(false);

        Func<Task> act = async () => _ = await Repository.ReadAsync(id);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }
    #endregion

    #region ReplaceAsync
    [SkippableTheory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ReplaceAsync_Throws_OnBadId(string id)
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther);
        replacement.Id = id;

        Func<Task> act = async () => await Repository.ReplaceAsync(replacement);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(400);
    }

    [SkippableFact]
    public async Task ReplaceAsync_Throws_OnMissingId()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(false);
        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther, id);

        Func<Task> act = async () => await Repository.ReplaceAsync(replacement);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(404);
    }

    [SkippableFact]
    public async Task ReplaceAsync_Throws_OnVersionMismatch()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity expected = await GetEntityAsync(id);
        byte[] version = Guid.NewGuid().ToByteArray();

        Func<Task> act = async () => await Repository.ReplaceAsync(replacement, version);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(expected);
    }

    [SkippableFact]
    public async Task ReplaceAsync_Replaces_OnVersionMatch()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity expected = await GetEntityAsync(id);
        byte[] version = expected.Version.ToArray();

        await Repository.ReplaceAsync(replacement, version);

        TEntity actual = await GetEntityAsync(id);

        actual.Should().BeEquivalentTo<IMovie>(replacement).And.NotBeEquivalentTo<ITableData>(expected);
        actual.Version.Should().NotBeEquivalentTo(version);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [SkippableFact]
    public async Task ReplaceAsync_Replaces_OnNoVersion()
    {
        Skip.IfNot(CanRunLiveTests());
        IRepository<TEntity> Repository = await GetPopulatedRepositoryAsync();
        string id = await GetRandomEntityIdAsync(true);

        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity expected = await GetEntityAsync(id);
        byte[] version = expected.Version.ToArray();

        await Repository.ReplaceAsync(replacement);

        TEntity actual = await GetEntityAsync(id);

        actual.Should().BeEquivalentTo<IMovie>(replacement).And.NotBeEquivalentTo<ITableData>(expected);
        actual.Version.Should().NotBeEquivalentTo(version);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }
    #endregion
}
