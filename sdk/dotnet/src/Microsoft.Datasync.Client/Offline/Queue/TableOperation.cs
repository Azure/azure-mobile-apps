// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    /// <summary>
    /// The table operation kind.
    /// </summary>
    public enum TableOperationKind
    {
        Unknown,
        Delete,
        Insert,
        Update
    }

    /// <summary>
    /// The operational states for a table operation.
    /// </summary>
    public enum TableOperationState
    {
        Pending,
        Attempted,
        Completed,
        Failed
    };

    /// <summary>
    /// The base type for the various table operations.
    /// </summary>
    public abstract class TableOperation : IEquatable<TableOperation>
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
        /// <param name="kind">The kind of operation (delete, insert, update).</param>
        /// <param name="tableName">Name of the table holding the affected item.</param>
        /// <param name="itemId">The ID of the affected item.</param>
        protected TableOperation(TableOperationKind kind, string tableName, string itemId)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            Arguments.IsValidId(itemId, nameof(itemId));

            Kind = kind;
            Id = Guid.NewGuid().ToString("N");
            State = TableOperationState.Pending;
            TableName = tableName;
            ItemId = itemId;
            Version = 1;
        }

        /// <summary>
        /// If <c>true</c>, the operation can write the result of the operation to the store.
        /// </summary>
        [JsonIgnore]
        public virtual bool CanWriteResultToStore => true;

        /// <summary>
        /// The kind of operation
        /// </summary>
        [JsonProperty("kind")]
        public TableOperationKind Kind { get; }

        /// <summary>
        /// The unique ID of the operation.
        /// </summary>
        [JsonProperty(SystemProperties.JsonIdProperty)]
        public string Id { get; set; }

        /// <summary>
        /// If <c>true</c>, the operation can be cancelled.
        /// </summary>
        [JsonIgnore]
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// If <c>true</c>, the operation has been updated.
        /// </summary>
        [JsonIgnore]
        public bool IsUpdated { get; private set; }

        /// <summary>
        /// The ID of the item affected by the operation.
        /// </summary>
        [JsonProperty("itemId")]
        public string ItemId { get; }

        /// <summary>
        /// The item that is affected by this operation.
        /// </summary>
        [JsonIgnore]
        public JObject Item { get; set; }

        /// <summary>
        /// The <see cref="Item"/> as a string, for serialization and deserialization purposes.
        /// You should not have to set it yourself.
        /// </summary>
        [JsonProperty("item")]
        internal string JsonItem
        {
            get => Item != null && SerializeItemToQueue ? Item.ToString(Formatting.None) : null;
            set => Item = string.IsNullOrEmpty(value) ? null : JObject.Parse(value);
        }

        /// <summary>
        /// The sequence number of the operation in the queue.
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        /// <summary>
        /// If <c>true</c>, serialize the item to the quuee to store it persistently.
        /// </summary>
        [JsonIgnore]
        public virtual bool SerializeItemToQueue => false;

        /// <summary>
        /// The state of the operation.
        /// </summary>
        [JsonProperty("state")]
        public TableOperationState State { get; internal set; }

        /// <summary>
        /// The name of the table being affected by the operation.
        /// </summary>
        [JsonProperty("tableName")]
        public string TableName { get; }

        /// <summary>
        /// The version of the operation.
        /// </summary>
        [JsonProperty("version")]
        public long Version { get; set; }

        /// <summary>
        /// Abort the parent push operation.
        /// </summary>
        /// <exception cref="PushAbortedException"></exception>
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Required to be instance method for reflection.")]
        public void AbortPush() => throw new PushAbortedException();

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

            TableOperation operation = kind switch
            {
                TableOperationKind.Delete => new DeleteOperation(tableName, itemId),
                TableOperationKind.Insert => new InsertOperation(tableName, itemId),
                TableOperationKind.Update => new UpdateOperation(tableName, itemId),
                _ => throw new InvalidOperationException("Invalid operation kind")
            };

            if (operation != null)
            {
                operation.Id = obj.Value<string>(SystemProperties.JsonIdProperty);
                operation.Sequence = obj.Value<long?>("sequence").GetValueOrDefault();
                operation.Version = obj.Value<long?>("version").GetValueOrDefault();
                operation.JsonItem = obj.Value<string>("item");
                operation.State = (TableOperationState)obj.Value<int?>("state").GetValueOrDefault();
            }

            return operation;
        }

        /// <summary>
        /// Executes the operation on the offline store.
        /// </summary>
        /// <param name="store">The offline store.</param>
        /// <param name="item">The item to use for the store operation.</param>
        /// <returns>A task that completes when the store operation is completed.</returns>
        public abstract Task ExecuteOperationOnOfflineStoreAsync(IOfflineStore store, JObject item, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the operation against remote table.
        /// </summary>
        /// <param name="client">The <see cref="DatasyncClient"/> to use for communication with the backend.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        public async Task<JObject> ExecuteOperationOnRemoteServiceAsync(DatasyncClient client, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(client, nameof(client));
            if (IsCancelled)
            {
                return null;
            }
            if (Item == null)
            {
                throw new DatasyncInvalidOperationException("A table operation must have an item associated with it.");
            }

            IRemoteTable table = client.GetRemoteTable(TableName);
            JToken response = await ExecuteRemoteOperationAsync(table, cancellationToken).ConfigureAwait(false);
            var result = response as JObject;
            if (response != null && result == null)
            {
                throw new DatasyncInvalidOperationException("Datasync table operation returned an unexpected response.");
            }
            return result;
        }

        /// <summary>
        /// Internal version of the <see cref="ExecuteOperationOnRemoteServiceAsync(DatasyncClient, CancellationToken)"/>, to execute
        /// the operation against a remote table.
        /// </summary>
        /// <param name="table">The remote table connection.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result (or <c>null</c>) when complete.</returns>
        protected abstract Task<JToken> ExecuteRemoteOperationAsync(IRemoteTable table, CancellationToken cancellationToken);

        /// <summary>
        /// Serializes this object to a <see cref="JObject"/>.
        /// </summary>
        /// <returns>A <see cref="JObject"/> that represents this object.</returns>
        internal JObject Serialize() => JObject.FromObject(this);

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

        #region IEquatable<TableOperation>
        /// <summary>
        /// Determines if another object is equivalent to this object.
        /// </summary>
        /// <param name="other">The comparison object.</param>
        /// <returns><c>true</c> if this object is equivalent to the provided object, <c>false</c> otherwise.</returns>
        public bool Equals(TableOperation other)
            => Kind == other.Kind && ItemId == other.ItemId && JsonItem == other.JsonItem && Sequence == other.Sequence && State == other.State && TableName == other.TableName && Version == other.Version;

        /// <summary>
        /// Determines if another object is equivalent to this object.
        /// </summary>
        /// <param name="obj">The comparison object.</param>
        /// <returns><c>true</c> if this object is equivalent to the provided object, <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
            => obj is TableOperation tableOperation && Equals(tableOperation);

        /// <summary>
        /// Returns the hash code for the instance (which is a numberical value used to insert and
        /// identify an object in a hash-based collection).
        /// </summary>
        /// <returns>The hash code for the item.</returns>
        public override int GetHashCode() => ItemId.GetHashCode();
        #endregion
    }
}
