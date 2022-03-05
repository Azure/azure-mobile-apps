// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Actions;
using Microsoft.Datasync.Client.Offline.Operations;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Coordinates all the requests for offline operations.
    /// </summary>
    public class SyncContext : IDisposable
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
        /// The number of pending operations in the operations queue.
        /// </summary>
        public long PendingOperations { get => IsInitialized ? OperationsQueue.PendingOperations : 0; }

        /// <summary>
        /// A queue of pending operations waiting to be sent to the remote service.
        /// </summary>
        internal OperationsQueue OperationsQueue { get; set; }

        /// <summary>
        /// The associated client.
        /// </summary>
        internal DatasyncClient ServiceClient { get; }

        /// <summary>
        /// The synchronization handler.
        /// </summary>
        public ISyncHandler SyncHandler { get; private set; }

        /// <summary>
        /// A queue for executing sync calls (push, pull) one after the other.
        /// </summary>
        private ActionBlock SyncQueue { get; set; }

        /// <summary>
        /// The configuration store for the query delta tokens and other table configuration.
        /// </summary>
        internal SyncSettingsManager SyncSettingsManager { get; set; }

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
                SyncQueue = new ActionBlock();

                await OfflineStore.InitializeAsync(cancellationToken).ConfigureAwait(false);

                OperationsQueue = await OperationsQueue.LoadFromOfflineStoreAsync(OfflineStore, cancellationToken).ConfigureAwait(false);
                SyncSettingsManager = new SyncSettingsManager(OfflineStore);

                initializationTask.SetResult(null);
            }
        }

        #region Store Operations initiated from an OfflineTable
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
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            return await OfflineStore.GetItemAsync(tableName, id, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves a page of items from the offline store.
        /// </summary>
        /// <param name="tableName">The name of the table to query.</param>
        /// <param name="query">The query to execute against the offline store.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items from the store when complete.</returns>
        public async Task<Page<JToken>> GetPageAsync(string tableName, string query, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            var queryDescription = QueryDescription.Parse(tableName, query);
            using (await storeQueueLock.ReaderLockAsync(cancellationToken).ConfigureAwait(false))
            {
                return await OfflineStore.GetPageAsync(queryDescription, cancellationToken).ConfigureAwait(false);
            }
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
            var operation = new UpdateOperation(tableName, id)
            {
                Table = ServiceClient.GetRemoteTable(tableName) as RemoteTable
            };
            await ExecuteOperationAsync(operation, instance, cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Synchronization Methods between offline and remote tables
        /// <summary>
        /// Pulls the items matching the provided query from the remote table.
        /// </summary>
        /// <param name="tableName">The name of the remote table.</param>
        /// <param name="query">The OData query that determines which items to pull from the remote table.</param>
        /// <param name="options">The options used to configure the pull operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation has finished.</returns>
        public async Task PullAsync(string tableName, string query, PullOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all the items in the offline table that match the query.
        /// </summary>
        /// <param name="tableName">The name of the remote table.</param>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="options">The options used to configure the purge operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the purge operation has finished.</returns>
        public async Task PurgeAsync(string tableName, string query, PurgeOptions options, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync();
            var table = ServiceClient.GetRemoteTable(tableName) as RemoteTable;
            var queryDescription = QueryDescription.Parse(tableName, query);
            var queryId = options.QueryId ?? GetQueryIdFromQuery(tableName, query);
            var action = new PurgeAction(this, table, queryId, queryDescription, options.DiscardPendingOperations, cancellationToken);
            await ExecuteSyncActionAsync(action).ConfigureAwait(false);
        }

        /// <summary>
        /// Pushes all the pending operations to the remote service.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public Task PushAsync(CancellationToken cancellationToken = default)
            => PushAsync(Array.Empty<string>(), cancellationToken);

        /// <summary>
        /// Pushes a list of table names to the remote service.
        /// </summary>
        /// <param name="tableNames">The list of tables to push.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task PushAsync(string[] tableNames, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            var action = new PushAction(this, tableNames, cancellationToken);
            await ExecuteSyncActionAsync(action).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a <see cref="SyncAction"/> on a queue.
        /// </summary>
        /// <param name="action">The <see cref="SyncAction"/> to execute.</param>
        /// <returns>A task that completes when the action is completed.</returns>
        private Task ExecuteSyncActionAsync(SyncAction action)
        {
            _ = SyncQueue.PostAsync(action.ExecuteAsync, action.CancellationToken);
            return action.CompletionTask;
        }
        #endregion

        #region Conflict Resolution Methods
        /// <summary>
        /// Cancels the table operation, discarding the item.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> being processed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public Task CancelAndDiscardItemAsync(TableOperationError error, CancellationToken cancellationToken = default)
        {
            string itemId = error.Item.Value<string>(SystemProperties.JsonIdProperty);
            return ExecuteOperationSafelyAsync(itemId, error.TableName, async () =>
            {
                await TryCancelOperationAsync(error, cancellationToken).ConfigureAwait(false);
                await OfflineStore.DeleteAsync(error.TableName, itemId, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
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
            string itemId = error.Item.Value<string>(SystemProperties.JsonIdProperty);
            return ExecuteOperationSafelyAsync(itemId, error.TableName, async () =>
            {
                await TryCancelOperationAsync(error, cancellationToken).ConfigureAwait(false);
                await OfflineStore.UpsertAsync(error.TableName, item, true, cancellationToken).ConfigureAwait(false);
            }, cancellationToken);
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
            string itemId = error.Item.Value<string>(SystemProperties.JsonIdProperty);
            return ExecuteOperationSafelyAsync(itemId, error.TableName, async () =>
            {
                if (!await OperationsQueue.UpdateAsync(error.Id, error.OperationVersion, item, cancellationToken).ConfigureAwait(false))
                {
                    throw new InvalidOperationException("The operation has been updated and cannot be updated again.");
                }
                await OfflineStore.DeleteAsync(OfflineSystemTables.SyncErrors, error.Id, cancellationToken).ConfigureAwait(false);
                if (error.OperationKind != TableOperationKind.Delete)
                {
                    await OfflineStore.UpsertAsync(error.TableName, item, true, cancellationToken).ConfigureAwait(false);
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Tries to cancel an operation in the operations queue.
        /// </summary>
        /// <param name="error">The table operation to cancel.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns></returns>
        private async Task TryCancelOperationAsync(TableOperationError error, CancellationToken cancellationToken = default)
        {
            if (!await OperationsQueue.DeleteAsync(error.Id, error.OperationVersion, cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException("The operation has been updated and cannot be cancelled.");
            }
            await OfflineStore.DeleteAsync(OfflineSystemTables.SyncErrors, error.Id, cancellationToken).ConfigureAwait(false);
        }
        #endregion

        /// <summary>
        /// Ensures that the synchronization context is initialized, waiting for it if still initializing.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when initialization is complete.</returns>
        /// <exception cref="InvalidOperationException">If <see cref="InitializeAsync(IOfflineStore, ISyncHandler, CancellationToken)"/> has not been called.</exception>
        private Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
            => initializationTask?.Task.ContinueWith(t => t.Result, cancellationToken) ?? throw new InvalidOperationException("SyncContext is not yet initialized.");

        /// <summary>
        /// Execute an operation on the queue.
        /// </summary>
        /// <param name="operation">The table operation in the queue</param>
        /// <param name="item">The associated item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation has executed.</returns>
        private Task ExecuteOperationAsync(TableOperation operation, JObject item, CancellationToken cancellationToken = default)
            => ExecuteOperationSafelyAsync(operation.ItemId, operation.TableName, async () =>
            {
                TableOperation existing = await OperationsQueue.GetOperationByItemIdAsync(operation.TableName, operation.ItemId, cancellationToken).ConfigureAwait(false);
                existing?.ValidateOperationCanCollapse(operation);

                try
                {
                    await operation.ExecuteOperationOnOfflineStoreAsync(OfflineStore, item, cancellationToken).ConfigureAwait(false);
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
            }, cancellationToken);

        /// <summary>
        /// Execute an operation safely, inside the confines of item and table locks, and ensuring the store actually exists.
        /// </summary>
        /// <param name="itemId">The ID of the item being affected.</param>
        /// <param name="tableName">The name of the table holding the item.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is executed.</returns>
        private async Task ExecuteOperationSafelyAsync(string itemId, string tableName, Func<Task> action, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);

            // take slowest lock first and quickest last in order to avoid blocking quick operations for long time            
            using (await OperationsQueue.LockItemAsync(itemId, cancellationToken).ConfigureAwait(false))  // prevent any inflight operation on the same item
            using (await OperationsQueue.LockTableAsync(tableName, cancellationToken).ConfigureAwait(false)) // prevent interferance with any in-progress pull/purge action
            using (await storeQueueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false)) // prevent any other operation from interleaving between store and queue insert
            {
                await action();
            }
        }

        /// <summary>
        /// Obtains a query ID from a query and table name.  This is used
        /// when the dev doesn't specify a query ID.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="query">The query string.</param>
        /// <returns>A query ID.</returns>
        private static string GetQueryIdFromQuery(string tableName, string query)
        {
            string hashKey = $"q|{tableName}|{query}";
            byte[] bytes = Encoding.UTF8.GetBytes(hashKey);
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
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
