// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit.Abstractions;

namespace Datasync.Common;

public class CosmosDbContext : BaseDbContext<CosmosDbContext, CosmosEntityMovie>
{
    public static CosmosDbContext CreateContext(string connectionString, ITestOutputHelper output = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        DbContextOptionsBuilder<CosmosDbContext> optionsBuilder = new DbContextOptionsBuilder<CosmosDbContext>()
            .UseCosmos(connectionString, databaseName: "unittests")
            .EnableLogging(output);
        CosmosDbContext context = new(optionsBuilder.Options);

        context.InitializeDatabase();
        context.PopulateDatabase();
        return context;
    }

    public CosmosDbContext(DbContextOptions<CosmosDbContext> options) : base(options)
    {
    }

    internal void InitializeDatabase()
    {
        Database.EnsureCreated();

        // Cosmos doesn't have a good way to delete all items, so let's get the list of IDs
        // and remove them one by one.
        RemoveRange(Movies.ToList());
        SaveChanges();
    }

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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    internal class DateOnlyConverter : ValueConverter<DateOnly, string>
    {
        private const string format = "yyyy-MM-dd";
        public DateOnlyConverter() : base(d => d.ToString(format), d => DateOnly.ParseExact(d, format))
        {
        }
    }
}
