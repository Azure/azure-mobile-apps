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

    public PgDbContext(DbContextOptions<PgDbContext> options) : base(options)
    {
    }

    public DbSet<EntityMovie> Movies { get; set; }

    internal void InstallDatasyncTriggers()
    {
        const string trigger = @"
            CREATE OR REPLACE FUNCTION movies_datasync() RETURNS trigger AS $$
            BEGIN
                NEW.""UpdatedAt"" = NOW() AT TIME ZONE 'UTC';
                NEW.""Version"" = convert_to(gen_random_uuid()::text, 'UTF8');
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE TRIGGER
                movies_datasync
            BEFORE INSERT OR UPDATE ON
                ""Movies""
            FOR EACH ROW EXECUTE PROCEDURE
                movies_datasync();
        ";
        Database.ExecuteSqlRaw(trigger);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityMovie>().Property(m => m.UpdatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        base.OnModelCreating(modelBuilder);
    }
}
