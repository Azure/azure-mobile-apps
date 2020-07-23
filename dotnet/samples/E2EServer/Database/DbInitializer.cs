using E2EServer.DataObjects;
using System;
using System.Data;
using System.Linq;

namespace E2EServer.Database
{
    public static class DbInitializer
    {
        public static void Initialize(E2EDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            if (context.Movies.Any())
            {
                SeedMovies(context);
            }

            context.SaveChanges();
        }

        public static void SeedMovies(E2EDbContext context)
        {
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
        }
    }
}
