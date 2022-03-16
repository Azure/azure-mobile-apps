// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Actions
{
    /// <summary>
    /// Base class for table-specific actions (purge and pull)
    /// </summary>
    internal abstract class TableAction : SyncAction
    {
        internal TableAction(SyncContext context, string tableName, string query, string queryId) : base(context)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            Arguments.IsNotNull(query, nameof(query));
            Arguments.IsNotNull(queryId, nameof(queryId));

            TableName = tableName;
            Query = QueryDescription.Parse(tableName, query);
            QueryId = queryId;
        }

        /// <summary>
        /// The name of the table being actioned.
        /// </summary>
        protected string TableName { get; }

        /// <summary>
        /// The query to be executed against the table.
        /// </summary>
        protected QueryDescription Query { get; }

        /// <summary>
        /// The query ID for this query.
        /// </summary>
        protected string QueryId { get; }

        /// <summary>
        /// Executes the table action.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        internal async override Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            long pendingOperations = await OperationsQueue.CountPendingOperationsAsync(TableName, cancellationToken).ConfigureAwait(false);
            if (pendingOperations > 0)
            {
                await HandleDirtyTableAsync(cancellationToken).ConfigureAwait(false);
            }
            await ProcessTableAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles a dirty table (i.e. one with pending operations waiting to be pushed).
        /// </summary>
        /// <returns>A task that returns <c>true</c> if there are no pending operations waiting.</returns>
        protected abstract Task HandleDirtyTableAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Processes the table action.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the processing is finished.</returns>
        protected abstract Task ProcessTableAsync(CancellationToken cancellationToken);
    }
}
