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
    private readonly TestDbContext context;
    private readonly EntityTableRepository<SqliteEntityMovie> repository;

    public EntityTableRepository_Tests(ITestOutputHelper output) : base()
    {
        context = TestDbContext.CreateContext(output);
        // This is for Sqlite because it doesn't support computed columns
        EntityTableRepositoryOptions options = new() { DatabaseUpdatesTimestamp = false, DatabaseUpdatesVersion = false };
        repository = new EntityTableRepository<SqliteEntityMovie>(context, options);
        Repository = repository;
    }

    protected override SqliteEntityMovie GetEntity(string id)
        => context.Movies.AsNoTracking().SingleOrDefault(m => m.Id == id);

    protected override int GetEntityCount()
        => context.Movies.Count();
    #endregion

    [Fact]
    public void EntityTableRepository_BadDbSet_Throws()
    {
        Action act = () => _ = new EntityTableRepository<EntityTableData>(context);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EntityTableRepository_GoodDbSet_Works()
    {
        Action act = () => _ = new EntityTableRepository<SqliteEntityMovie>(context);
        act.Should().NotThrow();
    }

    [Fact]
    public async void WrapExceptionAsync_ThrowsConflictException_WhenDbConcurrencyUpdateExceptionThrown()
    {
        const string id = "id-001";
        SqliteEntityMovie expectedPayload = GetEntity(id);

        static Task innerAction() => throw new DbUpdateConcurrencyException("Concurrency exception");

        Func<Task> act = async () => await repository.WrapExceptionAsync(id, innerAction);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(409).And.WithPayload(expectedPayload);
    }

    [Fact]
    public async void WrapExceptionAsync_ThrowsRepositoryException_WhenDbUpdateExceptionThrown()
    {
        const string id = "id-001";
        SqliteEntityMovie expectedPayload = GetEntity(id);

        static Task innerAction() => throw new DbUpdateException("Non-concurrency exception");

        Func<Task> act = async () => await repository.WrapExceptionAsync(id, innerAction);
        await act.Should().ThrowAsync<RepositoryException>();
    }
}
