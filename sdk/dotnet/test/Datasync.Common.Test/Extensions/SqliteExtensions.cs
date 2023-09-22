// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace Datasync.Common.Test.Extensions;

/// <summary>
/// A set of extension methods for enabling model support for SQLite.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class SqliteExtensions
{
    /// <summary>
    /// Called immediately after the database is created (via EnsureCreated) to
    /// execute any additional SQL statements.
    /// </summary>
    /// <param name="context">The database context</param>
    internal static void EnableSqliteExtensions(this DbContext context)
    {
        foreach (var table in context.Model.GetEntityTypes())
        {
            var props = table.GetProperties().Where(prop => prop.ClrType == typeof(byte[]) && prop.ValueGenerated == ValueGenerated.OnAddOrUpdate);
            foreach (var prop in props)
            {
                context.InstallUpdateTrigger(table.GetTableName()!, prop.Name, "UPDATE");
            }
        }
    }

    internal static void InstallUpdateTrigger(this DbContext context, string table, string field, string op)
    {
        var sql = $@"
                CREATE TRIGGER s_{table}_{field}_{op} AFTER {op} ON {table}
                BEGIN
                    UPDATE {table}
                    SET {field} = STRFTIME('%Y-%m-%d %H:%M:%f', 'NOW')
                    WHERE rowid = NEW.rowid;
                END
            ";
        context.Database.ExecuteSqlRaw(sql);
    }

    /// <summary>
    /// Called during the database creation process to enable any field level support.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    internal static void EnableSqliteExtensions(this ModelBuilder builder)
    {
        var timestampProperties = builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(byte[]) && p.ValueGenerated == ValueGenerated.OnAddOrUpdate);
        foreach (var property in timestampProperties)
        {
            property.SetValueConverter(new SqliteTimestampConverter());
            property.SetDefaultValueSql("STRFTIME('%Y-%m-%d %H:%M:%f', 'NOW')");
        }
    }
}

[ExcludeFromCodeCoverage]
internal class SqliteTimestampConverter : ValueConverter<byte[], string>
{
    public SqliteTimestampConverter() : base(v => ToDb(v), v => FromDb(v))
    {
    }

    public static string ToDb(byte[] v) => Encoding.UTF8.GetString(v);
    public static byte[] FromDb(string v) => Encoding.UTF8.GetBytes(v);
}
