// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        /// Acquires the queue lock so that write operations are not interrupted.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>An <see cref="IDisposable"/> to release the lock.</returns>
        internal Task<IDisposable> AcquireLockAsync(CancellationToken cancellationToken)
            => mutex.AcquireAsync(cancellationToken);

        /// <summary>
        /// Counts the number of operations in the queue for a specific table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The number of operations in the queue for the table.</returns>
        public async Task<long> CountPendingOperationsAsync(string tableName, CancellationToken cancellationToken = default)
        {
            EnsureQueueIsInitialized();
            QueryDescription query = new(SystemTables.OperationsQueue)
            {
                Filter = Compare(BinaryOperatorKind.Equal, "tableName", tableName),
                IncludeTotalCount = true,
                Top = 0
            };
            var page = await OfflineStore.GetPageAsync(query, cancellationToken).ConfigureAwait(false);
            return page.Count ?? 0;
        }

        /// <summary>
        /// Deletes an operation from the operations queue.
        /// </summary>
        /// <param name="id">The ID of the operation.</param>
        /// <param name="version">The version of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is removed.</returns>
        public async Task<bool> DeleteOperationByIdAsync(string id, long version, CancellationToken cancellationToken = default)
        {
            using (await AcquireLockAsync(cancellationToken).ConfigureAwait(false))
            {
                EnsureQueueIsInitialized();
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
        }

        /// <summary>
        /// Deletes operations in the queue identified by the query.
        /// </summary>
        /// <param name="query">The query identifying the operations to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task DeleteOperationsAsync(QueryDescription query, CancellationToken cancellationToken = default)
        {
            using (await AcquireLockAsync(cancellationToken).ConfigureAwait(false))
            {
                EnsureQueueIsInitialized();
                if (query.TableName != SystemTables.OperationsQueue)
                {
                    throw new InvalidOperationException("To delete an operation in the operations queue by query, ensure the operations queue is the target.");
                }
                await OfflineStore.DeleteAsync(query, cancellationToken).ConfigureAwait(false);
                await UpdatePendingOperationsAsync(cancellationToken).ConfigureAwait(false);
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
            using (await AcquireLockAsync(cancellationToken).ConfigureAwait(false))
            {
                EnsureQueueIsInitialized();
                operation.Sequence = Interlocked.Increment(ref sequenceId);
                await OfflineStore.UpsertAsync(SystemTables.OperationsQueue, new[] { operation.Serialize() }, false, cancellationToken).ConfigureAwait(false);
                Interlocked.Increment(ref pendingOperations);
            }
        }

        /// <summary>
        /// Execute a query against an offline table, returning a pageable response.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the results when the query finishes.</returns>
        internal AsyncPageable<TableOperation> GetAsyncOperations(string query, CancellationToken cancellationToken = default)
        {
            EnsureQueueIsInitialized();
            return new FuncAsyncPageable<TableOperation>(nextLink => GetNextPageOfOperationsAsync(query, nextLink, cancellationToken));
        }

        /// <summary>
        /// Gets the next page of results in a search for operations in the queue.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="nextLink">The URI to the next link of results.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of results when complete.</returns>
        private async Task<Page<TableOperation>> GetNextPageOfOperationsAsync(string query, string nextLink, CancellationToken cancellationToken = default)
        {
            var queryDescription = QueryDescription.Parse(SystemTables.OperationsQueue, nextLink != null ? new Uri(nextLink).Query.TrimStart('?') : query);
            var rawPage = await OfflineStore.GetPageAsync(queryDescription, cancellationToken).ConfigureAwait(false);
            return new Page<TableOperation>()
            {
                Count = rawPage.Count,
                Items = rawPage.Items.Select(x => TableOperation.Deserialize(x)).ToArray(),
                NextLink = rawPage.NextLink
            };
        }

        /// <summary>
        /// Retrieves a table operation within the operations queue based on the ID of the operation.
        /// </summary>
        /// <param name="id">The ID of the operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the table operation when complete.</returns>
        public virtual async Task<TableOperation> GetOperationByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            EnsureQueueIsInitialized();
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
        public async Task<TableOperation> GetOperationByItemIdAsync(string tableName, string itemId, CancellationToken cancellationToken = default)
        {
            EnsureQueueIsInitialized();
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
            using (await AcquireLockAsync(cancellationToken).ConfigureAwait(false))
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
        /// Updates the pendingOperations field securely.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is complete.</returns>
        private async Task UpdatePendingOperationsAsync(CancellationToken cancellationToken = default)
        {
            var query = new QueryDescription(SystemTables.OperationsQueue) { IncludeTotalCount = true, Top = 1 };
            Page<JObject> page = await OfflineStore.GetPageAsync(query, cancellationToken).ConfigureAwait(false);
            Interlocked.Exchange(ref pendingOperations, page.Count ?? 0);
        }

        /// <summary>
        /// Peeks inside the operations queue for the next operation (after the given sequence ID)targeting the set of tables
        /// </summary>
        /// <param name="previousSequenceId">The previous sequence ID read.</param>
        /// <param name="tableNames">The list of tables to consider.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the table operation when finished.</returns>
        public virtual async Task<TableOperation> PeekAsync(long previousSequenceId, IEnumerable<string> tableNames, CancellationToken cancellationToken = default)
        {
            if (previousSequenceId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(previousSequenceId), "Sequence ID must be a positive integer");
            }

            QueryDescription query = new(SystemTables.OperationsQueue)
            {
                Filter = Compare(BinaryOperatorKind.GreaterThan, "sequence", previousSequenceId),
                Top = 1
            };
            query.Ordering.Add(new OrderByNode(new MemberAccessNode(null, "sequence"), OrderByDirection.Ascending));

            if (tableNames?.Any() == true)
            {
                BinaryOperatorNode nameInList = tableNames
                    .Select(t => Compare(BinaryOperatorKind.Equal, "tableName", t))
                    .Aggregate((first, second) => new BinaryOperatorNode(BinaryOperatorKind.Or, first, second));
                query.Filter = new BinaryOperatorNode(BinaryOperatorKind.And, query.Filter, nameInList);
            }
            var page = await OfflineStore.GetPageAsync(query, cancellationToken).ConfigureAwait(false);
            JToken result = page.Items?.FirstOrDefault();
            return (result != null && result is JObject op) ? TableOperation.Deserialize(op) : null;
        }

        /// <summary>
        /// Tries to update the operation state within the queue, and throws an error if it can't/
        /// </summary>
        /// <param name="operation">The operation to update.</param>
        /// <param name="state">The new state of the operation.</param>
        /// <param name="batch">An <see cref="OperationBatch"/> to report any failure..</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual async Task TryUpdateOperationStateAsync(TableOperation operation, TableOperationState state, OperationBatch batch = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(operation, nameof(operation));
            try
            {
                operation.State = state;
                await UpdateOperationAsync(operation, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                batch?.Abort(PushStatus.CancelledByOfflineStoreError);
                throw new OfflineStoreException($"Failed to set operation state for QID '{operation.Id}' to '{state}' in the offline store.", ex);
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
            using (await AcquireLockAsync(cancellationToken).ConfigureAwait(false))
            {
                EnsureQueueIsInitialized();
                try
                {
                    await OfflineStore.UpsertAsync(SystemTables.OperationsQueue, new[] { operation.Serialize() }, false, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new OfflineStoreException("Failed to update operation in the local store.", ex);
                }
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
        public virtual async Task<bool> UpdateOperationAsync(string id, long version, JObject item, CancellationToken cancellationToken = default)
        {
            try
            {
                TableOperation operation = await GetOperationByIdAsync(id, cancellationToken).ConfigureAwait(false);
                if (operation == null || operation.Version != version)
                {
                    return false;
                }
                operation.Version++;
                operation.State = TableOperationState.Pending;
                operation.Item = operation.Kind == TableOperationKind.Delete ? item : null;

                await UpdateOperationAsync(operation, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                throw new OfflineStoreException($"Failed to update operation '{id}' in the offline store", ex);
            }
        }

        /// <summary>
        /// Ensures the queue is initialized.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The operations queue is not initialized.</exception>
        private void EnsureQueueIsInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("The operations queue is not initialized.");
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
