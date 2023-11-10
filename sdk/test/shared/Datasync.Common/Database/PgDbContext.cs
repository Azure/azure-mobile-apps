// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Datasync.Common;

[ExcludeFromCodeCoverage]
public class PgDbContext : BaseDbContext<PgDbContext, EntityMovie>
{
    public static PgDbContext CreateContext(string connectionString, ITestOutputHelper output = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        DbContextOptionsBuilder<PgDbContext> optionsBuilder = new DbContextOptionsBuilder<PgDbContext>()
            .UseNpgsql(connectionString)
            .EnableLogging(output);
        PgDbContext context = new(optionsBuilder.Options);

        context.InitializeDatabase();
        context.PopulateDatabase();
        return context;
    }

    public PgDbContext(DbContextOptions<PgDbContext> options) : base(options)
    {
    }

    internal void InitializeDatabase()
    {
        const string datasyncTrigger = @"
            CREATE OR REPLACE FUNCTION {0}_datasync() RETURNS trigger AS $$
            BEGIN
                NEW.""UpdatedAt"" = NOW() AT TIME ZONE 'UTC';
                NEW.""Version"" = convert_to(gen_random_uuid()::text, 'UTF8');
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE TRIGGER
                {0}_datasync
            BEFORE INSERT OR UPDATE ON
                ""{0}""
            FOR EACH ROW EXECUTE PROCEDURE
                {0}_datasync();
        ";

        Database.EnsureCreated();
        ExecuteRawSqlOnEachEntity(@"DELETE FROM ""{0}""");
        ExecuteRawSqlOnEachEntity(datasyncTrigger);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EntityMovie>()
            .Property(m => m.UpdatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        base.OnModelCreating(modelBuilder);
    }
}
