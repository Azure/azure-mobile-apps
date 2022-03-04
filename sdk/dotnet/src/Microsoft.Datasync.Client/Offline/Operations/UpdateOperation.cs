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
    /// A <see cref="TableOperation"/> representing an update of an item in a table.
    /// </summary>
    internal class UpdateOperation : TableOperation
    {
        /// <summary>
        /// Create a new update operation.
        /// </summary>
        /// <param name="tableName">The name of the table being affected.</param>
        /// <param name="itemId">The ID of the item being affected.</param>
        public UpdateOperation(string tableName, string itemId) : base(tableName, itemId)
        {
        }

        /// <summary>
        /// The kind of operation
        /// </summary>
        public override TableOperationKind Kind => TableOperationKind.Update;

        /// <summary>
        /// Collapse this operation with a new operation by cancellation of either operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        public override void CollapseOperation(TableOperation newOperation)
        {
            if (newOperation.ItemId != ItemId)
            {
                throw new ArgumentException("ItemId does not match this operation", nameof(newOperation));
            }
            if (newOperation is DeleteOperation)
            {
                // If we update, then delete - we are deleting.
                Cancel();
                newOperation.Update();
            }
            else if (newOperation is UpdateOperation)
            {
                // if we update, then update - we are still updating.
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
        public override Task ExecuteOperationOnOfflineStoreAsync(IOfflineStore store, JObject item, CancellationToken cancellationToken = default)
            => store.UpsertAsync(TableName, item, false, cancellationToken);

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
                throw new InvalidOperationException("an update operation on the item is already in the queue.");
            }
        }

        /// <summary>
        /// The internal version of the <see cref="ExecuteAsync"/> method.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        protected override Task<JToken> ExecuteOperationAsync(CancellationToken cancellationToken = default)
            => Table.ReplaceItemAsync(Item, cancellationToken);
    }
}
