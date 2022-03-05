// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Actions
{
    internal class PurgeAction : TableAction
    {
        /// <summary>
        /// Creates a new <see cref="PurgeAction"/>.
        /// </summary>
        /// <param name="context">The <see cref="SyncContext"/> that generated this action.</param>
        /// <param name="table">The table reference being used.</param>
        /// <param name="queryId">The queryID, used to reference a unique query.</param>
        /// <param name="query">The query to execute to find the items to purge.</param>
        /// <param name="force">If <c>true</c>, discard pending operations for the table in the queue.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe for the lifetime of the action.</param>
        public PurgeAction(SyncContext context, RemoteTable table, string queryId, QueryDescription query, bool force, CancellationToken cancellationToken)
            : base(context, table, queryId, query, null, cancellationToken)
        {
            Force = force;
        }

        /// <summary>
        /// If <c>true</c>, disacard any pending operations so that the purge can happen.
        /// </summary>
        private bool Force { get; }

        /// <summary>
        /// Handles a dirty table (i.e. one with pending operations waiting to be pushed).
        /// </summary>
        /// <returns>A task that returns <c>true</c> if there are no pending operations waiting.</returns>
        protected override async Task<bool> HandleDirtyTableAsync(CancellationToken cancellationToken)
        {
            if (Query.Filter != null || !Force)
            {
                throw new InvalidOperationException("The table cannot be purged because it has pending operations.");
            }

            var deleteOperationsQuery = new QueryDescription(OfflineSystemTables.OperationsQueue)
            {
                Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new MemberAccessNode(null, "tableName"), new ConstantNode(Table.TableName)),
                IncludeTotalCount = true,
                Top = 0
            };
            Page<JToken> deleteOperationsPage = await OfflineStore.GetPageAsync(deleteOperationsQuery, cancellationToken).ConfigureAwait(false);
            long itemsToRemove = deleteOperationsPage.Count ?? 0;

            // Delete the operations
            deleteOperationsQuery.Top = null;
            await OfflineStore.DeleteAsync(deleteOperationsQuery, cancellationToken).ConfigureAwait(false);

            // Delete the errors
            var deleteErrorsQuery = new QueryDescription(OfflineSystemTables.SyncErrors)
            {
                Filter = deleteOperationsQuery.Filter
            };
            await OfflineStore.DeleteAsync(deleteErrorsQuery, cancellationToken).ConfigureAwait(false);

            // Update the queue operations count
            OperationsQueue.UpdateOperationsCount(-itemsToRemove);

            return true;
        }

        /// <summary>
        /// Processes the table action.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the processing is finished.</returns>
        protected override async Task ProcessTableAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(QueryId))
            {
                await SettingsManager.ResetDeltaTokenAsync(Table.TableName, QueryId, cancellationToken).ConfigureAwait(false);
            }
            await OfflineStore.DeleteAsync(Query, cancellationToken).ConfigureAwait(false);
        }
    }
}
