// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Integration.Test.Helpers
{
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
        /// Get a cloned copy of the movie by ID, or null if it doesn't exist.
        /// </summary>
        /// <param name="id">The ID of the movie</param>
        /// <returns>The movie</returns>
        public EFMovie? GetMovieById(string id)
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
            Database.EnsureCreated();

            var seedData = TestData.Movies.OfType<EFMovie>().ToArray();
            Movies.AddRange(seedData);
            SaveChanges();
        }
    }
}
