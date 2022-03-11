// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    internal class SyncContext : IDisposable
    {
        private readonly AsyncLock initializationLock = new();

        /// <summary>
        /// Coordinates all the requests for offline operations.
        /// </summary>
        internal SyncContext(DatasyncClient client, IOfflineStore store)
        {
            ServiceClient = client;
            OfflineStore = store;
        }

        /// <summary>
        /// <c>true</c> when the <see cref="SyncContext"/> can be used.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The offline store to use for persistent storage.
        /// </summary>
        public IOfflineStore OfflineStore { get; }

        /// <summary>
        /// The persistent operations queue.
        /// </summary>
        public OperationsQueue OperationsQueue { get; private set; }

        /// <summary>
        /// The service client to use for communicating with the back end service.
        /// </summary>
        public DatasyncClient ServiceClient { get; }

        /// <summary>
        /// Initialize the synchronization context for this offline store.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when initialization is complete.</returns>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            using (await initializationLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                if (IsInitialized)
                {
                    return;
                }

                // Initialize the operations queue.
                OperationsQueue = new OperationsQueue(OfflineStore);
                await OperationsQueue.InitializeAsync(cancellationToken).ConfigureAwait(false);

                // TODO: Initialize the delta token store.

                IsInitialized = true;
            }
        }

        #region Store Operations initiated from an OfflineTable
        /// <summary>
        /// Deletes an item from the offline table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        internal async Task DeleteItemAsync(string tableName, JObject instance, CancellationToken cancellationToken = default)
        {
            EnsureContextIsInitialized();
            string itemId = ServiceSerializer.GetId(instance);
            var originalInstance = await GetItemAsync(tableName, itemId, cancellationToken).ConfigureAwait(false);
            var operation = new DeleteOperation(tableName, itemId) { Item = originalInstance };

            await EnqueueOperationAsync(operation, originalInstance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve an item from the offline table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        internal async Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            EnsureContextIsInitialized();
            return await OfflineStore.GetItemAsync(tableName, id, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the next page of items from a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        internal async Task<Page<JObject>> GetNextPageAsync(string tableName, string query, CancellationToken cancellationToken = default)
        {
            EnsureContextIsInitialized();
            var queryDescription = QueryDescription.Parse(tableName, query);
            return await OfflineStore.GetPageAsync(queryDescription, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts an item into the offline table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        internal async Task InsertItemAsync(string tableName, JObject instance, CancellationToken cancellationToken = default)
        {
            EnsureContextIsInitialized();

            // We have to pre-generate the ID when doing offline work.
            string itemId = ServiceSerializer.GetId(instance, allowDefault: true);
            if (itemId == null)
            {
                itemId = Guid.NewGuid().ToString("N");
                instance = (JObject)instance.DeepClone();
                instance[SystemProperties.JsonIdProperty] = itemId;
            }
            var operation = new InsertOperation(tableName, itemId);
            await EnqueueOperationAsync(operation, instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Replaces an item into the remote table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="instance">The instance to replace into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the replaced data when complete.</returns>
        internal async Task ReplaceItemAsync(string tableName, JObject instance, CancellationToken cancellationToken = default)
        {
            EnsureContextIsInitialized();

            string itemId = ServiceSerializer.GetId(instance);
            instance = ServiceSerializer.RemoveSystemProperties(instance, out string version);
            if (version != null)
            {
                instance[SystemProperties.JsonVersionProperty] = version;
            }
            var operation = new UpdateOperation(tableName, itemId);
            await EnqueueOperationAsync(operation, instance, cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Data synchronization methods
        /// <summary>
        /// Pulls the items matching the provided query from the remote table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="query">The OData query that determines which items to pull from the remote table.</param>
        /// <param name="options">The options used to configure the pull operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation has finished.</returns>
        internal Task PullItemsAsync(string tableName, string query, PullOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all the items in the offline table that match the query.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="options">The options used to configure the purge operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the purge operation has finished.</returns>
        internal Task PurgeItemsAsync(string tableName, string query, PurgeOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pushes items in the operations queue for this table to the remote service.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A task that completes when the push operation has finished.</returns>
        internal Task PushItemsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Enqueues an operation and updates the offline store.
        /// </summary>
        /// <param name="operation">The table operation to enqueue.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is enqueued on the operations queue.</returns>
        private async Task EnqueueOperationAsync(TableOperation operation, JObject instance, CancellationToken cancellationToken)
        {
            using (await OperationsQueue.AcquireLockAsync(cancellationToken).ConfigureAwait(false))
            {
                // See if there is an existing operation.  If there is, then validate that it can be collapsed.
                TableOperation existingOperation = await OperationsQueue.GetOperationByItemIdAsync(operation.TableName, operation.ItemId, cancellationToken).ConfigureAwait(false);
                existingOperation?.ValidateOperationCanCollapse(operation);

                // Execute the operation on the local store.
                try
                {
                    await operation.ExecuteOperationOnOfflineStoreAsync(OfflineStore, instance, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OfflineStoreException)
                {
                    throw new OfflineStoreException("Failed to perform operation on local store.", ex);
                }

                // If there is an existing operation, then collapse the operation with the previous one.
                if (existingOperation != null)
                {
                    existingOperation.CollapseOperation(operation);
                    if (existingOperation.IsCancelled)
                    {
                        await OperationsQueue.DeleteOperationByIdAsync(existingOperation.Id, existingOperation.Version, cancellationToken).ConfigureAwait(false);
                    }
                    else if (existingOperation.IsUpdated)
                    {
                        await OperationsQueue.UpdateOperationAsync(existingOperation, cancellationToken).ConfigureAwait(false);
                    }
                }

                if (!operation.IsCancelled)
                {
                    await OperationsQueue.EnqueueOperationAsync(operation, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Ensures that the <see cref="SyncContext"/> has been initialized before use.
        /// </summary>
        /// <exception cref="InvalidOperationException">if the <see cref="SyncContext"/> has not been initialized.</exception>
        private void EnsureContextIsInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("The synchronization context must be initialized before an offline store can be used.");
            }
        }

        #region IDisposable
        /// <summary>
        /// Disposes of this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                OfflineStore.Dispose();
            }
        }
        #endregion
    }
}
