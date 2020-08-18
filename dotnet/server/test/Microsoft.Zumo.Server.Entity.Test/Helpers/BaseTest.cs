// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Microsoft.Zumo.Server.Entity.Test.Helpers
{
    /// <summary>
    /// Base class for all tests within this project.
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// Obtains a reference to the test database context.
        /// </summary>
        /// <remarks>
        /// In this version, we set up a Sqlite in-memory test database that is
        /// refreshed for each test.  Note that the [Timestamp] attribute must be
        /// honored in these tests.  If it isn't, then the Version tests will fail.
        /// </remarks>
        /// <returns>A pre-configured <see cref="MovieDbContext"/> object</returns>
        internal MovieDbContext GetMovieContext()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<MovieDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new MovieDbContext(options);

            // Create the database
            context.Database.EnsureCreated();
            context.InstallTriggers();

            // Populate with test data
            for (var i = 0; i < TestData.Movies.Length; i++)
            {
                var offset = 180 + (new Random()).Next(180);
                var movie = new Movie()
                {
                    Id = $"movie-{i}",
                    Version = Guid.NewGuid().ToByteArray(),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-offset),
                    Deleted = false,
                    Title = TestData.Movies[i].Title,
                    Duration = TestData.Movies[i].Duration,
                    MpaaRating = TestData.Movies[i].MpaaRating,
                    ReleaseDate = TestData.Movies[i].ReleaseDate,
                    BestPictureWinner = TestData.Movies[i].BestPictureWinner,
                    Year = TestData.Movies[i].Year
                };
                context.Movies.Add(movie);
            }
            context.SaveChanges();

            // Finished
            return context;
        }

        /// <summary>
        /// Obtains a reference to the test table repository.  The table repository
        /// under test must be read/write and populates with a "Movies" table with
        /// the prescribed content.
        /// </summary>
        /// <returns>The pre-configured <see cref="EntityTableRepository{TEntity}"/> object</returns>
        internal EntityTableRepository<Movie> GetTableRepository()
        {
            return new EntityTableRepository<Movie>(GetMovieContext());
        }

        // Count of the movies in the dataset.
        internal int MOVIE_COUNT { get => TestData.Movies.Count(); }

        // Count of the R-rated movies in the dataset.
        internal int R_MOVIE_COUNT { get => TestData.Movies.Where(m => m.MpaaRating == "R").Count(); }
    }
}
