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
}
