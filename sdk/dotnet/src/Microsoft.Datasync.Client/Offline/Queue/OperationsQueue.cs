// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    /// <summary>
    /// The operations queue - a queue of operations waiting to be sent to the
    /// remote service.
    /// </summary>
    internal class OperationsQueue
    {
        /// <summary>
        /// Backing store for the equivalent properties.
        /// </summary>
        private long pendingOperations, sequenceId;

        /// <summary>
        /// Lock for the store.
        /// </summary>
        private readonly AsyncLock mutex = new();

        /// <summary>
        /// Creates a new <see cref="OperationsQueue"/>
        /// </summary>
        /// <param name="store">The offline store to use for persistent storage.</param>
        internal OperationsQueue(IOfflineStore store)
        {
            OfflineStore = store;
        }

        /// <summary>
        /// Set to <c>true</c> when the operations queue is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The offline store being used for persistent storage.
        /// </summary>
        public IOfflineStore OfflineStore { get; }

        /// <summary>
        /// The number of pending operations within the operations queue.
        /// </summary>
        public long PendingOperations { get => pendingOperations; }

        /// <summary>
        /// The next sequence ID to be used.
        /// </summary>
        internal long SequenceId { get => sequenceId; }

        /// <summary>
        /// Deletes an operation from the operations queue.
        /// </summary>
        /// <param name="id">The ID of the operation.</param>
        /// <param name="version">The version of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is removed.</returns>
        public async Task<bool> DeleteOperationByIdAsync(string id, long version, CancellationToken cancellationToken = default)
        {
            try
            {
                var operation = await GetOperationByIdAsync(id, cancellationToken).ConfigureAwait(false);
                if (operation == null || operation.Version != version)
                {
                    return false;
                }

                await OfflineStore.DeleteAsync(SystemTables.OperationsQueue, new[] { id }, cancellationToken).ConfigureAwait(false);
                Interlocked.Decrement(ref pendingOperations);
                return true;
            }
            catch (Exception ex)
            {
                throw new OfflineStoreException("Failed to delete the operation from the local store", ex);
            }
        }

        /// <summary>
        /// Places a new operation into the queue.
        /// </summary>
        /// <param name="operation">The operation to enqueue.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is enqueued.</returns>
        public async Task EnqueueOperationAsync(TableOperation operation, CancellationToken cancellationToken = default)
        {
            operation.Sequence = Interlocked.Increment(ref sequenceId);
            await OfflineStore.UpsertAsync(SystemTables.OperationsQueue, new[] { operation.Serialize() }, false, cancellationToken).ConfigureAwait(false);
            Interlocked.Increment(ref pendingOperations);
        }

        /// <summary>
        /// Retrieves a table operation within the operations queue based on the ID of the operation.
        /// </summary>
        /// <param name="id">The ID of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the table operation when complete.</returns>
        public virtual async Task<TableOperation> GetOperationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            JObject operation = await OfflineStore.GetItemAsync(SystemTables.OperationsQueue, id, cancellationToken).ConfigureAwait(false);
            return operation == null ? null : TableOperation.Deserialize(operation);

        }

        /// <summary>
        /// Retrieves an existing operation (by table/ID) from the operations queue.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="itemId">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the existing operation, or <c>null</c> if no operation exists when complete.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TableOperation> GetOperationByItemIdAsync(string tableName, string itemId, CancellationToken cancellationToken = default)
        {
            QueryDescription query = new(SystemTables.OperationsQueue)
            {
                Filter = new BinaryOperatorNode(BinaryOperatorKind.And, Compare(BinaryOperatorKind.Equal, "tableName", tableName), Compare(BinaryOperatorKind.Equal, "itemId", itemId))
            };
            var page = await OfflineStore.GetPageAsync(query, cancellationToken).ConfigureAwait(false);
            return page.Items?.FirstOrDefault() is JObject operation ? TableOperation.Deserialize(operation) : null;
        }

        /// <summary>
        /// Initializes the operations queue using data from the persistent store.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operations queue is initialized.</returns>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            using (await mutex.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                if (IsInitialized)
                {
                    return;
                }

                var query = new QueryDescription(SystemTables.OperationsQueue) { IncludeTotalCount = true, Top = 1 };
                query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "sequence"), OrderByDirection.Descending));

                Page<JObject> page = await OfflineStore.GetPageAsync(query, cancellationToken).ConfigureAwait(false);
                pendingOperations = page.Count ?? 0;
                sequenceId = page.Items?.Select(v => v.Value<long>("sequence")).FirstOrDefault() ?? 0;

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Updates an operation within the operations queue.
        /// </summary>
        /// <param name="operation">The operation to update.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is updated.</returns>
        public async Task UpdateOperationAsync(TableOperation operation, CancellationToken cancellationToken = default)
        {
            try
            {
                await OfflineStore.UpsertAsync(SystemTables.OperationsQueue, new[] { operation.Serialize() }, false, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new OfflineStoreException("Failed to update operation in the local store.", ex);
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
