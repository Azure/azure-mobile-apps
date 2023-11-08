// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit.Abstractions;
using TestData = Datasync.Common.TestData;

namespace Microsoft.AspNetCore.Datasync.Automapper.Tests;

[ExcludeFromCodeCoverage]
public class SqliteDbContext : DbContext
{
    public static SqliteDbContext CreateContext(ITestOutputHelper output = null)
    {

        SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>().UseSqlite(connection);
        if (output != null)
        {
            optionsBuilder.UseLoggerFactory(new TestLoggerFactory(output, new string[] { "DbLoggerCategory.Database.Command.Name" }));
            optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
        }
        SqliteDbContext context = new(optionsBuilder.Options) { Connection = connection };
        context.Database.EnsureCreated();

        Random random = new();
        TestData.Movies.OfType<SqliteEntityMovie>().ForEach(movie => context.Movies.Add(movie));
        context.SaveChanges();
        return context;
    }

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
    }

    public SqliteConnection Connection { get; set; }

    public DbSet<SqliteEntityMovie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite stores date/times with second resolution, which isn't high enough for Azure Mobile Apps.
        modelBuilder.Entity<SqliteEntityMovie>().Property(m => m.UpdatedAt).HasConversion(new SqliteDateTimeOffsetConverter());
        base.OnModelCreating(modelBuilder);
    }

    #region SaveChanges
    public override int SaveChanges()
    {
        UpdateTrackedEntities();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTrackedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTrackedEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTrackedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Updated the metadata of the entities on save.
    /// </summary>
    protected void UpdateTrackedEntities()
    {
        ChangeTracker.DetectChanges();
        foreach (object entity in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified).Select(t => t.Entity).ToList())
        {
            if (entity is ITableData metadata)
            {
                metadata.UpdatedAt = DateTimeOffset.UtcNow;
                metadata.Version = Guid.NewGuid().ToByteArray();
            }
        }
    }
    #endregion

    internal class SqliteDateTimeOffsetConverter : ValueConverter<DateTimeOffset, long>
    {
        public SqliteDateTimeOffsetConverter() : base(v => v.ToUnixTimeMilliseconds(), v => DateTimeOffset.FromUnixTimeMilliseconds(v))
        {
        }
    }
}
