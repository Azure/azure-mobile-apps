using Microsoft.EntityFrameworkCore;
using System;

namespace Azure.Mobile.Server.Entity.Test.Helpers
{
    public class InMemoryContext : DbContext
    {
        public InMemoryContext(DbContextOptions<InMemoryContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }

        public static InMemoryContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<InMemoryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;
            var context = new InMemoryContext(options);

            foreach (var movie in TestData.Movies)
            {
                context.Movies.Add(movie);
            }
            context.SaveChanges();

            return context;
        }
    }
}
