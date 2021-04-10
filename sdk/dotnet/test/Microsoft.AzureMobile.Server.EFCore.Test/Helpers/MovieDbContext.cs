// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.AzureMobile.Server.EFCore.Test.Helpers
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
        }

        // This is for storing the explicit connection used for SQLite.
        // It's used for testing error handling, by prematurely closing the connection
        public SqliteConnection Connection { get; set; }

        public DbSet<EntityMovie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (Database.IsSqlite())
            {
                // Sqlite does not support [Timestamp] attribute, which is required by the EntityTableRepository.
                // So we must fake it out and set up our own handling for version updates.
                // If you use Sqlite in your own code as an actual store, you will need to do this too
                var props = builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => IsTimestampField(p));
                foreach (var prop in props)
                {
                    prop.SetValueConverter(new SqliteTimestampConverter());
                    // We use the STRFTIME ISO8601 timestamp so that we can get ms precision.
                    // Using CURRENT_TIMESTAMP does not have the required resolution for rapid tests.
                    prop.SetDefaultValueSql("STRFTIME('%Y%m%dT%H%M%f', 'NOW')");
                }
            }
        }

        /// <summary>
        /// Determines if a particular property is a [Timestamp] property.
        /// </summary>
        /// <param name="t">The property</param>
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
                    UPDATE {tableName} 
                    SET {fieldName} = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ";

        /// <summary>
        /// Installs the triggers necessary to run the database, specifically around the [Timestamp]
        /// requirements.
        /// </summary>
        internal void InstallTriggers()
        {
            if (Database.IsSqlite())
            {
                var tables = Model.GetEntityTypes();
                foreach (var table in tables)
                {
                    var props = table.GetProperties().Where(p => IsTimestampField(p));
                    var tableName = table.GetTableName();

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
        public SqliteTimestampConverter() : base(v => v == null ? null : ToDb(v), v => v == null ? null : FromDb(v))
        {
        }

        private static byte[] FromDb(string v) => Encoding.ASCII.GetBytes(v);

        private static string ToDb(byte[] v) => Encoding.ASCII.GetString(v);
    }
}
