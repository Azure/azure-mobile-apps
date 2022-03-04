// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Operations;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// A queue of all the pending and failed operations waiting on a remote service.
    /// </summary>
    internal class OperationsQueue
    {
        /// <summary>
        /// A collection of locks for individual items.
        /// </summary>
        private readonly AsyncLockDictionary itemLocks = new();

        /// <summary>
        /// A collection of locks for tables.
        /// </summary>
        private readonly AsyncLockDictionary tableLocks = new();

        /// <summary>
        /// The pending operations.
        /// </summary>
        private long pendingOperations;

        /// <summary>
        /// The current sequence ID.
        /// </summary>
        private long sequenceId;

        /// <summary>
        /// Creates a new <see cref="OperationsQueue"/>.
        /// </summary>
        /// <param name="store">The offline store used to persist the operations queue.</param>
        private OperationsQueue(IOfflineStore store)
        {
            OfflineStore = store;
        }

        /// <summary>
        /// The offline store used to persist the operations queue.
        /// </summary>
        internal IOfflineStore OfflineStore { get; }

        /// <summary>
        /// The number of pending operations in the operations queue.
        /// </summary>
        public long PendingOperations { get => pendingOperations; }

        /// <summary>
        /// Loads the operations queue from the persistent store.
        /// </summary>
        /// <param name="store">The offline store used to persist the operations queue.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the loaded operations queue when complete.</returns>
        public static async Task<OperationsQueue> LoadFromOfflineStoreAsync(IOfflineStore store, CancellationToken cancellationToken = default)
        {
            var operationsQueue = new OperationsQueue(store);

            var query = new QueryDescription(OfflineSystemTables.OperationsQueue) { IncludeTotalCount = true, Top = 1 };
            query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "sequence"), OrderByDirection.Descending));

            // Execute the query to find the number of pending operations and the last sequence ID.
            var pagedEnumerator = (store.GetAsyncItems(query) as AsyncPageable<JToken>)!.AsPages().GetAsyncEnumerator(cancellationToken);
            var hasPage = await pagedEnumerator.MoveNextAsync().ConfigureAwait(false);
            if (hasPage)
            {
                var page = pagedEnumerator.Current;
                operationsQueue.pendingOperations = page.Count ?? 0;
                operationsQueue.sequenceId = page.Items?.Select(v => v.Value<long>("sequence")).FirstOrDefault() ?? 0;
            }

            return operationsQueue;
        }

        /// <summary>
        /// Deletes an operation from the queue.
        /// </summary>
        /// <param name="id">The ID of the operation.</param>
        /// <param name="version">The version of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns <c>true</c> if the operation is deleted, and <c>false</c> otherwise.</returns>
        /// <exception cref="LocalStoreException">if the deletion could not occur because of a problem in the local store.</exception>
        public virtual async Task<bool> DeleteAsync(string id, long version, CancellationToken cancellationToken = default)
        {
            try
            {
                var operation = await GetOperationAsync(id, cancellationToken).ConfigureAwait(false);
                if (operation == null || operation.Version != version)
                {
                    return false;
                }

                await OfflineStore.DeleteAsync(OfflineSystemTables.OperationsQueue, id, cancellationToken).ConfigureAwait(false);
                Interlocked.Decrement(ref pendingOperations);
                return true;
            }
            catch (Exception ex)
            {
                throw new LocalStoreException("Failed to delete the operation from the local store", ex);
            }
        }

        /// <summary>
        /// Enqueue a table operation.
        /// </summary>
        /// <param name="operation">The table operation to place on the queue.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is enqueued.</returns>
        public async Task EnqueueAsync(TableOperation operation, CancellationToken cancellationToken = default)
        {
            operation.Sequence = Interlocked.Increment(ref sequenceId);
            await OfflineStore.UpsertAsync(OfflineSystemTables.OperationsQueue, operation.Serialize(), false, cancellationToken).ConfigureAwait(false);
            Interlocked.Increment(ref pendingOperations);
        }

        /// <summary>
        /// Retrieves a table operation within the operations queue based on the ID of the operation.
        /// </summary>
        /// <param name="id">The ID of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the table operation when complete.</returns>
        public virtual async Task<TableOperation> GetOperationAsync(string id, CancellationToken cancellationToken = default)
        {
            JObject operation = await OfflineStore.GetItemAsync(OfflineSystemTables.OperationsQueue, id, cancellationToken).ConfigureAwait(false);
            return operation == null ? null : TableOperation.Deserialize(operation);
        }

        /// <summary>
        /// Retrieves a table operation within the operations queue based on the ID of the item being processed.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="itemId">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the table operation when loaded.</returns>
        public virtual async Task<TableOperation> GetOperationByItemIdAsync(string tableName, string itemId, CancellationToken cancellationToken = default)
        {
            QueryDescription query = new(OfflineSystemTables.OperationsQueue);
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.And, Compare(BinaryOperatorKind.Equal, "tableName", tableName), Compare(BinaryOperatorKind.Equal, "itemId", itemId));
            var result = await OfflineStore.GetAsyncItems(query).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            return result is JObject operation ? TableOperation.Deserialize(operation) : null;
        }

        /// <summary>
        /// Locks an item.
        /// </summary>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a disposable lock when complete.</returns>
        public virtual Task<IDisposable> LockItemAsync(string id, CancellationToken cancellationToken = default)
            => itemLocks.AcquireAsync(id, cancellationToken);

        /// <summary>
        /// Locks a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a disposable lock when complete.</returns>
        public virtual Task<IDisposable> LockTableAsync(string tableName, CancellationToken cancellationToken = default)
            => tableLocks.AcquireAsync(tableName, cancellationToken);

        /// <summary>
        /// Updates a table operation within the operations queue.
        /// </summary>
        /// <param name="operation">The new version of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the item has been updated.</returns>
        public virtual async Task UpdateAsync(TableOperation operation, CancellationToken cancellationToken = default)
        {
            try
            {
                await OfflineStore.UpsertAsync(OfflineSystemTables.OperationsQueue, operation.Serialize(), false, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new LocalStoreException("Failed to update operation in the local store.", ex);
            }
        }

        /// <summary>
        /// Updates a table operation within the operations queue.
        /// </summary>
        /// <param name="id">The ID of the table operation.</param>
        /// <param name="version">The version of the table operation to update.</param>
        /// <param name="item">The new version of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns <c>true</c> if the item has been updated, and false otherwise.</returns>
        public virtual async Task<bool> UpdateAsync(string id, long version, JObject item, CancellationToken cancellationToken = default)
        {
            try
            {
                TableOperation operation = await GetOperationAsync(id, cancellationToken).ConfigureAwait(false);
                if (operation == null || operation.Version != version)
                {
                    return false;
                }
                operation.Version++;
                operation.State = TableOperationState.Pending;
                operation.Item = operation.Kind == TableOperationKind.Delete ? item : null;

                await UpdateAsync(operation, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                throw new LocalStoreException("Failed to update operation in the local store", ex);
            }
        }

        /// <summary>
        /// Creates a comparison node for adding into a query expression tree.
        /// </summary>
        /// <param name="kind">The comparison operator.</param>
        /// <param name="member">The member name.</param>
        /// <param name="value">The value of the member.</param>
        /// <returns>A <see cref="BinaryOperatorNode"/> representing the comparison.</returns>
        private static BinaryOperatorNode Compare(BinaryOperatorKind kind, string member, object value)
            => new(kind, new MemberAccessNode(null, member), new ConstantNode(value));
    }
}
