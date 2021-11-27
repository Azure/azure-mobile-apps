// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.Datasync.Integration.Test.Helpers.Extensions
{
    /// <summary>
    /// A set of extension methods for enabling model support for SQLite.
    /// </summary>
    internal static class SqliteExtensions
    {
        /// <summary>
        /// Called immediately after the database is created (via EnsureCreated) to
        /// execute any additional SQL statements.
        /// </summary>
        /// <param name="context">The database context</param>
        internal static void EnableSqliteExtensions(this DbContext context)
        {
            var tables = context.Model.GetEntityTypes();
            foreach (var table in tables)
            {
                Debug.WriteLine($"EnableSqliteExtensions(context): Processing table {table.Name}");
                var timestamps = table.GetProperties().Where(p => p.IsTimestamp());
                foreach (var prop in timestamps)
                {
                    Debug.WriteLine($">>> [Timestamp]: {prop.Name}");
                    context.Database.ExecuteSqlRaw($"CREATE TRIGGER s_{table.Name}_{prop.Name} AFTER UPDATE ON {table.Name} BEGIN UPDATE {table.Name} SET {prop.Name} = randomblob(8) WHERE rowid = NEW.rowid; END");
                    context.Database.ExecuteSqlRaw($"CREATE TRIGGER s_{table.Name}_{prop.Name} AFTER INSERT ON {table.Name} BEGIN UPDATE {table.Name} SET {prop.Name} = randomblob(8) WHERE rowid = NEW.rowid; END");
                }
            }
        }

        /// <summary>
        /// Called during the database creation process to enable any field level support.
        /// </summary>
        /// <param name="builder">The model builder.</param>
        internal static void EnableSqliteExtensions(this ModelBuilder builder)
        {
            var tables = builder.Model.GetEntityTypes();
            foreach (var table in tables)
            {
                Debug.WriteLine($"EnableSqliteExtensions(builder): Processing table {table.Name}");
                var timestamps = table.GetProperties().Where(prop => prop.IsTimestamp());
                foreach (var prop in timestamps)
                {
                    Debug.WriteLine($">>> [Timestamp]: {prop.Name}");
                    prop.SetColumnType("BLOB");
                }

                var dateprops = table.GetProperties().Where(prop => prop.IsDateTimeOffset());
                foreach (var prop in dateprops)
                {
                    Debug.WriteLine($">>> [DateTimeOffset]: {prop.Name}");
                    prop.SetColumnType("VARCHAR(128)");
                    prop.SetValueConverter(new SqliteDateTimeOffsetConverter());
                }
            }
        }

        /// <summary>
        /// Returns true if the property is adorned with [Timestamp]
        /// </summary>
        /// <param name="prop">The property to check</param>
        /// <returns>True if the property is a concurrency token</returns>
        internal static bool IsTimestamp(this IProperty prop)
            => prop.ClrType == typeof(byte[])
            && prop.ValueGenerated == ValueGenerated.OnAddOrUpdate
            && prop.IsConcurrencyToken;

        /// <summary>
        /// Returns true if the property is adorned with [Timestamp]
        /// </summary>
        /// <param name="prop">The property to check</param>
        /// <returns>True if the property is a concurrency token</returns>
        internal static bool IsTimestamp(this IMutableProperty prop)
            => prop.ClrType == typeof(byte[])
            && prop.ValueGenerated == ValueGenerated.OnAddOrUpdate
            && prop.IsConcurrencyToken;

        /// <summary>
        /// Returns true if the property is any type of DateTimeOffset
        /// </summary>
        /// <param name="prop">The property to check</param>
        /// <returns>True if a DateTimeOffset</returns>
        internal static bool IsDateTimeOffset(this IMutableProperty prop)
            => prop.ClrType == typeof(DateTimeOffset) || prop.ClrType == typeof(DateTimeOffset?);
    }

    /// <summary>
    /// Converts between an EF Timestamp field and the string equivalent.
    /// </summary>
    internal class SqliteTimestampConverter : ValueConverter<byte[]?, string?>
    {
        public SqliteTimestampConverter() : base(v => ToDb(v), v => FromDb(v))
        {
        }

        private static string? ToDb(byte[]? value)
            => value == null ? null : Encoding.UTF8.GetString(value);

        private static byte[]? FromDb(string? value)
            => value == null ? null : Encoding.UTF8.GetBytes(value);
    }

    /// <summary>
    /// Converts between an EF DateTimeOffset field and the Sqlite storage.
    /// </summary>
    internal class SqliteDateTimeOffsetConverter : ValueConverter<DateTimeOffset?, string?>
    {
        public SqliteDateTimeOffsetConverter() : base(v => ToDb(v), v => FromDb(v))
        {
        }

        private static string? ToDb(DateTimeOffset? value)
            => value?.ToString("o");

        private static DateTimeOffset? FromDb(string? value)
            => value == null ? null : DateTimeOffset.Parse(value);
    }
}
