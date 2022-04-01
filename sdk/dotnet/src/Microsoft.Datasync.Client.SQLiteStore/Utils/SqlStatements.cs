// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Datasync.Client.SQLiteStore.Utils
{
    /// <summary>
    /// A set of helper methods to build the appropriate SQL statements.
    /// </summary>
    internal static class SqlStatements
    {
        /// <summary>
        /// Produces a <c>ALTER TABLE ADD COLUMN</c> SQL statement.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="column">The column definition.</param>
        /// <returns>The SQL statement.</returns>
        internal static string AddColumnToTable(string tableName, ColumnDefinition column)
            => $"ALTER TABLE {FormatTableName(tableName)} ADD COLUMN {FormatColumnName(column.Name)} {column.StoreType}";

        /// <summary>
        /// Creates a <c>INSERT INTO {table}</c> SQL statement.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="items">The items to insert.</param>
        /// <param name="parameters">On completion, a list of parameters for the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        internal static string BatchInsert(string tableName, List<ColumnDefinition> columns, IEnumerable<JObject> items, out Dictionary<string, object> parameters)
        {
            var columnDefns = string.Join(", ", columns.Select(c => FormatColumnName(c.Name)));
            var insertParams = new Dictionary<string, object>();
            StringBuilder sql = new($"INSERT OR IGNORE INTO {FormatTableName(tableName)} ({columnDefns}) VALUES ");
            foreach (JObject item in items)
            {
                var itemColumns = columns.Select(c => AddParameter(item, c, insertParams));
                sql.Append('(').Append(string.Join(", ", itemColumns)).Append("),");
            }

            if (insertParams.Count > 0)
            {
                sql.Remove(sql.Length - 1, 1); // Remove the trailing comma
                parameters = insertParams;
                return sql.ToString();
            }

            parameters = null;
            return null;
        }

        /// <summary>
        /// Creates a <c>UPDATE {table} SET WHERE</c> SQL statement.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="item">The item to update.</param>
        /// <param name="parameters">On completion, a list of parameters for the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        internal static string BatchUpdate(string tableName, List<ColumnDefinition> columns, JObject item, out Dictionary<string, object> parameters)
        {
            StringBuilder sql = new($"UPDATE {FormatTableName(tableName)} SET ");
            var updateParams = new Dictionary<string, object>();

            ColumnDefinition idColumn = columns.Find(c => c.IsIdColumn);
            if (idColumn == null)
            {
                parameters = null;
                return null;
            }

            List<string> updates = new();
            foreach (var column in columns.Where(c => !c.IsIdColumn))
            {
                string paramName = AddParameter(item, column, updateParams);
                updates.Add($"{FormatColumnName(column.Name)} = {paramName}");
            }
            sql.Append(string.Join(",", updates));
            sql.Append(" WHERE ").Append(FormatColumnName(idColumn.Name)).Append(" = ").Append(AddParameter(item, idColumn, updateParams));

            parameters = updateParams;
            return sql.ToString();
        }

        /// <summary>
        /// Produces a <c>DELETE COUNT(*) FROM {table} WHERE {query}</c> SQL statement.
        /// </summary>
        /// <param name="query">The query to use.</param>
        /// <param name="parameters">On completion, a list of parameters to use with the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        internal static string CountFromTable(QueryDescription query, out Dictionary<string, object> parameters)
            => SqlQueryFormatter.FormatCountStatement(query, out parameters);

        /// <summary>
        /// Produces a <c>CREATE TABLE IF NOT EXISTS</c> SQL statement.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="primaryKey">The column definition of the primary key</param>
        /// <param name="otherColumns">The column definitions of the other columns.</param>
        /// <returns>The SQL statement.</returns>
        internal static string CreateTableFromColumns(string tableName, ColumnDefinition primaryKey, IEnumerable<ColumnDefinition> otherColumns)
        {
            var columnDefinitions = otherColumns.Select(c => $"{FormatColumnName(c.Name)} {c.StoreType}").ToList();
            columnDefinitions.Insert(0, $"{FormatColumnName(primaryKey.Name)} {primaryKey.StoreType} PRIMARY KEY");
            return $"CREATE TABLE IF NOT EXISTS {FormatTableName(tableName)} ({string.Join(", ", columnDefinitions)})";
        }

        /// <summary>
        /// Produces a <c>DELETE FROM {table} WHERE {query}</c> SQL statement.
        /// </summary>
        /// <param name="query">The query to use.</param>
        /// <param name="parameters">On completion, a list of parameters to use with the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        internal static string DeleteFromTable(QueryDescription query, out Dictionary<string, object> parameters)
            => SqlQueryFormatter.FormatDeleteStatement(query, out parameters);

        /// <summary>
        /// Produces a <c>DELETE FROM {table} WHERE {column} IN ({values})</c> SQL statement.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="values">The list of value parameter names to delete.</param>
        /// <returns>The SQL statement.</returns>
        internal static string DeleteIdsFromTable(string tableName, string columnName, IEnumerable<string> values)
            => $"DELETE FROM {FormatTableName(tableName)} WHERE {FormatColumnName(columnName)} IN ({string.Join(",", values)})";

        /// <summary>
        /// Produces a <c>ALTER TABLE DROP COLUMN</c> SQL statement.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The column name.</param>
        /// <returns>The SQL statement.</returns>
        internal static string DropColumnFromTable(string tableName, string columnName)
            => $"ALTER TABLE {FormatTableName(tableName)} DROP COLUMN {FormatColumnName(columnName)}";

        /// <summary>
        /// Produces a <c>SELECT * FROM {table} WHERE {column} = {value}</c> SQL statement.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="value">The value of the column.</param>
        /// <returns>The SQL statement.</returns>
        internal static string GetItemById(string tableName, string columnName, string value)
            => $"SELECT * FROM {FormatTableName(tableName)} WHERE {FormatColumnName(columnName)} = {value}";

        /// <summary>
        /// Produces a <c>PRAGMA table_info({table})</c> SQL statement to get information on the columns.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The SQL statement.</returns>
        internal static string GetTableInformation(string tableName)
            => $"PRAGMA table_info({FormatTableName(tableName)})";

        /// <summary>
        /// Produces a <c>SELECT * FROM {table} WHERE {query}</c> SQL statement.
        /// </summary>
        /// <param name="query">The query to use.</param>
        /// <param name="parameters">On completion, a list of parameters to use with the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        internal static string SelectFromTable(QueryDescription query, out Dictionary<string, object> parameters)
            => SqlQueryFormatter.FormatSelectStatement(query, out parameters);

        #region Helpers
        /// <summary>
        /// Adds a parameter to the parameter set and returns the parameter name.
        /// </summary>
        /// <param name="item">The item being processed.</param>
        /// <param name="column">The column definition.</param>
        /// <param name="parameters">The current set of parameters.</param>
        /// <returns>The parameter name.</returns>
        private static string AddParameter(JObject item, ColumnDefinition column, Dictionary<string, object> parameters)
        {
            JToken rawValue = item.GetValue(column.Name, StringComparison.OrdinalIgnoreCase);
            object value = column.SerializeValue(rawValue);
            string paramName = $"@p{parameters.Count}";
            parameters[paramName] = value;
            return paramName;
        }

        /// <summary>
        /// Formats a column name for use in SQL statements.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The identifier to use in SQL statements.</returns>
        private static string FormatColumnName(string columnName) => $"[{columnName}]";

        /// <summary>
        /// Formats a table name for use in SQL statements.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The identifier to use in SQL statements.</returns>
        private static string FormatTableName(string tableName) => $"[{tableName}]";
        #endregion
    }
}
