// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    /// <summary>
    /// Representation of an insert operation within the operations queue.
    /// </summary>
    internal class InsertOperation : TableOperation
    {
        /// <summary>
        /// Creates a new <see cref="InsertOperation"/> object.
        /// </summary>
        /// <param name="tableName">The name of the table that contains the affected item.</param>
        /// <param name="itemId">The ID of the affected item.</param>
        internal InsertOperation(string tableName, string itemId)
            : base(TableOperationKind.Insert, tableName, itemId)
        {
        }

        /// <summary>
        /// Collapse this operation with a new operation by cancellation of either operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        public override void CollapseOperation(TableOperation newOperation)
        {
            if (newOperation.ItemId != ItemId)
            {
                throw new ArgumentException("ItemId does not match", nameof(newOperation));
            }

            // An insert followed by a delete is a no-op, so cancel both operations.
            if (newOperation is DeleteOperation)
            {
                Cancel();
                newOperation.Cancel();
            }

            // An insert followed by an update is still an insert, so cancel the update.
            if (newOperation is UpdateOperation)
            {
                Update();
                newOperation.Cancel();
            }
        }

        /// <summary>
        /// Executes the operation on the offline store.
        /// </summary>
        /// <param name="store">The offline store.</param>
        /// <param name="item">The item to use for the store operation.</param>
        /// <returns>A task that completes when the store operation is completed.</returns>
        public override async Task ExecuteOperationOnOfflineStoreAsync(IOfflineStore store, JObject item, CancellationToken cancellationToken = default)
        {
            var existingItem = await store.GetItemAsync(TableName, ItemId, cancellationToken).ConfigureAwait(false);
            if (existingItem != null)
            {
                throw new OfflineStoreException("Insert failed - the item already exists in the offline store");
            }
            await store.UpsertAsync(TableName, new[] { item }, false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the operation against the remote table.
        /// </summary>
        /// <param name="table">The <see cref="IRemoteTable"/> to use for communication with the backend.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        protected override Task<JToken> ExecuteRemoteOperationAsync(IRemoteTable table, CancellationToken cancellationToken)
            => table.InsertItemAsync(Item, cancellationToken);

        /// <summary>
        /// Validates that the operation can collapse with a new operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        /// <exception cref="InvalidOperationException">when the operation cannot collapse with the new operation.</exception>
        public override void ValidateOperationCanCollapse(TableOperation newOperation)
        {
            if (newOperation.ItemId != ItemId)
            {
                throw new ArgumentException($"Cannot collapse insert operation '{Id}' with '{newOperation.Id}' - Item IDs do not match", nameof(newOperation));
            }
            // Insert followed by an Insert is not allowed.
            if (newOperation is InsertOperation)
            {
                throw new InvalidOperationException($"An insert operation on item '{ItemId}' already exists in the operations queue.");
            }
            // Insert followed by Delete is allowed, but only if the Insert has not been tried yet.
            // If it has been tried, it may not have completed successfully, so we have to ensure
            // a consistent offline state.
            if (newOperation is DeleteOperation && State != TableOperationState.Pending)
            {
                throw new InvalidOperationException("Cannot process deletion because there is an in-progress insert operation.  Complete a 'Push' operation first.");
            }
        }
    }
}
