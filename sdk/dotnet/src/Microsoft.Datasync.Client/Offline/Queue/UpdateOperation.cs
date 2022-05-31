// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    internal class UpdateOperation : TableOperation
    {
        /// <summary>
        /// Creates a new <see cref="UpdateOperation"/> object.
        /// </summary>
        /// <param name="tableName">The name of the table that contains the affected item.</param>
        /// <param name="itemId">The ID of the affected item.</param>
        internal UpdateOperation(string tableName, string itemId)
            : base(TableOperationKind.Update, tableName, itemId)
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
                throw new ArgumentException($"Cannot collapse update operation '{Id}' with '{newOperation.Id}' - Item IDs do not match", nameof(newOperation));
            }
            // An update followed by a delete is still a delete.  Cancel the update.
            if (newOperation is DeleteOperation)
            {
                Cancel();
                newOperation.Update();
            }
            // An update followed by another update is still an update.  Cancel the
            // second update and update the first update.
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
            var itemId = ServiceSerializer.GetId(item);
            var originalItem = await store.GetItemAsync(TableName, itemId, cancellationToken).ConfigureAwait(false);
            if (originalItem == null)
            {
                throw new OfflineStoreException($"Item with ID '{itemId}' does not exist in the offline store.");
            }
            await store.UpsertAsync(TableName, new[] { item }, false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Internal version of the <see cref="TableOperation.ExecuteOperationOnRemoteServiceAsync(DatasyncClient, CancellationToken)"/>, to execute
        /// the operation against a remote table.
        /// </summary>
        /// <param name="table">The remote table connection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result (or <c>null</c>) when complete.</returns>
        protected override Task<JToken> ExecuteRemoteOperationAsync(IRemoteTable table, CancellationToken cancellationToken)
            => table.ReplaceItemAsync(Item, cancellationToken);

        /// <summary>
        /// Validates that the operation can collapse with a new operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        /// <exception cref="InvalidOperationException">when the operation cannot collapse with the new operation.</exception>
        public override void ValidateOperationCanCollapse(TableOperation newOperation)
        {
            if (newOperation.ItemId != ItemId)
            {
                throw new ArgumentException($"Cannot collapse update operation '{Id}' with '{newOperation.Id}' - Item IDs do not match", nameof(newOperation));
            }
            if (newOperation is InsertOperation)
            {
                throw new InvalidOperationException($"An update operation on item '{ItemId}' already exists in the operations queue.");
            }
        }
    }
}
