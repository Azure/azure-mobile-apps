// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.InMemoryStore
{
    /// <summary>
    /// An in-memory store used primarily for testing purposes.
    /// </summary>
    public class InMemoryOfflineStore : AbstractOfflineStore
    {
        private readonly Dictionary<string, InMemoryTable> TableMap = new();

        #region IOfflineStore
        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public override async Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));
            await EnsureInitializedAsync(query.TableName, cancellationToken).ConfigureAwait(false);
            await TableMap[query.TableName].DeleteAsync(query, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public override async Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsNotNull(ids, nameof(ids));
            await EnsureInitializedAsync(tableName, cancellationToken).ConfigureAwait(false);
            await TableMap[tableName].DeleteAsync(ids, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="tableName">The table name holding the item.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public override async Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, true, nameof(tableName));
            Arguments.IsValidId(id, nameof(id));
            await EnsureInitializedAsync(tableName, cancellationToken).ConfigureAwait(false);
            return await TableMap[tableName].GetItemAsync(id, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        public override async Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));
            await EnsureInitializedAsync(query.TableName, cancellationToken).ConfigureAwait(false);
            return await TableMap[query.TableName].GetPageAsync(query, cancellationToken).ConfigureAwait(false);
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
            await EnsureInitializedAsync(tableName, cancellationToken).ConfigureAwait(false);
            await TableMap[tableName].UpsertAsync(items, ignoreMissingColumns, cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region AbstractOfflineStore
        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is ready to use.</returns>
        protected override Task InitializeOfflineStoreAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Defines a table in the local store.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="tableDefinition">The table definition as a sample JSON object.</param>
        public override void DefineTable(string tableName, JObject tableDefinition)
        {
            var table = new InMemoryTable(tableName, tableDefinition);
            TableMap.Add(tableName, table);
        }
        #endregion

        /// <summary>
        /// Ensures the offline store is initialized and that the requested table exists.
        /// </summary>
        /// <param name="tableName">The name of the table for the offline store operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        private async Task EnsureInitializedAsync(string tableName, CancellationToken cancellationToken)
        {
            await base.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            if (!TableMap.ContainsKey(tableName))
            {
                throw new OfflineStoreException($"Table '{tableName}' is not defined in the store");
            }
        }
    }
}
