// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Tests;

namespace Microsoft.AspNetCore.Datasync.InMemory.Tests;

[ExcludeFromCodeCoverage]
public class InMemoryRepository_Tests : RepositoryTests<InMemoryMovie>
{
    #region Setup
    public InMemoryRepository_Tests() : base()
    {
        Repository = new InMemoryRepository<InMemoryMovie>(Movies.OfType<InMemoryMovie>());
    }

    protected override InMemoryMovie GetEntity(string id)
        => (Repository as InMemoryRepository<InMemoryMovie>).GetEntity(id);

    protected override int GetEntityCount()
        => (Repository as InMemoryRepository<InMemoryMovie>).GetEntities().Count;

    protected void SetException(Exception ex)
        => (Repository as InMemoryRepository<InMemoryMovie>).ThrowException = ex;
    #endregion

    [Fact]
    public void Ctor_Empty()
    {
        var repository = new InMemoryRepository<InMemoryMovie>();

        repository.Should().NotBeNull();
        repository.GetEntities().Should().BeEmpty();
    }

    [Fact]
    public void Ctor_Populated()
    {
        Repository.Should().NotBeNull();
        GetEntityCount().Should().BeGreaterThan(0);
    }

    [Fact]
    public void Ctor_Populated_NoId()
    {
        var movies = Movies.OfType<InMemoryMovie>().ConvertAll(m => { m.Id = null; return m; });
        var repository = new InMemoryRepository<InMemoryMovie>(movies);
        repository.Should().NotBeNull();
        repository.GetEntities().Count.Should().Be(movies.Count);
    }

    [Fact]
    public async Task AsQueryableAsync_Throws()
    {
        SetException(new ApplicationException("test exception"));
        Func<Task> act = async () => _ = await Repository.AsQueryableAsync();
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task CreateAsync_Throws_OnForcedException()
    {
        SetException(new ApplicationException("test exception"));
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther);

        Func<Task> act = async () => await Repository.CreateAsync(addition);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("id-002")]
    public async Task DeleteAsync_Throws_OnForcedException(string id)
    {
        SetException(new ApplicationException("test exception"));
        Func<Task> act = async () => await Repository.DeleteAsync(id);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("id-006")]
    public async Task ReadAsync_Throws_OnForcedException(string id)
    {
        SetException(new ApplicationException("test exception"));
        Func<Task> act = async () => _ = await Repository.ReadAsync(id);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("id-008")]
    public async Task ReplaceAsync_Throws_OnForcedException(string id)
    {
        SetException(new ApplicationException("test exception"));
        InMemoryMovie replacement = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        Func<Task> act = async () => await Repository.ReplaceAsync(replacement);
        await act.Should().ThrowAsync<ApplicationException>();
    }
}
