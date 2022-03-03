// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
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
        private readonly SemaphoreSlim initializationLock = new(1);

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
        /// <returns>A task the completes when the items have been deleted from the table.</returns>
        public abstract Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes items from the table where the items are identified by their ID.
        /// </summary>
        /// <param name="tableName">The name of the table where the items are located.</param>
        /// <param name="ids">A list of IDs to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task the completes when the items have been deleted from the table.</returns>
        public abstract Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns items from the table.
        /// </summary>
        /// <param name="query">A query describing the items to be returned.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> of the items returned.</returns>
        public abstract IAsyncEnumerable<JToken> GetAsyncItems(QueryDescription query);

        /// <summary>
        /// Returns a single item by the ID of the item.
        /// </summary>
        /// <param name="tableName">The table name holding the item.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public abstract Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes the store for use.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is ready to use.</returns>
        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (!Initialized)
            {
                await initializationLock.WaitAsync(cancellationToken);
                try
                {
                    OfflineSystemTables.DefineAllSystemTableTables(this);
                    await InitializeStoreAsync(cancellationToken).ConfigureAwait(false);
                    Initialized = true;
                }
                finally
                {
                    initializationLock.Release();
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
        /// <param name="tableDefinition">The definition of the table as a sample JSON object.</param>
        internal abstract void DefineTable(string tableName, JObject tableDefinition);

        /// <summary>
        /// Ensures that the store has been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">if the store has not been initialized.</exception>
        protected void EnsureInitialized()
        {
            initializationLock.Wait();
            try
            {
                if (!Initialized)
                {
                    throw new InvalidOperationException("The offline store must be initialized before it can be used.");
                }
            }
            finally
            {
                initializationLock.Release();
            }
        }

        /// <summary>
        /// Initialize the store.  This is over-ridden by the store implementation to provide a point
        /// where the tables can be created or updated.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the store is initialized.</returns>
        protected abstract Task InitializeStoreAsync(CancellationToken cancellationToken = default);

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
