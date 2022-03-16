// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Datasync.Client.Offline.Actions
{
	/// <summary>
	/// Executes a purge  operation on a table.
	/// </summary>
	internal class PurgeAction : TableAction
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PurgeAction"/> class.
		/// </summary>
		/// <param name="context">The synchronization context.</param>
		/// <param name="tableName">The name of the table holding the items to be purged.</param>
		/// <param name="query">The query identifying the items to be purged.</param>
		/// <param name="queryId">The query identifier for the delta token.</param>
		/// <param name="discardPendingOperations">if set to <c>true</c>, discard pending operations for this table.</param>
		internal PurgeAction(SyncContext context, string tableName, string query, string queryId, bool discardPendingOperations)
			: base(context, tableName, query, queryId)
		{
			DiscardPendingOperations = discardPendingOperations;
		}

		/// <summary>
		/// If <c>true</c>, disacard any pending operations so that the purge can happen.
		/// </summary>
		private bool DiscardPendingOperations { get; }

		/// <summary>
		/// Handles a dirty table (i.e. one with pending operations waiting to be pushed).
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
		/// <returns>A task that returns <c>true</c> if the table is dirty and cannot be purged, <c>false</c> otherwise.</returns>
		protected override async Task HandleDirtyTableAsync(CancellationToken cancellationToken)
		{
			if (Query.Filter != null || !DiscardPendingOperations)
			{
				throw new InvalidOperationException("The table cannot be purged because it has pending operations.");
			}

			await OperationsQueue.DeleteOperationsForTableasync(Query, cancellationToken).ConfigureAwait(false);

			var pendingOperations = await OperationsQueue.CountPendingOperationsAsync(TableName, cancellationToken).ConfigureAwait(false);
			if (pendingOperations > 0)
			{
				throw new InvalidOperationException("The table cannot be purged because not all operations were removed for the table - run PushAsync() or purge all items.");
			}
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
				await DeltaTokenStore.ResetDeltaTokenAsync(TableName, QueryId, cancellationToken).ConfigureAwait(false);
			}
			await OfflineStore.DeleteAsync(Query, cancellationToken).ConfigureAwait(false);
		}

	}
}
