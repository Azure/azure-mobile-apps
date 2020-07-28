using Azure.Mobile.Server.Test.E2EServer.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Azure.Mobile.Server.Test.Helpers
{
    /// <summary>
    /// An EF Core database context for the movie database
    /// </summary>
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {

        }

        public DbSet<Movie> Movies { get; set; }

        public static MovieDbContext InMemoryContext()
        {
            var options = new DbContextOptionsBuilder<MovieDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
            var context =  new MovieDbContext(options);

            List<Movie> movies = new List<Movie>();
            for (var i = 0; i < TestData.Movies.Length; i++)
            {
                var m = new Movie()
                {
                    BestPictureWinner = TestData.Movies[i].BestPictureWinner,
                    Title = TestData.Movies[i].Title,
                    MpaaRating = TestData.Movies[i].MpaaRating,
                    ReleaseDate = TestData.Movies[i].ReleaseDate,
                    Year = TestData.Movies[i].Year
                };
                movies.Add(m);
            }
            context.Movies.AddRange(movies);
            context.SaveChanges();
            return context;
        }
    }
}
