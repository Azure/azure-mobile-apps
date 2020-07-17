using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Mobile.Common.Test
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
            var context = new MovieDbContext(options);
            context.AddRange(TestData.AsList());
            context.SaveChanges();
            return context;
        }
    }
}
