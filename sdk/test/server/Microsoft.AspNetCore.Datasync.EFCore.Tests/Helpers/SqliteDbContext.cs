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
            optionsBuilder.UseLoggerFactory(new TestLoggerFactory(output, new string[] { "DbLoggerCategory.Database.Command.Name" }));
            optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
        }
        SqliteDbContext context = new(optionsBuilder.Options) { Connection = connection };
        context.Database.EnsureCreated();

        Random random = new();
        TestData.Movies.OfType<SqliteEntityMovie>().ForEach(movie =>
        {
            movie.Version = Guid.NewGuid().ToByteArray();
            movie.UpdatedAt = DateTimeOffset.UtcNow.AddDays(random.Next(180) * -1);
            context.Movies.Add(movie);
        });
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

    internal class SqliteDateTimeOffsetConverter : ValueConverter<DateTimeOffset, long>
    {
        public SqliteDateTimeOffsetConverter() : base(v => v.ToUnixTimeMilliseconds(), v => DateTimeOffset.FromUnixTimeMilliseconds(v))
        {
        }
    }
}
