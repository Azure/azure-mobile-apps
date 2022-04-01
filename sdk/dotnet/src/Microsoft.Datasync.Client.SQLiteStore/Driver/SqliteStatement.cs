// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using SQLitePCL;
using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.SQLiteStore.Driver
{
    /// <summary>
    /// Represents a SQL Statement.
    /// </summary>
    internal class SqliteStatement : IDisposable
    {
        private readonly sqlite3 connection;
        private readonly sqlite3_stmt stmt;
        private bool disposedValue;

        /// <summary>
        /// Creates a new <see cref="SqliteStatement"/> object.
        /// </summary>
        /// <param name="connection">The database connection that created this statement.</param>
        /// <param name="stmt">The source statement.</param>
        internal SqliteStatement(sqlite3 connection, sqlite3_stmt stmt)
        {
            this.connection = connection;
            this.stmt = stmt;
        }

        /// <summary>
        /// Binds a single parameter to the statement.
        /// </summary>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public void BindParameter(string paramName, object value)
        {
            int rc;
            int index = raw.sqlite3_bind_parameter_index(stmt, paramName);

            if (value == null)
            {
                rc = raw.sqlite3_bind_null(stmt, index);
            }
            else if (value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong)
            {
                if (value is ulong @ulong && @ulong > long.MaxValue)
                {
                    throw new OverflowException($"Value of parameter '{paramName}' is too large.");
                }
                rc = raw.sqlite3_bind_int64(stmt, index, Convert.ToInt64(value));
            }
            else if (value is decimal || value is float || value is double)
            {
                rc = raw.sqlite3_bind_double(stmt, index, Convert.ToDouble(value));
            }
            else if (value is char || value is string)
            {
                rc = raw.sqlite3_bind_text(stmt, index, value.ToString());
            }
            else if (value is byte[] blob)
            {
                rc = raw.sqlite3_bind_blob(stmt, index, blob);
            }
            else
            {
                throw new NotSupportedException($"SQLite binding for type '{value.GetType().FullName}' not supported.");
            }
            if (rc != raw.SQLITE_OK)
            {
                throw new SQLiteException("Error executing SQLite command", rc, connection);
            }
        }

        /// <summary>
        /// Binds a dictionary of parameters to the statement.
        /// </summary>
        /// <param name="parameters">The list of parameters</param>
        public void BindParameters(IDictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                BindParameter(parameter.Key, parameter.Value);
            }
        }

        /// <summary>
        /// Executes a non-query statement.
        /// </summary>
        public void ExecuteNonQuery()
        {
            int rc = raw.sqlite3_step(stmt);
            if (rc != raw.SQLITE_DONE)
            {
                throw new SQLiteException("Invalid response while executing SQL statement", rc, connection);
            }
        }

        /// <summary>
        /// Executes a query statement.
        /// </summary>
        /// <returns>An enumerable of the query results, row by row.</returns>
        public IEnumerable<Dictionary<string, object>> ExecuteQuery()
        {
            int rc;
            while ((rc = raw.sqlite3_step(stmt)) == raw.SQLITE_ROW)
                yield return ReadRow();
            if (rc != raw.SQLITE_DONE)
            {
                throw new SQLiteException("Error reading results of query", rc, connection);
            }
            yield break;
        }

        /// <summary>
        /// Gets the column name at the specified index in the current row.
        /// </summary>
        /// <param name="index">The index of the column.</param>
        /// <returns>The column name.</returns>
        private string GetColumnName(int index)
            => raw.sqlite3_column_name(stmt, index).utf8_to_string();

        /// <summary>
        /// Gets the value of the column at the specified index in the current row.
        /// </summary>
        /// <param name="index">The index of the column.</param>
        /// <returns>The column value.</returns>
        private object GetColumnValue(int index)
        {
            var type = raw.sqlite3_column_type(stmt, index);
            return type switch
            {
                raw.SQLITE_INTEGER => raw.sqlite3_column_int64(stmt, index),
                raw.SQLITE_FLOAT => raw.sqlite3_column_double(stmt, index),
                raw.SQLITE_TEXT => raw.sqlite3_column_text(stmt, index).utf8_to_string(),
                raw.SQLITE_BLOB => raw.sqlite3_column_blob(stmt, index).ToArray(),
                _ => null
            };
        }

        /// <summary>
        /// Converts the row results to a dictionary.
        /// </summary>
        /// <returns>The dictionary of a single row of results.</returns>
        private Dictionary<string, object> ReadRow()
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < raw.sqlite3_column_count(stmt); i++)
            {
                string name = GetColumnName(i);
                row[name] = GetColumnValue(i);
            }
            return row;
        }

        #region IDisposable
        /// <summary>
        /// Dispose of this statement.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    stmt.Close();
                    stmt.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
