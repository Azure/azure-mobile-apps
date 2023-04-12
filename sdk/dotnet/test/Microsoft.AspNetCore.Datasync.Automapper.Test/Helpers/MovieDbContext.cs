// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.AspNetCore.Datasync.Automapper.Test;

/// <summary>
/// A <see cref="DbContext"/> for serving the movies set up for tests.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test suite")]
internal class MovieDbContext : DbContext
{
    /// <summary>
    /// Creates a new <see cref="MovieDbContext"/> seeded with data
    /// </summary>
    /// <returns>The <see cref="MovieDbContext"/> that was created.</returns>
    public static MovieDbContext CreateContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<MovieDbContext>().UseSqlite(connection).Options;
        var context = new MovieDbContext(options) { Connection = connection };

        // Create the database
        context.Database.EnsureCreated();
        context.InstallTriggers();

        // Populate with test data
        var seedData = TestData.Movies.OfType<EFMovie>();
        seedData.ForEach(movie =>
        {
            var offset = -(180 + new Random().Next(180));
            movie.Version = Guid.NewGuid().ToByteArray();
            movie.UpdatedAt = DateTimeOffset.UtcNow.AddDays(offset);
        });
        context.Movies.AddRange(seedData);
        context.SaveChanges();
        return context;
    }

    public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
    {
    }

    // This is for storing the explicit connection used for SQLite.
    // It's used for testing error handling, by prematurely closing the connection
    public SqliteConnection Connection { get; set; }

    /// <summary>
    /// The set of movies.
    /// </summary>
    public DbSet<EFMovie> Movies => Set<EFMovie>();

    /// <summary>
    /// Get a cloned copy of the movie by ID, or null if it doesn't exist.
    /// </summary>
    /// <param name="id">The ID of the movie</param>
    /// <returns>The movie</returns>
    public EFMovie GetMovieById(string id)
        => Movies.SingleOrDefault(x => x.Id == id)?.Clone();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Sqlite does not support [Timestamp] attribute, which is required by the EntityTableRepository.
        // So we must fake it out and set up our own handling for version updates.
        // If you use Sqlite in your own code as an actual store, you will need to do this too
        var props = builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => IsTimestampField((IProperty)p));
        foreach (var prop in props)
        {
            prop.SetValueConverter(new SqliteTimestampConverter());
            // We use the STRFTIME ISO8601 timestamp so that we can get ms precision.
            // Using CURRENT_TIMESTAMP does not have the required resolution for rapid tests.
            prop.SetDefaultValueSql("STRFTIME('%Y%m%dT%H%M%f', 'NOW')");
        }
    }

    /// <summary>
    /// Determines if a particular property is a [Timestamp] property.
    /// </summary>
    /// <param name="p">The property</param>
    /// <returns>true if the property is decorated as [Timestamp]</returns>
    private static bool IsTimestampField(IProperty p)
        => p.ClrType == typeof(byte[]) && p.ValueGenerated == ValueGenerated.OnAddOrUpdate && p.IsConcurrencyToken;

    /// <summary>
    /// Generates the SQL comamnd necessary to install the Timestamp trigger
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <param name="fieldName">The name of the field</param>
    /// <returns>The SQL command</returns>
    private static string GetTriggerSqlCommand(string tableName, string fieldName)
        => $@"
            CREATE TRIGGER s_{tableName}_{fieldName}_update
                AFTER UPDATE ON {tableName}
                BEGIN
                    UPDATE {tableName} SET {fieldName} = randomblob(8) WHERE rowid = NEW.rowid;
                END
            ";

    /// <summary>
    /// Installs the triggers necessary to run the database, specifically around the [Timestamp]
    /// requirements.
    /// </summary>
    internal void InstallTriggers()
    {
        var tables = Model.GetEntityTypes();
        foreach (var table in tables)
        {
            var props = table.GetProperties().Where(p => IsTimestampField(p));
            var tableName = table.GetTableName();

            if (tableName != null)
            {
                foreach (var field in props)
                {
                    _ = Database.ExecuteSqlRaw(GetTriggerSqlCommand(tableName, field.Name));
                }
            }
        }
    }
}

/// <summary>
/// ValueConverter for Sqlite to support the [Timestamp] type.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test suite")]
internal class SqliteTimestampConverter : ValueConverter<byte[], string>
{
    public SqliteTimestampConverter() : base(
        v => v == null ? null : Encoding.ASCII.GetString(v),
        v => v == null ? null : Encoding.ASCII.GetBytes(v))
    {
    }
}
