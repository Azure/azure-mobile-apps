// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit.Abstractions;
using TestData = Datasync.Common.TestData;

namespace Microsoft.AspNetCore.Datasync.EFCore.Tests;

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
            optionsBuilder.UseLoggerFactory(new TestLoggerFactory(output, new string[] { "Microsoft.EntityFrameworkCore.Database.Command" }));
            optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
        }
        SqliteDbContext context = new(optionsBuilder.Options) { Connection = connection };
        context.Database.EnsureCreated();

        TestData.Movies.OfType<SqliteEntityMovie>().ForEach(movie => context.Movies.Add(movie));
        context.SaveChanges();
        return context;
    }

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
    }

    public SqliteConnection Connection { get; set; }

    public DbSet<SqliteEntityMovie> Movies { get; set; }

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
    #endregion

    internal void UpdateTrackedEntities()
    {
        ChangeTracker.DetectChanges();
        foreach (var change in ChangeTracker.Entries().Where(m => m.State == EntityState.Added || m.State == EntityState.Modified))
        {
            if (change.Entity is ITableData movie)
            {
                movie.UpdatedAt = DateTimeOffset.UtcNow;
                movie.Version = Guid.NewGuid().ToByteArray();
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SqliteEntityMovie>().Property(m => m.UpdatedAt).HasConversion(new SqliteDateTimeOffsetConverter());
        base.OnModelCreating(modelBuilder);
    }

    internal class SqliteDateTimeOffsetConverter : ValueConverter<DateTimeOffset, long>
    {
        public SqliteDateTimeOffsetConverter() : base(v => v.ToUnixTimeMilliseconds(), v => DateTimeOffset.FromUnixTimeMilliseconds(v))
        {
        }
    }
}
