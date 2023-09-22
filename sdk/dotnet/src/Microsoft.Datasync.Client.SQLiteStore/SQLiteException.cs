// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using SQLitePCL;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// Indicates a SQLite error occurred.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SQLiteException : Exception
    {
        public SQLiteException() : base()
        {
        }

        public SQLiteException(string message) : base(message)
        {
        }

        public SQLiteException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a <see cref="SQLiteException"/> based on the result of a SQLite call.
        /// </summary>
        /// <param name="rc">the result from the call</param>
        /// <param name="db">the database connection</param>
        public SQLiteException(int rc, sqlite3 db) : base()
        {
            SetSqliteMessage(rc, db);
        }

        /// <summary>
        /// Creates a <see cref="SQLiteException"/> based on the result of a SQLite call.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="rc">the result from the call</param>
        /// <param name="db">the database connection</param>
        public SQLiteException(string message, int rc, sqlite3 db) : base(message)
        {
            SetSqliteMessage(rc, db);
        }

        /// <summary>
        /// Sets the <see cref="SqliteMessage"/> property.
        /// </summary>
        /// <param name="rc">The response code from the SQLite command.</param>
        /// <param name="db">The database connection.</param>
        private void SetSqliteMessage(int rc, sqlite3 db)
        {
            SqliteErrorCode = rc;
            SqliteMessage = db == null ? raw.sqlite3_errstr(rc).utf8_to_string() : raw.sqlite3_errmsg(db).utf8_to_string();
        }

        /// <summary>
        /// The error message from SQLite.
        /// </summary>
        public string SqliteMessage { get; private set; }
        
        /// <summary>
        /// The error code from SQLite.
        /// </summary>
        public int SqliteErrorCode { get; private set; }
    }
}
