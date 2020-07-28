using Azure.Mobile.Server.Test.E2EServer.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Azure.Mobile.Server.Test.E2EServer.Database
{
    public class E2EDbContext : DbContext
    {
        public E2EDbContext(DbContextOptions<E2EDbContext> options) : base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }

        // Datasets for List and Get unit tests
        public DbSet<Movie> Movies { get; set; }
        public DbSet<RMovie> RMovies { get; set; }

        // Datasets for unit tests
        public DbSet<Unit> Units { get; set; }

        // Datasets for Create, Patch, Replace & Delete unit tests
        public DbSet<SUnit> SUnits { get; set; }
        public DbSet<HUnit> HUnits { get; set; }
    }
}
