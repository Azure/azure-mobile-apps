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
    /// The repository under test.
    /// </summary>
    protected IRepository<TEntity> Repository { get; init; }

    /// <summary>
    /// The time that the current test started.
    /// </summary>
    protected DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The actual test class must provide an implementation that retrieves the entity through
    /// the backing data store.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <returns>Either <c>null</c> if the entity does not exist, or the entity.</returns>
    protected abstract TEntity GetEntity(string id);

    /// <summary>
    /// The actual test class must provide an implementation that retrieves the entity count in
    /// the backing data store.
    /// </summary>
    /// <returns>The number of entities in the store.</returns>
    protected abstract int GetEntityCount();

    #region AsQueryableAsync
    [Fact]
    public async Task AsQueryableAsync_ReturnsQueryable()
    {
        var sut = await Repository.AsQueryableAsync();
        sut.Should().NotBeNull().And.BeAssignableTo<IQueryable<TEntity>>();
    }

    [Theory]
    [InlineData("id-001")]
    public async Task AsQueryableAsync_CanRetrieveSingleItems(string id)
    {
        TEntity expected = GetEntity(id);
        TEntity actual = (await Repository.AsQueryableAsync()).Single(m => m.Id == id);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AsQueryable_CanRetrieveFilteredLists()
    {
        int expected = Movies.Count<TEntity>(m => m.Rating == MovieRating.R);
        List<TEntity> actual = (await Repository.AsQueryableAsync()).Where(m => m.Rating == MovieRating.R).ToList();
        actual.Should().HaveCount(expected);
    }
    #endregion

    #region CreateAsync
    [Theory]
    [InlineData("movie-blackpanther")]
    public async Task CreateAsync_CreatesNewEntity_WithSpecifiedId(string id)
    {
        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();

        await Repository.CreateAsync(sut);

        TEntity actual = GetEntity(id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.Should().NotBeEquivalentTo<ITableData>(addition).And.HaveEquivalentMetadataTo(sut);

        actual.Id.Should().Be(id);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateAsync_CreatesNewEntity_WithNullId(string id)
    {
        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther);
        addition.Id = id;
        TEntity sut = addition.Clone();

        await Repository.CreateAsync(sut);

        sut.Id.Should().NotBeNullOrEmpty();
        TEntity actual = GetEntity(sut.Id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("id-002")]
    public async Task CreateAsync_ThrowsConflict(string id)
    {
        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();
        TEntity expected = GetEntity(id);

        Func<Task> act = async () => await Repository.CreateAsync(sut);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409).And.WithPayload(expected);
    }

    [Theory]
    [InlineData("create-test-metadata")]
    public async Task CreateAsync_UpdatesMetadata(string id)
    {
        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();
        sut.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        byte[] expectedVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        sut.Version = expectedVersion.ToArray();

        await Repository.CreateAsync(sut);

        TEntity actual = GetEntity(id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.Should().NotBeEquivalentTo<ITableData>(addition).And.HaveEquivalentMetadataTo(sut);

        actual.Id.Should().Be(id);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        actual.Version.Should().NotBeEquivalentTo(expectedVersion);
    }

    [Theory]
    [InlineData("create-test-disconnected")]
    public async Task CreateAsync_StoresDisconnectedEntity(string id)
    {
        TEntity addition = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity sut = addition.Clone();
        sut.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        byte[] expectedVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        sut.Version = expectedVersion.ToArray();

        await Repository.CreateAsync(sut);

        TEntity actual = GetEntity(id);
        actual.Should().NotBeSameAs(sut);
    }
    #endregion

    #region DeleteAsync
    [Theory]
    [InlineData(null, 400)]
    [InlineData("", 400)]
    [InlineData("not-found", 404)]
    public async Task DeleteAsync_Throws_OnBadIds(string id, int expectedStatusCode)
    {
        Func<Task> act = async () => await Repository.DeleteAsync(id);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(expectedStatusCode);
        GetEntityCount().Should().Be(Movies.Count<TEntity>());
    }

    [Theory]
    [InlineData("id-003")]
    public async Task DeleteAsync_Throws_WhenVersionMismatch(string id)
    {
        TEntity expected = GetEntity(id);
        byte[] version = Guid.NewGuid().ToByteArray();

        Func<Task> act = async () => await Repository.DeleteAsync(id, version);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(expected);
    }

    [Theory]
    [InlineData("id-004")]
    public async Task DeleteAsync_Deletes_WhenVersionMatch(string id)
    {
        TEntity expected = GetEntity(id);
        byte[] version = expected.Version.ToArray();

        await Repository.DeleteAsync(id, version);

        TEntity actual = GetEntity(id);
        actual.Should().BeNull();
    }

    [Theory]
    [InlineData("id-005")]
    public async Task DeleteAsync_Deletes_WhenNoVersion(string id)
    {
        await Repository.DeleteAsync(id);

        TEntity actual = GetEntity(id);
        actual.Should().BeNull();
    }
    #endregion

    #region ReadAsync
    [Theory]
    [InlineData("id-007")]
    public async Task ReadAsync_ReturnsDisconnectedEntity(string id)
    {
        TEntity expected = GetEntity(id);
        TEntity actual = await Repository.ReadAsync(id);
        actual.Should().BeEquivalentTo(expected).And.NotBeSameAs(expected);
    }

    [Theory]
    [InlineData(null, 400)]
    [InlineData("", 400)]
    [InlineData("not-found", 404)]
    public async Task ReadAsync_Throws_OnBadId(string id, int expectedStatusCode)
    {
        Func<Task> act = async () => _ = await Repository.ReadAsync(id);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(expectedStatusCode);
    }
    #endregion

    #region ReplaceAsync
    [Theory]
    [InlineData(null, 400)]
    [InlineData("", 400)]
    [InlineData("not-found", 404)]
    public async Task ReplaceAsync_Throws_OnBadId(string id, int expectedStatusCode)
    {
        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther);
        replacement.Id = id;
        Func<Task> act = async () => await Repository.ReplaceAsync(replacement);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(expectedStatusCode);
    }

    [Theory]
    [InlineData("id-009")]
    public async Task ReplaceAsync_Throws_OnVersionMismatch(string id)
    {
        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity expected = GetEntity(id);
        byte[] version = Guid.NewGuid().ToByteArray();

        Func<Task> act = async () => await Repository.ReplaceAsync(replacement, version);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(expected);
    }

    [Theory]
    [InlineData("id-010")]
    public async Task ReplaceAsync_Replaces_OnVersionMatch(string id)
    {
        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity expected = GetEntity(id);
        byte[] version = expected.Version.ToArray();

        await Repository.ReplaceAsync(replacement, version);

        TEntity actual = GetEntity(id);

        actual.Should().BeEquivalentTo<IMovie>(replacement).And.NotBeEquivalentTo<ITableData>(expected);
        actual.Version.Should().NotBeEquivalentTo(version);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("id-011")]
    public async Task ReplaceAsync_Replaces_OnNoVersion(string id)
    {
        TEntity replacement = Movies.OfType<TEntity>(Movies.BlackPanther, id);
        TEntity expected = GetEntity(id);
        byte[] version = expected.Version.ToArray();

        await Repository.ReplaceAsync(replacement);

        TEntity actual = GetEntity(id);

        actual.Should().BeEquivalentTo<IMovie>(replacement).And.NotBeEquivalentTo<ITableData>(expected);
        actual.Version.Should().NotBeEquivalentTo(version);
        actual.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
    }
    #endregion
}
