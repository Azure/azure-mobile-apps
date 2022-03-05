// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Operations
{
    /// <summary>
    /// A collection of operations to be pushed to the remote service.
    /// </summary>
    internal class OperationBatch
    {
        public OperationBatch(ISyncHandler syncHandler, IOfflineStore store, SyncContext context)
        {
            SyncHandler = syncHandler;
            OfflineStore = store;
            SyncContext = context;
        }

        /// <summary>
        /// The status that returns the reason for an abort.
        /// </summary>
        public PushStatus? AbortReason { get; private set; }

        /// <summary>
        /// The offline store used for persistent storage.
        /// </summary>
        public IOfflineStore OfflineStore { get; }

        /// <summary>
        /// The errors that occur while interacting with the store or handler.
        /// </summary>
        public List<Exception> OtherErrors { get; } = new();

        /// <summary>
        /// The synchronization context.
        /// </summary>
        public SyncContext SyncContext { get; }

        /// <summary>
        /// The instance of the sync handler used to execute batch operations.
        /// </summary>
        public ISyncHandler SyncHandler { get; }

        /// <summary>
        /// Changes the status of the batch to aborted with a specific reason.
        /// </summary>
        /// <param name="reason">The abort reason.</param>
        /// <exception cref="ArgumentException">If the abort reason doesn't indicate an abort.</exception>
        public void Abort(PushStatus reason)
        {
            if (reason == PushStatus.Complete)
            {
                throw new ArgumentException("Invalid push status (Complete is not aborted).", nameof(reason));
            }
            AbortReason = reason;
        }

        /// <summary>
        /// Adds a synchronization error to the local store for this batch.
        /// </summary>
        /// <param name="error">The error that occurred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public Task AddSyncErrorAsync(TableOperationError error, CancellationToken cancellationToken = default)
            => OfflineStore.UpsertAsync(OfflineSystemTables.SyncErrors, error.Serialize(), false, cancellationToken);

        /// <summary>
        /// Loads all the sync errors in local store that are recorded for this batch.
        /// </summary>
        /// <param name="serializerSettings">the serializer settings to use for reading the errors.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A list of sync errors.</returns>
        public async Task<IList<TableOperationError>> LoadSyncErrorsAsync(DatasyncSerializerSettings serializerSettings, CancellationToken cancellationToken = default)
        {
            var errors = new List<TableOperationError>();
            var enumerable = new FuncAsyncPageable<TableOperationError>(nextLink => GetNextPageAsync(OfflineSystemTables.SyncErrors, "", nextLink, serializerSettings, cancellationToken));
            await foreach (var error in enumerable)
            {
                error.Context = SyncContext;
                errors.Add(error);
            }
            return errors;
        }

        /// <summary>
        /// Loads a single page from the SyncErrors table into memory.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="query">The query to execute.</param>
        /// <param name="nextLink">The next link.</param>
        /// <returns>A page of operation error objects.</returns>
        private async Task<Page<TableOperationError>> GetNextPageAsync(string tableName, string query, string nextLink, DatasyncSerializerSettings serializerSettings, CancellationToken cancellationToken = default)
        {
            if (nextLink != null)
            {
                var requestUri = new Uri(nextLink);
                query = requestUri.Query.TrimStart('?');
            }
            Page<JToken> sourcePage = await SyncContext.GetPageAsync(tableName, query, cancellationToken).ConfigureAwait(false);
            return new Page<TableOperationError>()
            {
                Count = sourcePage.Count,
                NextLink = sourcePage.NextLink,
                Items = sourcePage.Items?.Select(err => TableOperationError.Deserialize((JObject)err, serializerSettings))
            };
        }

        /// <summary>
        /// Checks if there are any unhandled sync errors or handler errors recorded for this batch.
        /// </summary>
        /// <param name="syncErrors">List of all sync errors.</param>
        /// <returns><c>true</c> if there are any errors to be concerned with.</returns>
        public bool HasErrors(IEnumerable<TableOperationError> syncErrors)
            => syncErrors.Any(e => !e.Handled) || OtherErrors.Any();
    }
}
