// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit.Abstractions;
using TestData = Datasync.Common.TestData;

namespace Microsoft.AspNetCore.Datasync.Automapper.Tests;

[ExcludeFromCodeCoverage]
public class TestDbContext : DbContext
{
    public static TestDbContext CreateContext(ITestOutputHelper output)
    {

        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseLoggerFactory(new TestLoggerFactory(output, new string[] { "DbLoggerCategory.Database.Command.Name" }))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .UseSqlite(connection)
            .Options;
        var context = new TestDbContext(options) { Connection = connection };
        context.Database.EnsureCreated();

        Random random = new();
        TestData.Movies.OfType<EntityMovie>().ForEach(movie =>
        {
            movie.Version = Guid.NewGuid().ToByteArray();
            movie.UpdatedAt = DateTimeOffset.UtcNow.AddDays(random.Next(180) * -1);
            context.Movies.Add(movie);
        });
        context.SaveChanges();
        return context;
    }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public SqliteConnection Connection { get; set; }

    public DbSet<EntityMovie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite stores date/times with second resolution, which isn't high enough for Azure Mobile Apps.
        modelBuilder.Entity<EntityMovie>().Property(m => m.UpdatedAt).HasConversion(new SqliteDateTimeOffsetConverter());
        base.OnModelCreating(modelBuilder);
    }

    internal class SqliteDateTimeOffsetConverter : ValueConverter<DateTimeOffset, long>
    {
        public SqliteDateTimeOffsetConverter() : base(v => v.ToUnixTimeMilliseconds(), v => DateTimeOffset.FromUnixTimeMilliseconds(v))
        {
        }
    }
}
