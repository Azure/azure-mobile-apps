using E2EServer.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace E2EServer.Database
{
    public class E2EDbContext : DbContext
    {
        public E2EDbContext(DbContextOptions<E2EDbContext> options): base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<RMovie> RMovies { get; set; }
    }
}
