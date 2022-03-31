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
    [SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.", Justification = "Only generated exceptions are thrown.")]
    public class SQLiteException : Exception
    {
        /// <summary>
        /// Creates a <see cref="SQLiteException"/> based on the result of a SQLite call.
        /// </summary>
        /// <param name="rc">the result from the call</param>
        /// <param name="db">the database connection</param>
        public SQLiteException(int rc, sqlite3 db) : base()
        {
            SqliteMessage = db == null ? raw.sqlite3_errstr(rc).utf8_to_string() : raw.sqlite3_errmsg(db).utf8_to_string();
        }

        /// <summary>
        /// The error message from SQLite.
        /// </summary>
        public string SqliteMessage { get; }
    }
}
