// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Query.OData;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Actions
{
    /// <summary>
    /// A synchronization action for a Pull operation.
    /// </summary>
    internal class PullAction : TableAction
    {
        /// <summary>
        /// A task that completes when the related push finishes.
        /// </summary>
        private Task pendingAction;

        /// <summary>
        /// Create a new <see cref="PullAction"/>.
        /// </summary>
        /// <param name="context">The <see cref="SyncContext"/> that created this action.</param>
        /// <param name="table">a reference to the remote table to pull.</param>
        /// <param name="queryId">The query ID for the purposes of incremental sync.</param>
        /// <param name="query">The filter for the items to pull.</param>
        /// <param name="relatedTables">A list of related tables to push.</param>
        /// <param name="options">The pull options for this action.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        internal PullAction(SyncContext context, RemoteTable table, string queryId, QueryDescription query, IEnumerable<string> relatedTables, CancellationToken cancellationToken)
            : base(context, table, queryId, query, relatedTables, cancellationToken)
        {
        }

        /// <summary>
        /// Handles a dirty table (i.e. one with pending operations waiting to be pushed).
        /// </summary>
        /// <returns>A task that returns <c>true</c> if there are no pending operations waiting.</returns>
        protected override Task<bool> HandleDirtyTableAsync(CancellationToken cancellationToken)
        {
            pendingAction = Context.DeferTableActionAsync(this, cancellationToken);
            return Task.FromResult(false);
        }

        /// <summary>
        /// Processes the table action.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the processing is finished.</returns>
        protected override async Task ProcessTableAsync(CancellationToken cancellationToken)
        {
            // Modify the query description for the delta token
            var deltaToken = await SettingsManager.GetDeltaTokenAsync(Table.TableName, QueryId, cancellationToken).ConfigureAwait(false);
            var deltaTokenFilter = CompareDeltaToken(BinaryOperatorKind.GreaterThan, SystemProperties.JsonUpdatedAtProperty, deltaToken);
            Query.Filter = Query.Filter == null ? deltaTokenFilter : new BinaryOperatorNode(BinaryOperatorKind.And, Query.Filter, deltaTokenFilter);

            // Set the IncludeDeleted flag
            Dictionary<string, string> parameters = new() { { ODataOptions.IncludeDeleted, "true" } };

            // Generate the query.
            var odataString = Query.ToODataString(parameters);
            
            // Run the query
            await foreach (var item in Table.GetAsyncItems(odataString).WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessItemAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Wait for any pending actions to complete.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when there are no pending actions.</returns>
        protected override Task WaitPendingActionAsync(CancellationToken cancellationToken)
            => pendingAction ?? Task.CompletedTask;

        /// <summary>
        /// Creates a delta-token comparison node.
        /// </summary>
        /// <param name="kind">The kind of comparison.</param>
        /// <param name="propName">The property name.</param>
        /// <param name="value">The value of the propert.</param>
        /// <returns>A <see cref="QueryNode"/> for the comparison.</returns>
        private static BinaryOperatorNode CompareDeltaToken(BinaryOperatorKind kind, string propName, DateTimeOffset value)
        {
            string strValue = $"datetimeoffset'{value.ToUniversalTime().ToString(DatasyncIsoDateTimeConverter.IsoDateTimeFormat)}'";
            return new BinaryOperatorNode(kind, new MemberAccessNode(null, propName), new ConstantNode(strValue));
        }

        /// <summary>
        /// Processes a single item in the pull operation.
        /// </summary>
        /// <param name="instance">The item to process.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item has been processed.</returns>
        private async Task ProcessItemAsync(JToken instance, CancellationToken cancellationToken)
        {
            if (instance is not JObject item)
            {
                throw new InvalidOperationException("Item pulled from the remote table is not an object.");
            }

            string id = ServiceSerializer.GetId(item);
            if (id == null)
            {
                throw new InvalidOperationException("Received an item without an ID.");
            }

            var pendingOperation = await OperationsQueue.GetOperationByItemIdAsync(Table.TableName, id, cancellationToken).ConfigureAwait(false);
            if (pendingOperation != null)
            {
                throw new InvalidOperationException("Received an item for which there is a pending operation.");
            }
            DateTimeOffset? updatedAt = ServiceSerializer.GetUpdatedAt(item)?.ToUniversalTime();

            if (ServiceSerializer.IsDeleted(item))
            {
                await OfflineStore.DeleteAsync(Table.TableName, id, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await OfflineStore.UpsertAsync(Table.TableName, item, true, cancellationToken).ConfigureAwait(false);
            }
            
            if (updatedAt.HasValue)
            {
                await SettingsManager.SetDeltaTokenAsync(Table.TableName, QueryId, updatedAt.Value, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
