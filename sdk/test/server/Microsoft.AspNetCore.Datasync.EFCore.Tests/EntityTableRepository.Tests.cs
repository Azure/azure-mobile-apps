// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Datasync.EFCore.Tests;

[ExcludeFromCodeCoverage]
public class EntityTableRepository_Tests : RepositoryTests<SqliteEntityMovie>
{
    #region Setup
    private List<SqliteEntityMovie> movies;
    private Lazy<TestDbContext> _context = new(() => TestDbContext.CreateContext());

    private TestDbContext Context { get => _context.Value; }

    protected override Task<SqliteEntityMovie> GetEntityAsync(string id)
        => Task.FromResult(Context.Movies.AsNoTracking().SingleOrDefault(m => m.Id == id));

    protected override Task<int> GetEntityCountAsync()
        => Task.FromResult(Context.Movies.Count());

    protected override Task<IRepository<SqliteEntityMovie>> GetPopulatedRepositoryAsync()
    {
        movies = Context.Movies.AsNoTracking().ToList();
        EntityTableRepositoryOptions options = new() { DatabaseUpdatesTimestamp = false, DatabaseUpdatesVersion = false };
        return Task.FromResult<IRepository<SqliteEntityMovie>>(new EntityTableRepository<SqliteEntityMovie>(Context, options));
    }

    protected override Task<string> GetRandomEntityIdAsync(bool exists)
    {
        Random random = new();
        return Task.FromResult(exists ? movies[random.Next(Context.Movies.Count())].Id : Guid.NewGuid().ToString());
    }
    #endregion

    [Fact]
    public void EntityTableRepository_BadDbSet_Throws()
    {
        Action act = () => _ = new EntityTableRepository<EntityTableData>(Context);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EntityTableRepository_GoodDbSet_Works()
    {
        Action act = () => _ = new EntityTableRepository<SqliteEntityMovie>(Context);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("id-001")]
    public async Task WrapExceptionAsync_ThrowsConflictException_WhenDbConcurrencyUpdateExceptionThrown(string id)
    {
        EntityTableRepository<SqliteEntityMovie> repository = await GetPopulatedRepositoryAsync() as EntityTableRepository<SqliteEntityMovie>;
        SqliteEntityMovie expectedPayload = await GetEntityAsync(id);

        static Task innerAction() => throw new DbUpdateConcurrencyException("Concurrency exception");

        Func<Task> act = async () => await repository.WrapExceptionAsync(id, innerAction);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409).And.WithPayload(expectedPayload);
    }

    [Theory]
    [InlineData("id-001")]
    public async Task WrapExceptionAsync_ThrowsRepositoryException_WhenDbUpdateExceptionThrown(string id)
    {
        EntityTableRepository<SqliteEntityMovie> repository = await GetPopulatedRepositoryAsync() as EntityTableRepository<SqliteEntityMovie>;
        SqliteEntityMovie expectedPayload = await GetEntityAsync(id);

        static Task innerAction() => throw new DbUpdateException("Non-concurrency exception");

        Func<Task> act = async () => await repository.WrapExceptionAsync(id, innerAction);
        await act.Should().ThrowAsync<RepositoryException>();
    }
}
