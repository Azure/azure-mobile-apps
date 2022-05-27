// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    /// <summary>
    /// Representation of a delete operation within the operations queue.
    /// </summary>
    internal class DeleteOperation : TableOperation
    {
        /// <summary>
        /// Creates a new <see cref="DeleteOperation"/> object.
        /// </summary>
        /// <param name="tableName">The name of the table that contains the affected item.</param>
        /// <param name="itemId">The ID of the affected item.</param>
        internal DeleteOperation(string tableName, string itemId)
            : base(TableOperationKind.Delete, tableName, itemId)
        {
        }

        /// <summary>
        /// If <c>true</c>, the operation can write the result of the operation to the store.
        /// </summary>
        public override bool CanWriteResultToStore => false;

        /// <summary>
        /// If <c>true</c>, serialize the item to the quuee to store it persistently.
        /// </summary>
        public override bool SerializeItemToQueue => true;

        /// <summary>
        /// Collapse this operation with a new operation by cancellation of either operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        public override void CollapseOperation(TableOperation newOperation)
        {
            // There is nothing to collapse; we do not allow any operation after a delete.
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
        /// Internal version of the <see cref="TableOperation.ExecuteOperationOnRemoteServiceAsync(DatasyncClient, CancellationToken)"/>, to execute
        /// the operation against a remote table.
        /// </summary>
        /// <param name="table">The remote table connection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        protected override async Task<JToken> ExecuteRemoteOperationAsync(IRemoteTable table, CancellationToken cancellationToken)
        {
            try
            {
                await table.DeleteItemAsync(Item, cancellationToken).ConfigureAwait(false);
                return null;
            }
            catch (DatasyncInvalidOperationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound || ex.Response?.StatusCode == HttpStatusCode.Gone)
            {
                // The item no longer exists on the server, so skip the deletion.
                return null;
            }
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
                throw new ArgumentException($"Cannot collapse delete operation '{Id}' with '{newOperation.Id}' - Item IDs do not match", nameof(newOperation));
            }

            // We do not allow any operations on an object that has already been deleted.
            throw new InvalidOperationException($"A delete operation on item '{Id}' is already in the queue.");
        }
    }
}
