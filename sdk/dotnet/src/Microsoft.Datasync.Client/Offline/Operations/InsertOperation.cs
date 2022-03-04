// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Operations
{
    /// <summary>
    /// A <see cref="TableOperation"/> representing an insertion into a table.
    /// </summary>
    internal class InsertOperation : TableOperation
    {
        /// <summary>
        /// Create a new insert operation.
        /// </summary>
        /// <param name="tableName">The name of the table being affected.</param>
        /// <param name="itemId">The ID of the item being affected.</param>
        public InsertOperation(string tableName, string itemId) : base(tableName, itemId)
        {
        }

        /// <summary>
        /// The kind of operation
        /// </summary>
        public override TableOperationKind Kind => TableOperationKind.Insert;

        /// <summary>
        /// Collapse this operation with a new operation by cancellation of either operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        public override void CollapseOperation(TableOperation newOperation)
        {
            if (newOperation.ItemId != ItemId)
            {
                throw new ArgumentException("ItemId of new operation does not match existing operation", nameof(newOperation));
            }

            if (newOperation is DeleteOperation)
            {
                // An insert followed by a delete is a no-op, so cancel both operations.
                Cancel();
                newOperation.Cancel();
            }
            else if (newOperation is UpdateOperation)
            {
                // An insert followed by an update is still an insert, but with new data
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
                // This **should** never happen due to the collapsing rules.
                throw new OfflineStoreException("An insert operation on the item is already in the queue.");
            }
            await store.UpsertAsync(TableName, item, false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates that the operation can collapse with a new operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        /// <exception cref="InvalidOperationException">when the operation cannot collapse with the new operation.</exception>
        public override void ValidateOperationCanCollapse(TableOperation newOperation)
        {
            if (newOperation.ItemId != ItemId)
            {
                throw new ArgumentException("ItemId does not match this operation", nameof(newOperation));
            }
            if (newOperation is InsertOperation)
            {
                throw new InvalidOperationException("An insert operation on the item is already in thwe queue.");
            }
            if (newOperation is DeleteOperation && State != TableOperationState.Pending)
            {
                // If the insert was attempted, then we can't be usre it was applied, so we can't collpase the delete until changes are pushed.
                throw new InvalidOperationException("The item is inconsistent with the local store.  Call PushAsync() before deleting the item");
            }
        }

        /// <summary>
        /// The internal version of the <see cref="ExecuteAsync"/> method.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        protected override async Task<JToken> ExecuteOperationAsync(CancellationToken cancellationToken = default)
        {
            JObject item = ServiceSerializer.RemoveSystemProperties(Item, out _);
            return await Table.InsertItemAsync(item, cancellationToken).ConfigureAwait(false);
        }
    }
}
