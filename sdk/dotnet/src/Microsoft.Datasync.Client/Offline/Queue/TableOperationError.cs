// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    /// <summary>
    /// The details of a failed table operation.
    /// </summary>
    public class TableOperationError : IEquatable<TableOperationError>
    {
        /// <summary>
        /// The table definition for the offline store representation.
        /// </summary>
        internal static readonly JObject TableDefinition = new()
        {
            { SystemProperties.JsonIdProperty, string.Empty },
            { "status", 0 },
            { "version", 0 },
            { "kind", 0 },
            { "tableName", string.Empty },
            { "item", string.Empty },
            { "rawResult", string.Empty }
        };

        /// <summary>
        /// Initializes an instance of <see cref="TableOperationError"/>
        /// </summary>
        /// <param name="id">The id of error that is same as operation id.</param>
        /// <param name="operationVersion">The version of the operation.</param>
        /// <param name="operationKind">The kind of table operation.</param>
        /// <param name="status">The HTTP status code returned by server.</param>
        /// <param name="tableName">The name of the remote table.</param>
        /// <param name="item">The item associated with the operation.</param>
        /// <param name="rawResult">Raw response of the table operation.</param>
        /// <param name="result">Response of the table operation.</param>
        public TableOperationError(string id, long operationVersion, TableOperationKind operationKind, HttpStatusCode? status, string tableName, JObject item, string rawResult, JObject result)
        {
            Id = id;
            OperationVersion = operationVersion;
            Status = status;
            OperationKind = operationKind;
            TableName = tableName;
            Item = item;
            RawResult = rawResult;
            Result = result;
        }

        /// <summary>
        /// Initializes an instance of <see cref="TableOperationError"/>
        /// </summary>
        /// <param name="operation">The operation causing the error</param>
        /// <param name="context">The <see cref="SyncContext"/> processing the operation.</param>
        /// <param name="status">The HTTP status code returned by server.</param>
        /// <param name="rawResult">Raw response of the table operation.</param>
        /// <param name="result">Response of the table operation.</param>
        internal TableOperationError(TableOperation operation, SyncContext context, HttpStatusCode? status, string rawResult, JObject result)
            : this(operation.Id, operation.Version, operation.Kind, status, operation.TableName, operation.Item, rawResult, result)
        {
            Context = context;
        }

        /// <summary>
        /// A unique identifier for the error. The Value matches value of the <see cref="TableOperation"/> Id property.
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// Indicates whether error is handled.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// The version of the operation.
        /// </summary>
        internal long OperationVersion { get; }

        /// <summary>
        /// The kind of table operation.
        /// </summary>
        public TableOperationKind OperationKind { get; }

        /// <summary>
        /// The name of the remote table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The item associated with the operation.
        /// </summary>
        public JObject Item { get; }

        /// <summary>
        /// Response of the table operation.
        /// </summary>
        public JObject Result { get; }

        /// <summary>
        /// Raw response of the table operation.
        /// </summary>
        public string RawResult { get; }

        /// <summary>
        /// The HTTP status code returned by server.
        /// </summary>
        /// <remarks>
        /// This is nullable because this error can also occur if the handler throws an exception
        /// </remarks>
        public HttpStatusCode? Status { get; }

        /// <summary>
        /// The synchronization context handling the operation.
        /// </summary>
        internal SyncContext Context { get; set; }

        /// <summary>
        /// Cancels the table operation and updates the local instance of the item with the given item.
        /// </summary>
        /// <param name="item">The item to update in local store.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>Task that completes when operation is cancelled.</returns>
        public async Task CancelAndUpdateItemAsync(JObject item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            await Context.CancelAndUpdateItemAsync(this, item, cancellationToken).ConfigureAwait(false);
            Handled = true;
        }

        /// <summary>
        /// Updates the table operation and updates the local instance of the item with the given item.
        /// Clears the error and sets the operation state to pending.
        /// </summary>
        /// <param name="item">The item to update in local store.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>Task that completes when operation is updated.</returns>
        public async Task UpdateOperationAsync(JObject item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            await Context.UpdateOperationAsync(this, item, cancellationToken).ConfigureAwait(false);
            Handled = true;
        }

        /// <summary>
        /// Cancels the table operation and discards the local instance of the item.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>Task that completes when operation is cancelled.</returns>
        public async Task CancelAndDiscardItemAsync(CancellationToken cancellationToken = default)
        {
            await Context.CancelAndDiscardItemAsync(this, cancellationToken);
            Handled = true;
        }

        /// <summary>
        /// Serialize this operations error to JSON.
        /// </summary>
        /// <returns>The <see cref="JObject"/> for this item.</returns>
        internal JObject Serialize() => new()
        {
            { SystemProperties.JsonIdProperty, Id },
            { "status", Status.HasValue ? (int?)Status.Value : null },
            { "version", OperationVersion },
            { "kind", (int)OperationKind },
            { "tableName", TableName },
            { "item", Item?.ToString(Formatting.None) },
            { "rawResult", RawResult }
        };

        /// <summary>
        /// Deserialize an operation error from the offline store.
        /// </summary>
        /// <param name="obj">The <see cref="JObject"/> version of the oepration error received from the offline store.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <returns>The deserialized <see cref="TableOperationError"/>.</returns>
        internal static TableOperationError Deserialize(JObject obj, DatasyncSerializerSettings serializerSettings)
        {
            // Get the relevant values from the JObject.
            string id = obj.Value<string>(SystemProperties.JsonIdProperty);
            HttpStatusCode? status = obj.ContainsKey("status") ? (HttpStatusCode?)obj.Value<int?>("status") : null;
            long version = obj.Value<long?>("version").GetValueOrDefault();
            TableOperationKind operationKind = (TableOperationKind)obj.Value<int>("kind");
            string tableName = obj.Value<string>("tableName");

            string itemStr = obj.Value<string>("item");
            JObject item = string.IsNullOrWhiteSpace(itemStr) ? null : JObject.Parse(itemStr);
            string rawResult = obj.Value<string>("rawResult");
            JObject result = null;
            if (rawResult != null)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<JObject>(rawResult, serializerSettings);
                }
                catch (JsonException)
                {
                    // Ignore JsonException, because 'rawResult' might not be JSON.
                }
            }

            return new TableOperationError(id, version, operationKind, status, tableName, item, rawResult, result);
        }

        /// <summary>
        /// Indicates if this <see cref="TableOperationError"/> is identical to the other one.
        /// </summary>
        /// <param name="other">The other <see cref="TableOperationError"/>.</param>
        /// <returns><c>true</c> if the <paramref name="other"/> is identical to this one.</returns>
        public bool Equals(TableOperationError other)
            => other != null && other.Id == Id && other.OperationVersion == OperationVersion && other.OperationKind == OperationKind && other.TableName == TableName && other.RawResult == RawResult;

        /// <summary>
        /// Indicates if this <see cref="TableOperationError"/> is identical to the other one.
        /// </summary>
        /// <param name="obj">The other <see cref="TableOperationError"/>.</param>
        /// <returns><c>true</c> if the <paramref name="obj"/> is identical to this one.</returns>
        public override bool Equals(object obj)
            => Equals(obj as TableOperationError);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode() => Id.GetHashCode();
    }
}