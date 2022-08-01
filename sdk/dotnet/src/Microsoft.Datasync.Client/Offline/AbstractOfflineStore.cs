// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The base implementation of the <see cref="IOfflineStore"/> interface.  Implementors of
    /// new offline stores should implement based on this class, and not the interface.
    /// </summary>
    public abstract class AbstractOfflineStore : IOfflineStore
    {
        /// <summary>
        /// A lock around the initialization code to ensure that we don't initialize the store twice.
        /// </summary>
        private readonly AsyncLock initializationLock = new();

        /// <summary>
        /// Indicates whether the offline store has been initialized.
        /// </summary>
        protected bool Initialized { get; private set; }

        #region IOfflineStore
        /// <summary>
        /// Deletes items from the table where the items are identified by a query.
        /// </summary>
        /// <param name="query">A query description that identifies the items to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public abstract Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the items have been deleted from the table.</returns>
        public abstract Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="tableName">The table name holding the item.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public abstract Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        public abstract Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of offline tables that have been defined.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the list of tables that have been defined.</returns>
        public abstract Task<IList<string>> GetTablesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is ready to use.</returns>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            using (await initializationLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!Initialized)
                {
                    SystemTables.DefineAllSystemTables(this);
                    await InitializeOfflineStoreAsync(cancellationToken).ConfigureAwait(false);
                    Initialized = true;
                }
            }
        }

        /// <summary>
        /// Updates or inserts the item(s) provided in the table.
        /// </summary>
        /// <param name="tableName">The table to be used for the operation.</param>
        /// <param name="items">The item(s) to be updated or inserted.</param>
        /// <param name="ignoreMissingColumns">If <c>true</c>, extra properties on the item can be ignored.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been updated or inserted into the table.</returns>
        public abstract Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default);
        #endregion

        /// <summary>
        /// Defines a table in the local store.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="tableDefinition">The table definition as a sample JSON object.</param>
        public abstract void DefineTable(string tableName, JObject tableDefinition);

        /// <summary>
        /// Determines if a table is defined.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>true if the table is defined.</returns>
        public abstract bool TableIsDefined(string tableName);

        /// <summary>
        /// Ensures that the store has been initialized.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store has been initialized.</returns>
        protected async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            using (await initializationLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                await InitializeOfflineStoreAsync(cancellationToken).ConfigureAwait(false);
                //if (!Initialized)
                //{
                //    throw new InvalidOperationException("The offline store must be initialized before it can be used.");
                //}
            }
        }

        /// <summary>
        /// Initialize the store.  This is over-ridden by the store implementation to provide a point
        /// where the tables can be created or updated.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is initialized.</returns>
        protected abstract Task InitializeOfflineStoreAsync(CancellationToken cancellationToken);

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
        #endregion
    }
}
