// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using TestData = Datasync.Common.TestData;

namespace Microsoft.AspNetCore.Datasync.EFCore.Tests;

[ExcludeFromCodeCoverage]
public class PgDbContext : DbContext
{
    public static PgDbContext CreateContext(string connectionString, ITestOutputHelper output = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        DbContextOptionsBuilder<PgDbContext> optionsBuilder = new DbContextOptionsBuilder<PgDbContext>().UseNpgsql(connectionString);
        if (output != null)
        {
            optionsBuilder.UseLoggerFactory(new TestLoggerFactory(output, new string[] { "Microsoft.EntityFrameworkCore.Database.Command" }));
            optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
        }
        PgDbContext context = new(optionsBuilder.Options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        TestData.Movies.OfType<PgEntityMovie>().ForEach(movie => context.Movies.Add(movie));
        context.SaveChanges();
        return context;
    }

    public PgDbContext(DbContextOptions<PgDbContext> options) : base(options)
    {
    }

    public DbSet<PgEntityMovie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PgEntityMovie>().Property(m => m.UpdatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        base.OnModelCreating(modelBuilder);
    }
}
