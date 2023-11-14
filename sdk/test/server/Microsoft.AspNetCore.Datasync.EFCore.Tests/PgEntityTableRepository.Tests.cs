// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.Datasync.EFCore.Tests;

[ExcludeFromCodeCoverage]
public class PgEntityTableRepository_Tests : RepositoryTests<EntityMovie>
{
    #region Setup
    private readonly Random random = new();
    private readonly string connectionString;
    private readonly List<EntityMovie> movies;
    private readonly Lazy<PgDbContext> _context;

    public PgEntityTableRepository_Tests(ITestOutputHelper output) : base()
    {
        connectionString = Environment.GetEnvironmentVariable("ZUMO_PGSQL_CONNECTIONSTRING");
        if (!string.IsNullOrEmpty(connectionString))
        {
            _context = new Lazy<PgDbContext>(() => PgDbContext.CreateContext(connectionString, output));
            movies = Context.Movies.AsNoTracking().ToList();
        }
    }

    private PgDbContext Context { get => _context.Value; }

    protected override bool CanRunLiveTests() => !string.IsNullOrEmpty(connectionString);

    protected override Task<EntityMovie> GetEntityAsync(string id)
        => Task.FromResult(Context.Movies.AsNoTracking().SingleOrDefault(m => m.Id == id));

    protected override Task<int> GetEntityCountAsync()
        => Task.FromResult(Context.Movies.Count());

    protected override Task<IRepository<EntityMovie>> GetPopulatedRepositoryAsync()
        => Task.FromResult<IRepository<EntityMovie>>(new EntityTableRepository<EntityMovie>(Context));

    protected override Task<string> GetRandomEntityIdAsync(bool exists)
       => Task.FromResult(exists ? movies[random.Next(movies.Count)].Id : Guid.NewGuid().ToString());
    #endregion

    [SkippableFact]
    public void EntityTableRepository_BadDbSet_Throws()
    {
        Skip.IfNot(CanRunLiveTests());
        Action act = () => _ = new EntityTableRepository<EntityTableData>(Context);
        act.Should().Throw<ArgumentException>();
    }

    [SkippableFact]
    public void EntityTableRepository_GoodDbSet_Works()
    {
        Skip.IfNot(CanRunLiveTests());
        Action act = () => _ = new EntityTableRepository<EntityMovie>(Context);
        act.Should().NotThrow();
    }

    [SkippableFact]
    public async Task WrapExceptionAsync_ThrowsConflictException_WhenDbConcurrencyUpdateExceptionThrown()
    {
        Skip.IfNot(CanRunLiveTests());
        EntityTableRepository<EntityMovie> repository = await GetPopulatedRepositoryAsync() as EntityTableRepository<EntityMovie>;
        string id = await GetRandomEntityIdAsync(true);
        EntityMovie expectedPayload = await GetEntityAsync(id);

        static Task innerAction() => throw new DbUpdateConcurrencyException("Concurrency exception");

        Func<Task> act = async () => await repository.WrapExceptionAsync(id, innerAction);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409).And.WithPayload(expectedPayload);
    }

    [SkippableFact]
    public async Task WrapExceptionAsync_ThrowsRepositoryException_WhenDbUpdateExceptionThrown()
    {
        Skip.IfNot(CanRunLiveTests());
        EntityTableRepository<EntityMovie> repository = await GetPopulatedRepositoryAsync() as EntityTableRepository<EntityMovie>;
        string id = await GetRandomEntityIdAsync(true);
        EntityMovie expectedPayload = await GetEntityAsync(id);

        static Task innerAction() => throw new DbUpdateException("Non-concurrency exception");

        Func<Task> act = async () => await repository.WrapExceptionAsync(id, innerAction);
        await act.Should().ThrowAsync<RepositoryException>();
    }
}
