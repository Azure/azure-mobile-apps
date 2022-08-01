// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Allows saving and reading data in the local offline tables.  Implementors of
    /// new offline stores should implement based on <see cref="AbstractOfflineStore"/>, and not
    /// this interface.
    /// </summary>
    public interface IOfflineStore : IDisposable
    {
        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="tableName">The table name holding the item.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of offline tables that have been defined.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the list of tables that have been defined.</returns>
        Task<IList<string>> GetTablesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is ready to use.</returns>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates or inserts the item(s) provided in the table.
        /// </summary>
        /// <param name="tableName">The table to be used for the operation.</param>
        /// <param name="items">The item(s) to be updated or inserted.</param>
        /// <param name="ignoreMissingColumns">If <c>true</c>, extra properties on the item can be ignored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been updated or inserted into the table.</returns>
        Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default);
    }
}