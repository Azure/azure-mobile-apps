// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using TestData = Datasync.Common.TestData;

namespace Microsoft.AspNetCore.Datasync.EFCore.Tests;

public class CosmosDbContext : DbContext
{
    public static CosmosDbContext CreateContext(string connectionString, ITestOutputHelper output = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        DbContextOptionsBuilder<CosmosDbContext> optionsBuilder = new DbContextOptionsBuilder<CosmosDbContext>()
            .UseCosmos(connectionString, databaseName: "unittests");
        if (output != null)
        {
            optionsBuilder.UseLoggerFactory(new TestLoggerFactory(output, new string[] { "Microsoft.EntityFrameworkCore.Database.Command" }));
            optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
        }
        CosmosDbContext context = new(optionsBuilder.Options);
        context.Database.EnsureCreated();

        TestData.Movies.OfType<CosmosEntityMovie>().ForEach(movie => context.Movies.Add(movie));
        context.SaveChanges();
        return context;
    }

    public CosmosDbContext(DbContextOptions<CosmosDbContext> options) : base(options)
    {
    }

    public DbSet<CosmosEntityMovie> Movies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CosmosEntityMovie>(builder =>
        {
            builder.ToContainer("Movies");
            builder.HasNoDiscriminator();
            builder.HasPartitionKey(model => model.Id);
            builder.Property(model => model.EntityTag).IsETagConcurrency();
        });
        base.OnModelCreating(modelBuilder);
    }
}
