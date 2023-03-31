// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.Datasync.NSwag.Test.Service;

/// <summary>
/// A <see cref="DbContext"/> for serving the movies set up for tests.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class ServiceDbContext : DbContext
{
    public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// The set of kitchen sink values
    /// </summary>
    public DbSet<KitchenSink> KitchenSinks => Set<KitchenSink>();

    /// <summary>
    /// The set of TodoItem values
    /// </summary>
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    /// <summary>
    /// Initializes the current database
    /// </summary>
    public void InitializeDatabase()
    {
        bool created = Database.EnsureCreated();
        if (created && Database.IsSqlite())
        {
            this.EnableSqliteExtensions();
        }
    }

    /// <summary>
    /// Configures the current database to handle models.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        if (Database.IsSqlite())
        {
            builder.EnableSqliteExtensions();
        }
    }
}
