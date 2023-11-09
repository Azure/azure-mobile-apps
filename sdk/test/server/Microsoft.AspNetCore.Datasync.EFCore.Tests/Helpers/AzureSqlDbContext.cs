// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit.Abstractions;
using TestData = Datasync.Common.TestData;

namespace Microsoft.AspNetCore.Datasync.EFCore.Tests;

[ExcludeFromCodeCoverage]
public class AzureSqlDbContext : DbContext
{
    public static AzureSqlDbContext CreateContext(string connectionString, ITestOutputHelper output = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        DbContextOptionsBuilder<AzureSqlDbContext> optionsBuilder = new DbContextOptionsBuilder<AzureSqlDbContext>()
            .UseSqlServer(connectionString);
        if (output != null)
        {
            optionsBuilder.UseLoggerFactory(new TestLoggerFactory(output, new string[] { "Microsoft.EntityFrameworkCore.Database.Command" }));
            optionsBuilder.EnableSensitiveDataLogging().EnableDetailedErrors();
        }
        AzureSqlDbContext context = new(optionsBuilder.Options);
        context.Database.EnsureCreated();

        // Entity Framework Core does not support the update of DateTimeOffset properties.
        // We need to add a specific trigger to update the UpdatedAt property.
        context.InstallDatasyncTriggers();

        // Remove all items from the Movies table.
        context.Database.ExecuteSqlRaw("DELETE FROM \"Movies\"");

        TestData.Movies.OfType<EntityMovie>().ForEach(movie => context.Movies.Add(movie));
        context.SaveChanges();
        return context;
    }

    public AzureSqlDbContext(DbContextOptions<AzureSqlDbContext> options) : base(options)
    {
    }

    public DbSet<EntityMovie> Movies { get; set; }

    /// <summary>
    /// Updates the schema for the database to include the triggers needed for Datasync.
    /// </summary>
    internal void InstallDatasyncTriggers()
    {
        const string trigger = @"
            CREATE OR ALTER TRIGGER [dbo].[Movies_UpdatedAt] ON [dbo].[Movies]
                AFTER INSERT, UPDATE
            AS
            BEGIN
                SET NOCOUNT ON;
                UPDATE [dbo].[Movies] SET [UpdatedAt] = GETUTCDATE() WHERE [Id] IN (SELECT [Id] FROM INSERTED);
            END
        ";
        Database.ExecuteSqlRaw(trigger);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateOnly>().HaveConversion<DateOnlyConverter>().HaveColumnType("date");
        base.ConfigureConventions(configurationBuilder);
    }

    internal class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
    {
        public DateOnlyConverter() : base(d => d.ToDateTime(TimeOnly.MinValue), d => DateOnly.FromDateTime(d))
        {
        }
    }
}
