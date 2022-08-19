using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking; 

namespace CosmosTodoService.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// The dataset for the TodoItems.
        /// </summary>
        public DbSet<TodoItem> TodoItems => Set<TodoItem>();

        /// <summary>
        /// Do any database initialization required.
        /// </summary>
        /// <returns>A task that completes when the database is initialized</returns>
        public async Task InitializeDatabaseAsync()
        {
            await this.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TodoItem>().Property(model => model.EntityTag).IsETagConcurrency();
            base.OnModelCreating(builder);
        }
    }
}