// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Exceptions;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Operations
{
    /// <summary>
    /// The base type for the various table operations:
    /// * <see cref="DeleteOperation"/>
    /// * <see cref="InsertOperation"/>
    /// * <see cref="UpdateOperation"/>
    /// </summary>
    public abstract class TableOperation : ITableOperation
    {
        /// <summary>
        /// The definition of the table that can store the serialized content for this model.
        /// </summary>
        internal static readonly JObject TableDefinition = new()
        {
            { SystemProperties.JsonIdProperty, string.Empty },
            { "kind", 0 },
            { "state", 0 },
            { "tableName", string.Empty },
            { "itemId", string.Empty },
            { "item", string.Empty },
            { "sequence", 0 },
            { "version", 0 }
        };

        /// <summary>
        /// Creates a new <see cref="TableOperation"/>.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="itemId">The ID of the item affected by the operation.</param>
        protected TableOperation(string tableName, string itemId)
        {
            Id = Guid.NewGuid().ToString("N");
            State = TableOperationState.Pending;
            TableName = tableName;
            ItemId = itemId;
            Version = 1;
        }

        /// <summary>
        /// If <c>true</c>, the operation can write the result of the operation to the store.
        /// </summary>
        public virtual bool CanWriteResultToStore => true;

        /// <summary>
        /// The ID of the operation.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// If <c>true</c>, the operation has been cancelled.
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// If <c>true</c>, the operation has been updated.
        /// </summary>
        public bool IsUpdated { get; private set; }

        /// <summary>
        /// The ID of the item affected by the operation.
        /// </summary>
        public string ItemId { get; }

        /// <summary>
        /// The sequence number of the operation in the queue.
        /// </summary>
        public long Sequence { get; set; }

        /// <summary>
        /// If <c>true</c>, serialize the item to the queue to store it persistently.
        /// </summary>
        public virtual bool SerializeItemToQueue => false;

        /// <summary>
        /// The remote table reference for the table being affected by the operation.
        /// </summary>
        internal RemoteTable Table { get; set; }

        /// <summary>
        /// The name of the table being affected by the operation.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The version of the operation.
        /// </summary>
        public long Version { get; set; }

        #region ITableOperation
        /// <summary>
        /// The kind of operation
        /// </summary>
        public abstract TableOperationKind Kind { get; }

        /// <summary>
        /// The state of the operation
        /// </summary>
        public TableOperationState State { get; internal set; }

        /// <summary>
        /// The table that the operation will be executed against.
        /// </summary>
        IRemoteTable ITableOperation.Table => Table;

        /// <summary>
        /// The item associated with the operation.
        /// </summary>
        public JObject Item { get; set; }

        /// <summary>
        /// Abort the parent push operation.
        /// </summary>
        public void AbortPush()
            => throw new PushAbortedException();

        /// <summary>
        /// Deserialize a record from the persistent store into a table operation.
        /// </summary>
        /// <param name="obj">The object to deserialize.</param>
        /// <returns>The table operation represented by the object.</returns>
        internal static TableOperation Deserialize(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var kind = (TableOperationKind)obj.Value<int>("kind");
            string tableName = obj.Value<string>("tableName");
            string itemId = obj.Value<string>("itemId");
            string itemJson = obj.Value<string>("item");

            TableOperation operation = kind switch
            {
                TableOperationKind.Delete => new DeleteOperation(tableName, itemId),
                TableOperationKind.Insert => new InsertOperation(tableName, itemId),
                TableOperationKind.Update => new UpdateOperation(tableName, itemId),
                _ => null
            };

            if (operation != null)
            {
                operation.Id = obj.Value<string>(SystemProperties.JsonIdProperty);
                operation.Sequence = obj.Value<long?>("sequence").GetValueOrDefault();
                operation.Version = obj.Value<long?>("version").GetValueOrDefault();
                operation.Item = string.IsNullOrEmpty(itemJson) ? null : JObject.Parse(itemJson);
                operation.State = (TableOperationState)obj.Value<int?>("state").GetValueOrDefault();
            }

            return operation;
        }

        /// <summary>
        /// Executes the operation against remote table.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        public async Task<JObject> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (IsCancelled)
            {
                return null;
            }
            if (Item == null)
            {
                throw new DatasyncInvalidOperationException("A table operation must have an item associated with it.");
            }

            JToken response = await ExecuteOperationAsync(cancellationToken).ConfigureAwait(false);
            if (response is JObject result)
            {
                return result;
            }
            throw new DatasyncInvalidOperationException("Datasync table operation returned an unexpected response.");
        }
        #endregion

        /// <summary>
        /// Marks this operation as cancelled.
        /// </summary>
        internal void Cancel()
        {
            IsCancelled = true;
        }

        /// <summary>
        /// Collapse this operation with a new operation by cancellation of either operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        public abstract void CollapseOperation(TableOperation newOperation);

        /// <summary>
        /// The internal version of the <see cref="ExecuteAsync"/> method.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        protected abstract Task<JToken> ExecuteOperationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the operation on the offline store.
        /// </summary>
        /// <param name="store">The offline store.</param>
        /// <param name="item">The item to use for the store operation.</param>
        /// <returns>A task that completes when the store operation is completed.</returns>
        public abstract Task ExecuteOperationOnOfflineStoreAsync(IOfflineStore store, JObject item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serialize this table operation for storage in the persisted operations queue.
        /// </summary>
        /// <returns>The <see cref="JObject"/> representing this table operation.</returns>
        internal JObject Serialize() => new()
        {
            { SystemProperties.JsonIdProperty, Id },
            { "kind", (int)Kind },
            { "state", (int)State },
            { "tableName", TableName },
            { "itemId", ItemId },
            { "item", Item != null && SerializeItemToQueue ? Item.ToString(Formatting.None) : null },
            { "sequence", Sequence },
            { "version", Version }
        };

        /// <summary>
        /// Marks this operation as updated.
        /// </summary>
        internal void Update()
        {
            Version++;
            IsUpdated = true;
        }

        /// <summary>
        /// Validates that the operation can collapse with a new operation.
        /// </summary>
        /// <param name="newOperation">The new operation.</param>
        /// <exception cref="InvalidOperationException">when the operation cannot collapse with the new operation.</exception>
        public abstract void ValidateOperationCanCollapse(TableOperation newOperation);
    }
}
