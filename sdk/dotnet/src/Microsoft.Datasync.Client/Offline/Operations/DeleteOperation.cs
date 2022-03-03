// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Operations
{
    /// <summary>
    /// A <see cref="TableOperation"/> representing a deletion from a table.
    /// </summary>
    internal class DeleteOperation : TableOperation
    {
        /// <summary>
        /// Create a new deletion operation.
        /// </summary>
        /// <param name="tableName">The name of the table being affected.</param>
        /// <param name="itemId">The ID of the item being affected.</param>
        public DeleteOperation(string tableName, string itemId) : base(tableName, itemId)
        {
        }

        /// <summary>
        /// If <c>true</c>, the operation can write the result of the operation to the store.
        /// </summary>
        /// <remarks>
        /// The operation cannot write the result back to the offline store because the item
        /// will have been deleted already.
        /// </remarks>
        public override bool CanWriteResultToStore => false;

        /// <summary>
        /// The kind of operation
        /// </summary>
        public override TableOperationKind Kind => TableOperationKind.Delete;

        /// <summary>
        /// If <c>true</c>, serialize the item to the queue to store it persistently.
        /// </summary>
        /// <remarks>
        /// We need to serialize the item to the queue in case the operation is cancelled; we
        /// can then restore the item to the offline store.
        /// </remarks>
        public override bool SerializeItemToQueue => true;

        /// <summary>
        /// Collapse this operation with a new operation by cancellation of either operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        public override void CollapseOperation(TableOperation newOperation)
        {
            // Nothing to collapse as we do not allow any operation after a delete.
        }

        /// <summary>
        /// Executes the operation on the offline store.
        /// </summary>
        /// <param name="store">The offline store.</param>
        /// <param name="item">The item to use for the store operation.</param>
        /// <returns>A task that completes when the store operation is completed.</returns>
        public override Task ExecuteOperationOnOfflineStoreAsync(IOfflineStore store, JObject item, CancellationToken cancellationToken = default)
            => store.DeleteAsync(TableName, new[] { ItemId }, cancellationToken);

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
            throw new InvalidOperationException("A delete operation on this item is already in the queue.");
        }

        /// <summary>
        /// The internal version of the <see cref="ExecuteAsync"/> method.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        protected override async Task<JToken> ExecuteOperationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await Table.DeleteItemAsync(Item, cancellationToken).ConfigureAwait(false);
            }
            catch (DatasyncInvalidOperationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
