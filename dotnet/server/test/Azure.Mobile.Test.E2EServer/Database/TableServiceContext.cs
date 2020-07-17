using Azure.Mobile.Test.E2EServer.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Azure.Mobile.Test.E2EServer.Database
{
    public class TableServiceContext : DbContext
    {
        public TableServiceContext(DbContextOptions<TableServiceContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<OfflineReady> OfflineReadyItems { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
    }
}
