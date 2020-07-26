using E2EServer.DataObjects;
using System;

namespace E2EServer.Database
{
    public static class DbInitializer
    {
        public static void Initialize(E2EDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SeedMovies(context);
            SeedRMovies(context);
            SeedSUnits(context);
            SeedHUnits(context);

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

        // RMovies == Movies, but all R movies are soft-deleted
        public static void SeedRMovies(E2EDbContext context)
        {
            for (var i = 0; i < TestData.Movies.Length; i++)
            {
                var offset = 180 + (new Random()).Next(180);
                var movie = new RMovie()
                {
                    Id = $"rmovie-{i}",
                    Version = Guid.NewGuid().ToByteArray(),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-offset),
                    Deleted = (TestData.Movies[i].MpaaRating == "R"),
                    Title = TestData.Movies[i].Title,
                    Duration = TestData.Movies[i].Duration,
                    MpaaRating = TestData.Movies[i].MpaaRating,
                    ReleaseDate = TestData.Movies[i].ReleaseDate,
                    BestPictureWinner = TestData.Movies[i].BestPictureWinner,
                    Year = TestData.Movies[i].Year
                };
                context.RMovies.Add(movie);
            }
        }

        // SUnits for soft-delete, HUnits for hard-delete
        public static void SeedSUnits(E2EDbContext context)
        {
            for (var i = 0; i < 20; i++)
            {
                var offset = 180 + (new Random()).Next(180);
                var sunit = new SUnit
                {
                    Id = $"sunit-{i}",
                    Version = Guid.NewGuid().ToByteArray(),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-offset),
                    Deleted = false,
                    Data = $"sunit-{i}"
                };
                context.SUnits.Add(sunit);
            }
        }

        public static void SeedHUnits(E2EDbContext context)
        {
            for (var i = 0; i < 20; i++)
            {
                var offset = 180 + (new Random()).Next(180);
                var hunit = new HUnit
                {
                    Id = $"hunit-{i}",
                    Version = Guid.NewGuid().ToByteArray(),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-offset),
                    Deleted = false,
                    Data = $"hunit-{i}"
                };
                context.HUnits.Add(hunit);
            }
        }
    }
}
