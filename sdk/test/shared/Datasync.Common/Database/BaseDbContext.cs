// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Datasync.Common.Database;

public abstract class BaseDbContext<TContext, TEntity> : DbContext
    where TContext : DbContext
    where TEntity : class, IMovie, ITableData, new()
{
    protected BaseDbContext(DbContextOptions<TContext> options) : base(options)
    {
    }

    /// <summary>
    /// If set, the <see cref="ITestOutputHelper"/> for logging.
    /// </summary>
    protected ITestOutputHelper OutputHelper { get; set; }

    /// <summary>
    /// The list of Movie IDs that are in the database.
    /// </summary>
    protected IList<string> MovieIds { get; set; }

    /// <summary>
    /// The collection of movies.
    /// </summary>
    public virtual DbSet<TEntity> Movies { get; set; }

    /// <summary>
    /// Executes the provided SQL statement for each entity in our set of entities.
    /// </summary>
    /// <param name="format"></param>
    protected void ExecuteRawSqlOnEachEntity(string format)
    {
        foreach (var table in Model.GetEntityTypes())
        {
            string sql = string.Format(format, table.GetTableName());
            Database.ExecuteSqlRaw(sql);
        }
    }

    /// <summary>
    /// Populates the database with the core set of movies.  Ensures that we
    /// have the same data for all tests.
    /// </summary>
    protected void PopulateDatabase()
    {
        List<TEntity> movies = TestData.Movies.OfType<TEntity>();
        MovieIds = movies.ConvertAll(m => m.Id);

        // Make sure we are populating with the right data
        bool setUpdatedAt = Attribute.IsDefined(typeof(TEntity).GetProperty("UpdatedAt")!, typeof(UpdatedByRepositoryAttribute));
        bool setVersion = Attribute.IsDefined(typeof(TEntity).GetProperty("Version")!, typeof(UpdatedByRepositoryAttribute));
        foreach (TEntity movie in movies)
        {
            if (setUpdatedAt) movie.UpdatedAt = DateTimeOffset.UtcNow;
            if (setVersion) movie.Version = Guid.NewGuid().ToByteArray();
            Movies.Add(movie);
        }
        SaveChanges();
    }
}
