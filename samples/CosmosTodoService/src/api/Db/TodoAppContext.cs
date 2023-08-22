using Microsoft.EntityFrameworkCore;

namespace api.Db;

public class TodoAppContext : DbContext
{
    private readonly ILogger<TodoAppContext> _logger;
    public TodoAppContext(DbContextOptions<TodoAppContext> options, ILogger<TodoAppContext> logger) : base(options)
    {
        _logger = logger;
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
        await Database.EnsureCreatedAsync().ConfigureAwait(false);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<TodoItem>(builder =>
        {
            builder.ToContainer("TodoItems");
            builder.HasNoDiscriminator();
            builder.HasPartitionKey(model => model.Id);
            builder.Property(model => model.EntityTag).IsETagConcurrency();
        });
        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
        base.OnConfiguring(optionsBuilder);
    }
}