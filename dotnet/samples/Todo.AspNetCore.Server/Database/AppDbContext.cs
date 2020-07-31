using Microsoft.EntityFrameworkCore;

namespace Todo.AspNetCore.Server.Database
{
    /// <summary>
    /// The Entity Framework Core database context.
    /// </summary>
    /// <seealso cref="DbContext"/>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<TodoItemDTO> TodoItems { get; set; }
    }
}
