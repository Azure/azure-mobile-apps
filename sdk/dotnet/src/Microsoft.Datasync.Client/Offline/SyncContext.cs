// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Operations;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Coordinates all the requests for offline operations.
    /// </summary>
    internal class SyncContext : IDisposable
    {
        /// <summary>
        /// The backing field for the <see cref="OfflineStore"/> property.
        /// </summary>
        private IOfflineStore _store;

        /// <summary>
        /// A task used to monitor the initialization of the sync context.
        /// </summary>
        private TaskCompletionSource<object> initializationTask;

        /// <summary>
        /// A lock to ensure that multiple operations don't interleave as they are added to the queue and store.
        /// </summary>
        private readonly AsyncReaderWriterLock storeQueueLock = new();

        /// <summary>
        /// Creates a new <see cref="SyncContext"/>.
        /// </summary>
        /// <param name="client">The associated client.</param>
        public SyncContext(DatasyncClient client)
        {
            Arguments.IsNotNull(client, nameof(client));
            ServiceClient = client;
        }

        /// <summary>
        /// When <c>true</c>, the sync context has been initialized.
        /// </summary>
        public bool IsInitialized { get => initializationTask?.Task.Status == TaskStatus.RanToCompletion; }

        /// <summary>
        /// The local store for offline operations.
        /// </summary>
        public IOfflineStore OfflineStore
        {
            get => _store;
            set
            {
                if (_store != value)
                {
                    _store?.Dispose();
                    _store = value;
                }
            }
        }

        /// <summary>
        /// A queue of pending operations waiting to be sent to the remote service.
        /// </summary>
        private OperationsQueue OperationsQueue { get; set; }

        /// <summary>
        /// The associated client.
        /// </summary>
        private DatasyncClient ServiceClient { get; }

        /// <summary>
        /// The synchronization handler.
        /// </summary>
        public ISyncHandler SyncHandler { get; private set; }

        /// <summary>
        /// The configuration store for the query delta tokens and other table configuration.
        /// </summary>
        private SyncSettingsManager SyncSettingsManager { get; set; }

        /// <summary>
        /// Initialize the synchronization context for this offline store, using the default sync handler.
        /// </summary>
        /// <param name="store">The offline store to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the synchronization context is initialized.</returns>
        public Task InitializeAsync(IOfflineStore store, CancellationToken cancellationToken = default)
            => InitializeAsync(store, null, cancellationToken);

        /// <summary>
        /// Initialize the synchronization context for this offline store.
        /// </summary>
        /// <param name="store">The offline store to use.</param>
        /// <param name="handler">The synchronization handler.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the synchronization context is initialized.</returns>
        public async Task InitializeAsync(IOfflineStore store, ISyncHandler handler, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(store, nameof(store));

            initializationTask = new();
            using (await storeQueueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
            {
                SyncHandler = handler ?? new SyncHandler();
                OfflineStore = store;

                await OfflineStore.InitializeAsync(cancellationToken).ConfigureAwait(false);

                OperationsQueue = await OperationsQueue.LoadFromOfflineStoreAsync(OfflineStore, cancellationToken).ConfigureAwait(false);
                SyncSettingsManager = new SyncSettingsManager(OfflineStore);

                initializationTask.SetResult(null);
            }
        }

        /// <summary>
        /// Removes an item by ID from the offline table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="id">The ID of the item to remove.</param>
        /// <param name="instance">The instance being deleted (for version validation).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item is removed from the offline table.</returns>
        public async Task DeleteAsync(string tableName, string id, JObject instance, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            var operation = new DeleteOperation(tableName, id)
            {
                Table = ServiceClient.GetRemoteTable(tableName) as RemoteTable,
                Item = instance
            };
            await ExecuteOperationAsync(operation, instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an item by ID from the offline store.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="id">The ID of the item to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item (or <c>null</c>) when complete.</returns>
        public async Task<JObject> GetAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            return await OfflineStore.GetItemAsync(tableName, id, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts an item into the offline table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="id">The ID of the item to insert.</param>
        /// <param name="instance">The instance being inserted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item is inserted to the offline table.</returns>
        public async Task InsertAsync(string tableName, string id, JObject instance, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            var operation = new InsertOperation(tableName, id)
            {
                Table = ServiceClient.GetRemoteTable(tableName) as RemoteTable
            };
            await ExecuteOperationAsync(operation, instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Updated an item in the offline table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="id">The ID of the item to update.</param>
        /// <param name="instance">The new data for the instance.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item is updated in the offline table.</returns>
        public async Task UpdateAsync(string tableName, string id, JObject instance, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            var operation = new UpdateOperation(tableName, id)
            {
                Table = ServiceClient.GetRemoteTable(tableName) as RemoteTable
            };
            await ExecuteOperationAsync(operation, instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Cancels the table operation, discarding the item.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> being processed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public Task CancelAndDiscardItemAsync(TableOperationError error, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancels an operation and updated the item with the version from the service.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> that is being processed.</param>
        /// <param name="item">The item to use for updating.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the update is finished.</returns>
        public Task CancelAndUpdateItemAsync(TableOperationError error, JObject item, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the item within an operation for retry.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> that is being processed.</param>
        /// <param name="item">The item to use for updating.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the update is finished.</returns>
        public Task UpdateOperationAsync(TableOperationError error, JObject item, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ensures that the synchronization context is initialized, waiting for it if still initializing.
        /// </summary>
        /// <returns>A task that completes when initialization is complete.</returns>
        /// <exception cref="InvalidOperationException">If <see cref="InitializeAsync(IOfflineStore, ISyncHandler, CancellationToken)"/> has not been called.</exception>
        private Task EnsureInitializedAsync()
            => initializationTask?.Task ?? throw new InvalidOperationException("SyncContext is not yet initialized.");

        /// <summary>
        /// Execute an operation on the queue.
        /// </summary>
        /// <param name="operation">The table operation in the queue</param>
        /// <param name="item">The associated item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation has executed.</returns>
        private async Task ExecuteOperationAsync(TableOperation operation, JObject item, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);

            // Take the slowest lock first and quickest last to avoid blocking quick operations for a long time.
            using (await OperationsQueue.LockItemAsync(operation.ItemId, cancellationToken).ConfigureAwait(false))
            using (await OperationsQueue.LockTableAsync(operation.TableName, cancellationToken).ConfigureAwait(false))
            using (await storeQueueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
            {
                TableOperation existing = await OperationsQueue.GetOperationByItemIdAsync(operation.TableName, operation.ItemId, cancellationToken).ConfigureAwait(false);
                existing?.ValidateOperationCanCollapse(operation);

                try
                {
                    await operation.ExecuteOperationOnOfflineStoreAsync(_store, item, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not LocalStoreException)
                {
                    throw new LocalStoreException("Failed to perform operation on local store.", ex);
                }

                if (existing != null)
                {
                    existing.CollapseOperation(operation);
                    await OfflineStore.DeleteAsync(OfflineSystemTables.SyncErrors, existing.Id, cancellationToken).ConfigureAwait(false);
                    if (existing.IsCancelled)
                    {
                        await OperationsQueue.DeleteAsync(existing.Id, existing.Version, cancellationToken).ConfigureAwait(false);
                    }
                    else if (existing.IsUpdated)
                    {
                        await OperationsQueue.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
                    }
                }

                if (!operation.IsCancelled)
                {
                    await OperationsQueue.EnqueueAsync(operation, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                OfflineStore?.Dispose();
            }
        }
        #endregion
    }
}
