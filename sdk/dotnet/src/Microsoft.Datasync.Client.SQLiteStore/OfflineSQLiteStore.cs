// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// The mapping from the table name to the table definition.  This is built using the
        /// <see cref="DefineTable(string, JObject)"/> method before store initialization.
        /// </summary>
        private readonly Dictionary<string, TableDefinition> tableMap = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A lock that is used to serialize write operations to the database.
        /// </summary>
        private readonly AsyncLock operationLock = new();

        /// <summary>
        /// Parameterless constructor for unit testing.
        /// </summary>
        protected OfflineSQLiteStore()
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="OfflineSQLiteStore"/> using the provided connection string.
        /// </summary>
        /// <remarks>
        /// <para>If the connection string starts with <c>file:</c>, then it is considered to be a URI filename and
        /// should be structured as such.  This allows the setting of any options (such as mode, cache, etc.)
        /// if needed.</para>
        /// <para>If the connection string does not start with file:, then it should be an absolute path (which starts
        /// with a <c>/</c>).</para>
        /// </remarks>
        /// <see href="https://sqlite.org/c3ref/open.html"/>
        /// <param name="connectionString">The connection string to use for persistent storage.</param>
        public OfflineSQLiteStore(string connectionString)
        {
            Arguments.IsNotNullOrWhitespace(connectionString, nameof(connectionString));
            DbConnection = new SqliteConnection(connectionString);
        }

        /// <summary>
        /// The database connection.
        /// </summary>
        internal SqliteConnection DbConnection { get; }

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

            if (Initialized)
            {
                throw new InvalidOperationException("Cannot define a table after the offline store has been initialized.");
            }

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
            foreach (var table in tableMap)
            {
                CreateTableFromDefinition(table.Key, table.Value);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates or inserts the item(s) provided in the table.
        /// </summary>
        /// <param name="tableName">The table to be used for the operation.</param>
        /// <param name="items">The item(s) to be updated or inserted.</param>
        /// <param name="ignoreMissingColumns">If <c>true</c>, extra properties on the item can be ignored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been updated or inserted into the table.</returns>
        public override async Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsNotNull(items, nameof(items));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            var first = items.FirstOrDefault();
            if (first == null)
            {
                return; // No items.
            }

            var tableDefinition = GetTableOrThrow(tableName);
            var columns = new List<ColumnDefinition>();
            foreach (var prop in first.Properties())
            {
                if (!tableDefinition.TryGetValue(prop.Name, out ColumnDefinition columnDefinition) && !ignoreMissingColumns)
                {
                    throw new InvalidOperationException($"Column '{prop.Name}' is not defined on local table '{tableName}'");
                }
                if (columnDefinition != null)
                {
                    columns.Add(columnDefinition);
                }
            }

            if (columns.Count == 0)
            {
                return; // No query to execute if there are no columns to add.
            }

            using (await operationLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                DbConnection.ExecuteNonQuery("BEGIN TRANSACTION");
                BatchInsert(tableName, items, columns);
                DbConnection.ExecuteNonQuery("COMMIT TRANSACTION");
            }
        }
        #endregion

        /// <summary>
        /// Batch insert some items into the database.
        /// </summary>
        /// <param name="tableName">The name of the local table.</param>
        /// <param name="items">The items to insert.</param>
        /// <param name="columns">The column definitions.</param>
        private void BatchInsert(string tableName, IEnumerable<JObject> items, List<ColumnDefinition> columns)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a table in SQLite based on the definition.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="definition">The table definition</param>
        private void CreateTableFromDefinition(string tableName, TableDefinition definition)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtains the table definition for a defined table, or throws if not available.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The table definition</returns>
        /// <exception cref="InvalidOperationException">If the table is not defined.</exception>
        private TableDefinition GetTableOrThrow(string tableName)
        {
            if (tableMap.TryGetValue(tableName, out TableDefinition table))
            {
                return table;
            }
            throw new InvalidOperationException($"Table '{tableName}' is not defined.");
        }
    }
}
