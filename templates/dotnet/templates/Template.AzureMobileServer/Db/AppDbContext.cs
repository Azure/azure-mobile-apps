using Microsoft.EntityFrameworkCore;

namespace Template.AzureMobileServer.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}