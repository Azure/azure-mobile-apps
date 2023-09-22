// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Extensions;
using Datasync.Common.Test.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Datasync.Common.Test.Service;

/// <summary>
/// A <see cref="DbContext"/> for serving the movies set up for tests.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class MovieDbContext : DbContext
{
    public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// The set of movies.
    /// </summary>
    public DbSet<EFMovie> Movies => Set<EFMovie>();

    /// <summary>
    /// The set of kitchen sink values
    /// </summary>
    public DbSet<KitchenSink> KitchenSinks => Set<KitchenSink>();

    /// <summary>
    /// Get a cloned copy of the movie by ID, or null if it doesn't exist.
    /// </summary>
    /// <param name="id">The ID of the movie</param>
    /// <returns>The movie</returns>
    public EFMovie GetMovieById(string id)
        => Movies.SingleOrDefault(x => x.Id == id)?.Clone();

    /// <summary>
    /// Mark a set of movies as deleted.
    /// </summary>
    /// <param name="func">The function identifying the movies to be deleted.</param>
    /// <returns></returns>
    public async Task SoftDeleteMoviesAsync(Expression<Func<EFMovie, bool>> func)
    {
        await Movies.Where(func).ForEachAsync(movie => movie.Deleted = true).ConfigureAwait(true);
        await SaveChangesAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Initializes the current database
    /// </summary>
    public void InitializeDatabase()
    {
        bool created = Database.EnsureCreated();
        if (created && Database.IsSqlite())
        {
            this.EnableSqliteExtensions();
        }

        var seedData = TestData.Movies.OfType<EFMovie>().ToArray();
        Movies.AddRange(seedData);
        SaveChanges();
    }

    /// <summary>
    /// Configures the current database to handle models.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        if (Database.IsSqlite())
        {
            builder.EnableSqliteExtensions();
        }
    }
}
