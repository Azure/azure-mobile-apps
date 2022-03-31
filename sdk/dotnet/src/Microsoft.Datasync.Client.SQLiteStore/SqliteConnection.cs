// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    internal class SqliteConnection
    {
        /// <summary>
        /// Set to <c>true</c> once the SQLitePCL library is configured.
        /// </summary>
        internal static bool sqliteIsInitialized;

        /// <summary>
        /// The SQLite database connection.
        /// </summary>
        internal sqlite3 connection;

        /// <summary>
        /// Creates a new <see cref="SqliteConnection"/> to execute SQLite commands.
        /// </summary>
        /// <remarks>
        /// <para>If the connection string starts with <c>file:</c>, then it is considered to be a URI filename and
        /// should be structured as such.  This allows the setting of any options (such as mode, cache, etc.)
        /// if needed.</para>
        /// <para>If the connection string does not start with file:, then it should be an absolute path (which starts
        /// with a <c>/</c>).</para>
        /// </remarks>
        /// <param name="connectionString">The connection string to use.</param>
        public SqliteConnection(string connectionString)
        {
            if (!sqliteIsInitialized)
            {
                Batteries_V2.Init();
                sqliteIsInitialized = true;
            }

            ExecuteSqlite3(() => raw.sqlite3_config(raw.SQLITE_CONFIG_URI, 1), "configure sqlite3 for URI connection strings.");
            ExecuteSqlite3(() => raw.sqlite3_open(connectionString, out connection), $"open database connection to '{connectionString}'");
        }

        /// <summary>
        /// Executes a SQL statement on a given table in the local SQLite database.
        /// </summary>
        /// <param name="sqlStatement">the SQL statement to execute.</param>
        /// <param name="parameters">The query parameters</param>
        public void ExecuteNonQuery(string sqlStatement, IDictionary<string, object> parameters = null)
        {
            parameters ??= new Dictionary<string, object>();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a SQLite3PCLRaw command with error checking.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="errorMessage">The error message to produce if it fails.</param>
        private void ExecuteSqlite3(Func<int> action, string errorMessage)
            => ExecuteSqlite3(action, raw.SQLITE_OK, errorMessage);

        /// <summary>
        /// Executes a SQLite3PCLRaw command with error checking.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="expectedResponse">The expected return code</param>
        /// <param name="errorMessage">The error message to produce if it fails.</param>
        private void ExecuteSqlite3(Func<int> action, int expectedResponse, string errorMessage)
        {
            int rc = action.Invoke();
            if (rc != expectedResponse)
            {
                throw new SQLiteException($"Unable to {errorMessage}", rc, connection);
            }
        }

    }
}
