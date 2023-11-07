// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Tests;

namespace Microsoft.AspNetCore.Datasync.InMemory.Tests;

[ExcludeFromCodeCoverage]
public class InMemoryRepository_Tests : RepositoryTests<InMemoryMovie>
{
    #region Setup
    private InMemoryRepository<InMemoryMovie> repository;

    protected override Task<InMemoryMovie> GetEntityAsync(string id)
        => Task.FromResult(repository.GetEntity(id));

    protected override Task<int> GetEntityCountAsync()
        => Task.FromResult(repository.GetEntities().Count);

    protected override Task<IRepository<InMemoryMovie>> GetPopulatedRepositoryAsync()
    {
        repository = new InMemoryRepository<InMemoryMovie>(Movies.OfType<InMemoryMovie>());
        return Task.FromResult<IRepository<InMemoryMovie>>(repository);
    }

    protected override Task<string> GetRandomEntityIdAsync(bool exists)
    {
        Random random = new();
        return Task.FromResult(exists ? repository.GetEntities()[random.Next(repository.GetEntities().Count)].Id : Guid.NewGuid().ToString());
    }
    #endregion

    [Fact]
    public void Ctor_Empty()
    {
        var repository = new InMemoryRepository<InMemoryMovie>();
        repository.Should().NotBeNull();
        repository.GetEntities().Should().BeEmpty();
    }

    [Fact]
    public async Task Ctor_Populated()
    {
        var repository = await GetPopulatedRepositoryAsync() as InMemoryRepository<InMemoryMovie>;
        int movieCount = Movies.MovieList.Length;
        repository.Should().NotBeNull();
        repository.GetEntities().Count.Should().Be(movieCount);
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
        var repository = await GetPopulatedRepositoryAsync() as InMemoryRepository<InMemoryMovie>;
        repository.ThrowException = new ApplicationException("test exception");
        Func<Task> act = async () => _ = await repository.AsQueryableAsync();
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Fact]
    public async Task CreateAsync_Throws_OnForcedException()
    {
        var repository = await GetPopulatedRepositoryAsync() as InMemoryRepository<InMemoryMovie>;
        repository.ThrowException = new ApplicationException("test exception");
        InMemoryMovie addition = Movies.OfType<InMemoryMovie>(Movies.BlackPanther);

        Func<Task> act = async () => await repository.CreateAsync(addition);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("id-002")]
    public async Task DeleteAsync_Throws_OnForcedException(string id)
    {
        var repository = await GetPopulatedRepositoryAsync() as InMemoryRepository<InMemoryMovie>;
        repository.ThrowException = new ApplicationException("test exception");
        Func<Task> act = async () => await repository.DeleteAsync(id);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("id-006")]
    public async Task ReadAsync_Throws_OnForcedException(string id)
    {
        var repository = await GetPopulatedRepositoryAsync() as InMemoryRepository<InMemoryMovie>;
        repository.ThrowException = new ApplicationException("test exception");
        Func<Task> act = async () => _ = await repository.ReadAsync(id);
        await act.Should().ThrowAsync<ApplicationException>();
    }

    [Theory]
    [InlineData("id-008")]
    public async Task ReplaceAsync_Throws_OnForcedException(string id)
    {
        var repository = await GetPopulatedRepositoryAsync() as InMemoryRepository<InMemoryMovie>;
        repository.ThrowException = new ApplicationException("test exception");
        InMemoryMovie replacement = Movies.OfType<InMemoryMovie>(Movies.BlackPanther, id);
        Func<Task> act = async () => await repository.ReplaceAsync(replacement);
        await act.Should().ThrowAsync<ApplicationException>();
    }
}
