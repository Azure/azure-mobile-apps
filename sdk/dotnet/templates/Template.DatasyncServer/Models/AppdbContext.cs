using Microsoft.EntityFrameworkCore;

namespace Template.DatasyncServer.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<TodoItem> TodoItems { get; set; }
    }
}