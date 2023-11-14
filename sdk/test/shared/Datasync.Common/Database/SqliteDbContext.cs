// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Database;
using Microsoft.AspNetCore.Datasync;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit.Abstractions;

namespace Datasync.Common;

[ExcludeFromCodeCoverage]
public class SqliteDbContext : BaseDbContext<SqliteDbContext, SqliteEntityMovie>
{
    public static SqliteDbContext CreateContext(ITestOutputHelper output = null)
    {
        SqliteConnection connection = new("Data Source=:memory:");
        connection.Open();
        DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>()
            .UseSqlite(connection)
            .EnableLogging(output);
        SqliteDbContext context = new(optionsBuilder.Options) { Connection = connection };

        context.InitializeDatabase();
        context.PopulateDatabase();
        return context;
    }

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
    }

    public SqliteConnection Connection { get; set; }

    internal void InitializeDatabase()
    {
        Database.EnsureCreated();
        ExecuteRawSqlOnEachEntity(@"DELETE FROM ""{0}""");
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
        modelBuilder.Entity<SqliteEntityMovie>()
            .Property(m => m.UpdatedAt).HasConversion(new SqliteDateTimeOffsetConverter());
        base.OnModelCreating(modelBuilder);
    }

    internal class SqliteDateTimeOffsetConverter : ValueConverter<DateTimeOffset, long>
    {
        public SqliteDateTimeOffsetConverter() : base(v => v.ToUnixTimeMilliseconds(), v => DateTimeOffset.FromUnixTimeMilliseconds(v))
        {
        }
    }
}
