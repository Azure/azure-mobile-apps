// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.SQLiteStore
{
    /// <summary>
    /// An implementation of <see cref="IOfflineStore"/> using SQLite as the persistent store.
    /// </summary>
    public class OfflineSQLiteStore : AbstractOfflineStore
    {
        /// <summary>
        /// Internal flag to indicate that SQLite library is initialized.
        /// </summary>
        internal static bool sqliteIsInitialized = false;

        /// <summary>
        /// The mapping from the table name to the table definition.  This is built using the
        /// <see cref="DefineTable(string, JObject)"/> method before store initialization.
        /// </summary>
        private readonly Dictionary<string, TableDefinition> tableMap = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Parameterless constructor for unit testing.
        /// </summary>
        protected OfflineSQLiteStore()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="OfflineSQLiteStore"/> using the
        /// provided connection string.
        /// </summary>
        /// <param name="filename">The name of the file to use for persistent storage.</param>
        /// <param name="useInMemoryStore">If <c>true</c>, use the in-memory store.</param>
        public OfflineSQLiteStore(string filename, bool useInMemoryStore = false)
        {
            Arguments.IsNotNullOrWhitespace(filename, nameof(filename));
            if (!useInMemoryStore)
            {
                Filename = filename.StartsWith("/") ? filename : throw new NotSupportedException("Specify a fully-qualified path name.");
            }
            else
            {
                Filename = "in-memory-store.db";
            }
            DbConnection = GetSqliteConnection(Filename, useInMemoryStore);
        }

        /// <summary>
        /// The filename for the store.
        /// </summary>
        internal string Filename { get; }

        /// <summary>
        /// The database connection.
        /// </summary>
        internal sqlite3 DbConnection { get; }

        #region AbstractOfflineStore
        /// <summary>
        /// Defines the local table in the persistent store.
        /// </summary>
        /// <param name="tableName">The name of the local table.</param>
        /// <param name="tableDefinition">The definition of the table.</param>
        public override void DefineTable(string tableName, JObject tableDefinition)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsNotNull(tableDefinition, nameof(tableDefinition));
            tableMap.Add(tableName, new TableDefinition(tableDefinition));
        }

        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public override Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public override Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="tableName">The table name holding the item.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public override Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        public override Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initialize the store.  This is over-ridden by the store implementation to provide a point
        /// where the tables can be created or updated.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is initialized.</returns>
        protected override Task InitializeOfflineStoreAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates or inserts the item(s) provided in the table.
        /// </summary>
        /// <param name="tableName">The table to be used for the operation.</param>
        /// <param name="items">The item(s) to be updated or inserted.</param>
        /// <param name="ignoreMissingColumns">If <c>true</c>, extra properties on the item can be ignored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been updated or inserted into the table.</returns>
        public override Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Creates a new <see cref="sqlite3"/> database connection
        /// </summary>
        /// <param name="filename">The fully qualified filename to use.</param>
        /// <param name="useInMemoryStore">If <c>true</c>, ignore the filename and use an in-memory store.</param>
        /// <returns>The opened database connection.</returns>
        private static sqlite3 GetSqliteConnection(string filename, bool useInMemoryStore)
        {
            if (!sqliteIsInitialized)
            {
                Batteries_V2.Init();
                sqliteIsInitialized = true;
            }

            int flags = raw.SQLITE_OPEN_READWRITE | raw.SQLITE_OPEN_CREATE | raw.SQLITE_OPEN_FULLMUTEX;
            if (useInMemoryStore)
            {
                flags |= raw.SQLITE_OPEN_MEMORY;
            }
            int rc = raw.sqlite3_open_v2(filename, out sqlite3 db, flags, null);
            if (rc != raw.SQLITE_OK)
            {
                throw new SQLiteException(rc, db);
            }
            return db;
        }
    }
}
