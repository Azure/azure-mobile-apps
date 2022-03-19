// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    internal class SyncContext : IDisposable
    {
        private readonly AsyncLock initializationLock = new();
        private readonly AsyncReaderWriterLock queueLock = new();
        private readonly AsyncLockDictionary tableLock = new();

        /// <summary>
        /// Coordinates all the requests for offline operations.
        /// </summary>
        internal SyncContext(DatasyncClient client, IOfflineStore store)
        {
            Arguments.IsNotNull(client, nameof(client));
            Arguments.IsNotNull(store, nameof(store));

            ServiceClient = client;
            OfflineStore = store;
        }

        /// <summary>
        /// <c>true</c> when the <see cref="SyncContext"/> can be used.
        /// </summary>
        internal bool IsInitialized { get; private set; }

        /// <summary>
        /// The offline store to use for persistent storage.
        /// </summary>
        internal IOfflineStore OfflineStore { get; }

        /// <summary>
        /// The persistent operations queue.
        /// </summary>
        internal OperationsQueue OperationsQueue { get; private set; }

        /// <summary>
        /// Persistent storage for delta-tokens.
        /// </summary>
        internal DeltaTokenStore DeltaTokenStore { get; private set; }

        /// <summary>
        /// The service client to use for communicating with the back end service.
        /// </summary>
        internal DatasyncClient ServiceClient { get; }

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

                // Initialize the delta token store.
                DeltaTokenStore = new DeltaTokenStore(OfflineStore);

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
            if (originalInstance == null)
            {
                throw new InvalidOperationException($"The item with ID '{itemId}' is not in the offline store.");
            }
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
        internal async Task PullItemsAsync(string tableName, string query, PullOptions options, CancellationToken cancellationToken = default)
        {
            EnsureContextIsInitialized();
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
        internal async Task PurgeItemsAsync(string tableName, string query, PurgeOptions options, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            Arguments.IsNotNull(query, nameof(query));
            Arguments.IsNotNull(options, nameof(options));
            EnsureContextIsInitialized();

            var queryId = options.QueryId ?? GetQueryIdFromQuery(tableName, query);
            var queryDescription = QueryDescription.Parse(tableName, query);
            using (await tableLock.AcquireAsync(tableName, cancellationToken).ConfigureAwait(false))
            {
                if (await TableIsDirtyAsync(tableName, cancellationToken).ConfigureAwait(false))
                {
                    if (options.DiscardPendingOperations)
                    {
                        await DiscardTableOperationsAsync(tableName, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new InvalidOperationException("The table cannot be purged because it has pending operations.");
                    }
                }

                // Execute the purge
                using (await queueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (!string.IsNullOrEmpty(queryId))
                    {
                        await DeltaTokenStore.ResetDeltaTokenAsync(tableName, queryId, cancellationToken).ConfigureAwait(false);
                    }
                    await OfflineStore.DeleteAsync(queryDescription, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Pushes items in the operations queue for the named table to the remote service.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A task that completes when the push operation has finished.</returns>
        internal Task PushItemsAsync(string tableName, CancellationToken cancellationToken = default)
            => PushItemsAsync(new string[] { tableName }, cancellationToken);

        /// <summary>
        /// Pushes items in the operations queue for the list of tables to the remote service.
        /// </summary>
        /// <param name="tableNames">The list of table names to push.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the push operation has finished.</returns>
        /// <exception cref="PushFailedException">if the push operation failed.</exception>
        internal async Task PushItemsAsync(string[] tableNames, CancellationToken cancellationToken = default)
        {
            EnsureContextIsInitialized();

            var batch = new OperationBatch(this);
            cancellationToken.Register(() => batch.Abort(PushStatus.CancelledByToken));

            // Main Push Operations Loop
            try
            {
                using (await queueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
                {
                    TableOperation operation = await OperationsQueue.PeekAsync(0, tableNames, cancellationToken).ConfigureAwait(false);
                    while (operation != null)
                    {
                        bool success = await ExecutePushOperationAsync(operation, batch, cancellationToken).ConfigureAwait(false);
                        if (batch.AbortReason.HasValue)
                        {
                            break;
                        }
                        if (success)
                        {
                            await OperationsQueue.DeleteOperationByIdAsync(operation.Id, operation.Version, cancellationToken).ConfigureAwait(false);
                        }
                        operation = await OperationsQueue.PeekAsync(operation.Sequence, tableNames, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(ex);
            }

            // If there were errors, then create a push failed exception.
            List<TableOperationError> errors = new();
            PushStatus batchStatus = batch.AbortReason ?? PushStatus.Complete;
            try
            {
                errors.AddRange(await batch.LoadErrorsAsync(cancellationToken).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(new OfflineStoreException("Failed to read errors from the local store.", ex));
            }

            // If the push did not complete successfully, then throw a PushFailedException.
            if (batchStatus != PushStatus.Complete || batch.HasErrors(errors))
            {
                List<TableOperationError> unhandledErrors = errors.Where(error => !error.Handled).ToList();
                Exception innerException = batch.OtherErrors.Count > 0 ? new AggregateException(batch.OtherErrors) : null;
                throw new PushFailedException(new PushCompletionResult(unhandledErrors, batchStatus), innerException);
            }
        }
        #endregion

        #region Callbacks for handling conflicts
        /// <summary>
        /// Cancels the table operation, discarding the item.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> being processed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task CancelAndDiscardItemAsync(TableOperationError error, CancellationToken cancellationToken = default)
        {
            string itemId = error.Item.Value<string>(SystemProperties.JsonIdProperty);
            using (await queueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
            {
                await TryCancelOperationAsync(error, cancellationToken).ConfigureAwait(false);
                await OfflineStore.DeleteAsync(error.TableName, new[] { itemId }, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cancels an operation and updated the item with the version from the service.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> that is being processed.</param>
        /// <param name="item">The item to use for updating.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the update is finished.</returns>
        public async Task CancelAndUpdateItemAsync(TableOperationError error, JObject item, CancellationToken cancellationToken = default)
        {
            using (await queueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
            {
                await TryCancelOperationAsync(error, cancellationToken).ConfigureAwait(false);
                await OfflineStore.UpsertAsync(error.TableName, new[] { item }, true, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Updates the item within an operation for retry.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> that is being processed.</param>
        /// <param name="item">The item to use for updating.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the update is finished.</returns>
        public async Task UpdateOperationAsync(TableOperationError error, JObject item, CancellationToken cancellationToken = default)
        {
            string itemId = error.Item.Value<string>(SystemProperties.JsonIdProperty);
            using (await queueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
            {
                if (!await OperationsQueue.UpdateOperationAsync(error.Id, error.OperationVersion, item, cancellationToken).ConfigureAwait(false))
                {
                    throw new InvalidOperationException("The operation has been updated and cannot be updated again.");
                }
                await OfflineStore.DeleteAsync(SystemTables.SyncErrors, new[] { error.Id }, cancellationToken).ConfigureAwait(false);
                if (error.OperationKind != TableOperationKind.Delete)
                {
                    await OfflineStore.UpsertAsync(error.TableName, new[] { item }, true, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Tries to cancel an operation in the operations queue.
        /// </summary>
        /// <param name="error">The table operation to cancel.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns></returns>
        private async Task TryCancelOperationAsync(TableOperationError error, CancellationToken cancellationToken = default)
        {
            if (!await OperationsQueue.DeleteOperationByIdAsync(error.Id, error.OperationVersion, cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException("The operation has been updated and cannot be cancelled.");
            }
            await OfflineStore.DeleteAsync(SystemTables.SyncErrors, new[] { error.Id }, cancellationToken).ConfigureAwait(false);
        }
        #endregion

        /// <summary>
        /// Discards operations within the operations queue for a specific table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        internal async Task DiscardTableOperationsAsync(string tableName, CancellationToken cancellationToken)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            EnsureContextIsInitialized();
            var query = QueryDescription.Parse(SystemTables.OperationsQueue, $"$filter=(tableName eq '{tableName}')");
            using (await queueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
            {
                await OperationsQueue.DeleteOperationsAsync(query, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Enqueues an operation and updates the offline store.
        /// </summary>
        /// <param name="operation">The table operation to enqueue.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is enqueued on the operations queue.</returns>
        private async Task EnqueueOperationAsync(TableOperation operation, JObject instance, CancellationToken cancellationToken)
        {
            using (await queueLock.WriterLockAsync(cancellationToken).ConfigureAwait(false))
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

        /// <summary>
        /// Executes a single operation within the queue.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="batch">The operation batch being executed.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that returns <c>true</c> if the execution was successful when complete.</returns>
        private async Task<bool> ExecutePushOperationAsync(TableOperation operation, OperationBatch batch, CancellationToken cancellationToken)
        {
            if (operation.IsCancelled || cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            if (operation.Item == null)
            {
                try
                {
                    operation.Item = await OfflineStore.GetItemAsync(operation.TableName, operation.ItemId, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    batch.Abort(PushStatus.CancelledByOfflineStoreError);
                    throw new OfflineStoreException($"Failed to read Item '{operation.ItemId}' from table '{operation.TableName}' in the offline store.", ex);
                }

                if (operation.Item == null)
                {
                    var item = new JObject(new JProperty(SystemProperties.JsonIdProperty, operation.ItemId));
                    await batch.AddErrorAsync(operation, null, null, item, cancellationToken).ConfigureAwait(false);
                    return false;
                }
            }

            await OperationsQueue.TryUpdateOperationStateAsync(operation, TableOperationState.Attempted, batch, cancellationToken).ConfigureAwait(false);
            operation.Item = ServiceSerializer.RemoveSystemProperties(operation.Item, out string version);
            if (version != null)
            {
                operation.Item[SystemProperties.JsonVersionProperty] = version;
            }

            JObject result = null;
            Exception error = null;
            try
            {
                result = await operation.ExecuteOperationOnRemoteServiceAsync(ServiceClient, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await OperationsQueue.TryUpdateOperationStateAsync(operation, TableOperationState.Failed, batch, cancellationToken).ConfigureAwait(false);
                if (ex is HttpRequestException || ex is TimeoutException)
                {
                    batch.Abort(PushStatus.CancelledByNetworkError);
                    return false;
                }
                else if (ex is DatasyncInvalidOperationException ios && ios.Response?.StatusCode == HttpStatusCode.Unauthorized)
                {
                    batch.Abort(PushStatus.CancelledByAuthenticationError);
                    return false;
                }
                else if (ex is PushAbortedException)
                {
                    batch.Abort(PushStatus.CancelledByOperation);
                    return false;
                }

                error = ex;
            }

            if (error == null && result.Value<string>(SystemProperties.JsonIdProperty) != null && operation.CanWriteResultToStore)
            {
                try
                {
                    await OfflineStore.UpsertAsync(operation.TableName, new[] { result }, true, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    batch.Abort(PushStatus.CancelledByOfflineStoreError);
                    throw new OfflineStoreException($"Failed to update item '{operation.ItemId}' in table '{operation.TableName}' of the offline store.", ex);
                }
            }
            else if (error != null)
            {
                string rawResult = null;
                if (error is DatasyncInvalidOperationException iox && iox.Response != null)
                {
                    HttpStatusCode? statusCode = iox.Response.StatusCode;
                    try
                    {
                        rawResult = await iox.Response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        result = ServiceClient.Serializer.DeserializeObjectOrDefault(rawResult);
                    }
                    catch { /* Deliberately ignore JSON parsing errors */ }
                    await batch.AddErrorAsync(operation, statusCode, rawResult, result, cancellationToken).ConfigureAwait(false);
                }
            }

            return error == null;
        }

        /// <summary>
        /// Obtains a query ID from a query and table name.  This is used
        /// when the dev doesn't specify a query ID.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="query">The query string.</param>
        /// <returns>A query ID.</returns>
        internal static string GetQueryIdFromQuery(string tableName, string query)
        {
            string hashKey = $"q|{tableName}|{query}";
            byte[] bytes = Encoding.UTF8.GetBytes(hashKey);
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
        }

        /// <summary>
        /// Determines if the table is dirty (i.e. has pending operations)
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns <c>true</c> if the table is dirty, and <c>false</c> otherwise when complete.</returns>
        internal async Task<bool> TableIsDirtyAsync(string tableName, CancellationToken cancellationToken)
        {
            EnsureContextIsInitialized();
            using (await queueLock.ReaderLockAsync(cancellationToken).ConfigureAwait(false))
            {
                long pendingOperations = await OperationsQueue.CountPendingOperationsAsync(tableName, cancellationToken).ConfigureAwait(false);
                return pendingOperations > 0;
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
