// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.InMemory.Tests;

[ExcludeFromCodeCoverage]
public class InMemoryRepository_Tests
{
    #region Artifacts
    private readonly Lazy<InMemoryRepository<InMemoryMovie>> _Repository = new(() => new InMemoryRepository<InMemoryMovie>(Movies.OfType<InMemoryMovie>()));

    private readonly DateTimeOffset startTime = DateTimeOffset.UtcNow;

    public InMemoryRepository<InMemoryMovie> Repository => _Repository.Value;
    #endregion

    #region Constructors
    [Fact]
    public void Ctor_Empty()
    {
        var Repository = new InMemoryRepository<InMemoryMovie>();

        Repository.Should().NotBeNull();
        Repository.GetEntities().Should().BeEmpty();
    }

    [Fact]
    public void Ctor_Populated()
    {
        Repository.Should().NotBeNull();
        Repository.GetEntities().Should().NotBeEmpty();
    }
    #endregion

    #region AsQueryableAsync
    [Fact]
    public async Task AsQueryableAsync_Throws()
    {
        Repository.ThrowException = new ApplicationException();
        Func<Task> act = async () => _ = await Repository.AsQueryableAsync();
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task AsQueryableAsync_ReturnsQueryable()
    {
        var sut = await Repository.AsQueryableAsync();
        sut.Should().NotBeNull().And.BeAssignableTo<IQueryable<InMemoryMovie>>();
    }

    [Theory]
    [InlineData("id-001")]
    public async Task AsQueryableAsync_CanRetrieveSingleItems(string id)
    {
        InMemoryMovie expected = Repository.GetEntity(id);
        InMemoryMovie actual = (await Repository.AsQueryableAsync()).Single(m => m.Id == id);
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AsQueryable_CanRetrieveFilteredLists()
    {
        int expected = Repository.GetEntities().Count(m => m.Rating == MovieRating.R);
        List<InMemoryMovie> actual = (await Repository.AsQueryableAsync()).Where(m => m.Rating == MovieRating.R).ToList();
        actual.Should().HaveCount(expected);
    }
    #endregion

    #region CreateAsync
    [Fact]
    public async Task CreateAsync_Throws_OnForcedException()
    {
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther);
        Repository.ThrowException = new ApplicationException("test exception");

        Func<Task> act = async () => await Repository.CreateAsync(addition);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("movie-blackpanther")]
    public async Task CreateAsync_CreatesNewEntity_WithSpecifiedId(string id)
    {
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        InMemoryMovie sut = addition.Clone();

        await Repository.CreateAsync(sut);

        InMemoryMovie actual = Repository.GetEntity(id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.Should().NotBeEquivalentTo<ITableData>(addition).And.BeEquivalentTo<ITableData>(sut);

        actual.Id.Should().Be(id);
        actual.UpdatedAt.Should().BeAfter(startTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateAsync_CreatesNewEntity_WithNullId(string id)
    {
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther);
        addition.Id = id;
        InMemoryMovie sut = addition.Clone();

        await Repository.CreateAsync(sut);

        sut.Id.Should().NotBeNullOrEmpty();
        InMemoryMovie actual = Repository.GetEntity(sut.Id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.UpdatedAt.Should().BeAfter(startTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("id-002")]
    public async Task CreateAsync_ThrowsConflict(string id)
    {
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        InMemoryMovie sut = addition.Clone();
        InMemoryMovie expected = Repository.GetEntity(id);

        Func<Task> act = async () => await Repository.CreateAsync(sut);

        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409).And.WithPayload(expected);
    }

    [Theory]
    [InlineData("create-test-metadata")]
    public async Task CreateAsync_UpdatesMetadata(string id)
    {
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        InMemoryMovie sut = addition.Clone();
        sut.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        byte[] expectedVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        sut.Version = expectedVersion.ToArray();

        await Repository.CreateAsync(sut);

        InMemoryMovie actual = Repository.GetEntity(id);
        actual.Should().BeEquivalentTo<IMovie>(addition);
        actual.Should().NotBeEquivalentTo<ITableData>(addition).And.BeEquivalentTo<ITableData>(sut);

        actual.Id.Should().Be(id);
        actual.UpdatedAt.Should().BeAfter(startTime).And.BeBefore(DateTimeOffset.UtcNow);
        actual.Version.Should().NotBeEquivalentTo(expectedVersion);
    }

    [Theory]
    [InlineData("create-test-disconnected")]
    public async Task CreateAsync_StoresDisconnectedEntity(string id)
    {
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        InMemoryMovie sut = addition.Clone();
        sut.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1);
        byte[] expectedVersion = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        sut.Version = expectedVersion.ToArray();

        await Repository.CreateAsync(sut);

        InMemoryMovie actual = Repository.GetEntity(id);
        actual.Should().NotBeSameAs(sut);
    }
    #endregion

    #region DeleteAsync
    [Theory]
    [InlineData("id-002")]
    public async Task DeleteAsync_Throws_OnForcedException(string id)
    {
        Repository.ThrowException = new ApplicationException("test exception");

        Func<Task> act = async () => await Repository.DeleteAsync(id);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData(null, 400)]
    [InlineData("", 400)]
    [InlineData("not-found", 404)]
    public async Task DeleteAsync_Throws_OnBadIds(string id, int expectedStatusCode)
    {
        Func<Task> act = async () => await Repository.DeleteAsync(id);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(expectedStatusCode);
        Repository.GetEntities().Should().HaveCount(Movies.Count);
    }

    [Theory]
    [InlineData("id-003")]
    public async Task DeleteAsync_Throws_WhenVersionMismatch(string id)
    {
        InMemoryMovie expected = Repository.GetEntity(id);
        byte[] version = Guid.NewGuid().ToByteArray();

        Func<Task> act = async () => await Repository.DeleteAsync(id, version);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(expected);
    }

    [Theory]
    [InlineData("id-004")]
    public async Task DeleteAsync_Deletes_WhenVersionMatch(string id)
    {
        InMemoryMovie expected = Repository.GetEntity(id);
        byte[] version = expected.Version.ToArray();

        await Repository.DeleteAsync(id, version);

        InMemoryMovie actual = Repository.GetEntity(id);
        actual.Should().BeNull();
    }

    [Theory]
    [InlineData("id-005")]
    public async Task DeleteAsync_Deletes_WhenNoVersion(string id)
    {
        await Repository.DeleteAsync(id);

        InMemoryMovie actual = Repository.GetEntity(id);
        actual.Should().BeNull();
    }
    #endregion

    #region ReadAsync
    [Theory]
    [InlineData("id-006")]
    public async Task ReadAsync_Throws_OnForcedException(string id)
    {
        Repository.ThrowException = new ApplicationException("test exception");
        Func<Task> act = async () => _ = await Repository.ReadAsync(id);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("id-007")]
    public async Task ReadAsync_ReturnsDisconnectedEntity(string id)
    {
        InMemoryMovie expected = Repository.GetEntity(id);
        InMemoryMovie actual = await Repository.ReadAsync(id);
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
    [InlineData("id-008")]
    public async Task ReplaceAsync_Throws_OnForcedException(string id)
    {
        Repository.ThrowException = new ApplicationException("test exception");
        InMemoryMovie replacement = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        Func<Task> act = async () => await Repository.ReplaceAsync(replacement);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData(null, 400)]
    [InlineData("", 400)]
    [InlineData("not-found", 404)]
    public async Task ReplaceAsync_Throws_OnBadId(string id, int expectedStatusCode)
    {
        InMemoryMovie replacement = Movies.OfType<InMemoryMovie>(Movies.BlackPanther);
        replacement.Id = id;
        Func<Task> act = async () => await Repository.ReplaceAsync(replacement);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(expectedStatusCode);
    }

    [Theory]
    [InlineData("id-009")]
    public async Task ReplaceAsync_Throws_OnVersionMismatch(string id)
    {
        InMemoryMovie replacement = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        InMemoryMovie expected = Repository.GetEntity(id);
        byte[] version = Guid.NewGuid().ToByteArray();

        Func<Task> act = async () => await Repository.ReplaceAsync(replacement, version);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(412).And.WithPayload(expected);
    }

    [Theory]
    [InlineData("id-010")]
    public async Task ReplaceAsync_Replaces_OnVersionMatch(string id)
    {
        InMemoryMovie replacement = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        InMemoryMovie expected = Repository.GetEntity(id);
        byte[] version = expected.Version.ToArray();

        await Repository.ReplaceAsync(replacement, version);

        InMemoryMovie actual = Repository.GetEntity(id);

        actual.Should().BeEquivalentTo<IMovie>(replacement).And.NotBeEquivalentTo<ITableData>(expected);
        actual.Version.Should().NotBeEquivalentTo(version);
        actual.UpdatedAt.Should().BeAfter(startTime).And.BeBefore(DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("id-011")]
    public async Task ReplaceAsync_Replaces_OnNoVersion(string id)
    {
        InMemoryMovie replacement = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        InMemoryMovie expected = Repository.GetEntity(id);
        byte[] version = expected.Version.ToArray();

        await Repository.ReplaceAsync(replacement);

        InMemoryMovie actual = Repository.GetEntity(id);

        actual.Should().BeEquivalentTo<IMovie>(replacement).And.NotBeEquivalentTo<ITableData>(expected);
        actual.Version.Should().NotBeEquivalentTo(version);
        actual.UpdatedAt.Should().BeAfter(startTime).And.BeBefore(DateTimeOffset.UtcNow);
    }
    #endregion
}
