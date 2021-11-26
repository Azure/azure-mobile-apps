// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Integration.Test.Helpers
{
    /// <summary>
    /// A <see cref="DbContext"/> for serving the movies set up for tests.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// The set of movies.
        /// </summary>
        public DbSet<EFMovie> Movies => Set<EFMovie>();

        /// <summary>
        /// Get a cloned copy of the movie by ID, or null if it doesn't exist.
        /// </summary>
        /// <param name="id">The ID of the movie</param>
        /// <returns>The movie</returns>
        public EFMovie? GetMovieById(string id)
            => Movies.SingleOrDefault(x => x.Id == id)?.Clone();

        /// <summary>
        /// Mark a set of movies as deleted.
        /// </summary>
        /// <param name="func">The function identifying the movies to be deleted.</param>
        /// <returns></returns>
        public async Task SoftDeleteMovieAsync(Expression<Func<EFMovie, bool>> func)
        {
            await Movies.Where(func).ForEachAsync(movie => movie.Deleted = true).ConfigureAwait(true);
            await SaveChangesAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Initializes the current database
        /// </summary>
        public void InitializeDatabase()
        {
            // Set up the database
            Database.EnsureCreated();
            InstallTriggers();

            // Seed data
            var seedData = TestData.Movies.OfType<EFMovie>().ToArray();
            Movies.AddRange(seedData);
            SaveChanges();
        }

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

                foreach (var field in props)
                {
                    _ = Database.ExecuteSqlRaw(GetTriggerSqlCommand(tableName!, field.Name));
                }
            }
        }
    }

    /// <summary>
    /// ValueConverter for Sqlite to support the [Timestamp] type.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    internal class SqliteTimestampConverter : ValueConverter<byte[]?, string?>
    {
        public SqliteTimestampConverter() : base(
            v => v == null ? null : Encoding.ASCII.GetString(v),
            v => v == null ? null : Encoding.ASCII.GetBytes(v))
        {
        }
    }
}
