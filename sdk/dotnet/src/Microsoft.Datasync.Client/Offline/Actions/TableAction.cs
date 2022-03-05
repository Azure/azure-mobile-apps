using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Actions
{
    /// <summary>
    /// Base class for table-specific sync actions (Purge and Pull)
    /// </summary>
    internal abstract class TableAction : SyncAction
    {
        public TableAction(SyncContext context, RemoteTable table, string queryId, QueryDescription query, IEnumerable<string> relatedTables, CancellationToken cancellationToken)
            : base(context.OperationsQueue, context.OfflineStore, cancellationToken)
        {
            Table = table;
            QueryId = queryId;
            Query = query;
            RelatedTables = relatedTables;
            Context = context;
            SettingsManager = context.SyncSettingsManager;
        }

        /// <summary>
        /// The <see cref="SyncContext"/> handling the table operation.
        /// </summary>
        protected SyncContext Context { get; }

        /// <summary>
        /// The query to be executed against the table.
        /// </summary>
        protected QueryDescription Query { get; }

        /// <summary>
        /// The query ID for this query.
        /// </summary>
        protected string QueryId { get; }

        /// <summary>
        /// The list of related tables we may need to push.
        /// </summary>
        public IEnumerable<string> RelatedTables { get; set; }

        /// <summary>
        /// The settings manager that holds the delta tokens.
        /// </summary>
        protected SyncSettingsManager SettingsManager { get; }

        /// <summary>
        /// The reference to the remote table.
        /// </summary>
        protected RemoteTable Table { get; }

        /// <summary>
        /// The task to execute.
        /// </summary>
        /// <returns>A task that completes when the action is finished.</returns>
        public async override Task ExecuteAsync()
        {
            try
            {
                await WaitPendingActionAsync(CancellationToken).ConfigureAwait(false);
                using (await OperationsQueue.LockTableAsync(Table.TableName, CancellationToken).ConfigureAwait(false))
                {
                    long pendingOperations = await OperationsQueue.CountPendingOperationsAsync(Table.TableName, CancellationToken).ConfigureAwait(false);
                    if (pendingOperations > 0)
                    {
                        bool tableIsClean = await HandleDirtyTableAsync(CancellationToken).ConfigureAwait(false);
                        if (!tableIsClean)
                        {
                            // Table is dirty and we cannot proceed for execution.
                            return;
                        }
                    }
                    await ProcessTableAsync(CancellationToken).ConfigureAwait(false);
                    TaskSource.SetResult(0);
                }
            }
            catch (Exception ex)
            {
                TaskSource.TrySetException(ex);
                return;
            }
        }

        /// <summary>
        /// Wait for any pending actions to complete.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when there are no pending actions.</returns>
        protected virtual Task WaitPendingActionAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <summary>
        /// Handles a dirty table (i.e. one with pending operations waiting to be pushed).
        /// </summary>
        /// <returns>A task that returns <c>true</c> if there are no pending operations waiting.</returns>
        protected abstract Task<bool> HandleDirtyTableAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Processes the table action.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the processing is finished.</returns>
        protected abstract Task ProcessTableAsync(CancellationToken cancellationToken);
    }
}
