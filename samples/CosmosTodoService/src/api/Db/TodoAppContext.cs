using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.Azure.Cosmos;
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
        builder.Entity<TodoItem>().Property(model => model.EntityTag).IsETagConcurrency();

        builder.Entity<TodoItem>(entity =>
        {
            entity.HasPartitionKey(x => x.Id);
            entity.Property(x => x.EntityTag).IsETagConcurrency();
        });

        base.OnModelCreating(builder);
    }

    public async Task FixIndex()
    {
        var cosmosClient = Database.GetCosmosClient();
        var databaseId = Database.GetCosmosDatabaseId();
        const string containerId = nameof(TodoAppContext);

        _logger.LogInformation("Fix index settings for container {ContainerId}", containerId);
        var container = cosmosClient.GetContainer(databaseId, containerId);
        // fetch current container properties
        var containerResponse = await container.ReadContainerAsync();
        var containerProperties = containerResponse.Resource;

        // add composite index
        containerProperties.IndexingPolicy.CompositeIndexes.Add(new Collection<CompositePath>
        {
            new() {Path = "/" + nameof(ETagEntityTableData.UpdatedAt), Order = CompositePathSortOrder.Ascending},
            new() {Path = "/" + nameof(ETagEntityTableData.Id), Order = CompositePathSortOrder.Ascending},
        });

        // replace container properties with the new settings
        await container.ReplaceContainerAsync(containerProperties);
        _logger.LogInformation("Index settings for container {ContainerId} set", containerId);
    }
}