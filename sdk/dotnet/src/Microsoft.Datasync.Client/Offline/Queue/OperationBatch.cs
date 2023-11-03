// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    /// <summary>
    /// A collection of operations being pushed to the remote service.
    /// </summary>
    internal class OperationBatch
    {
        /// <summary>
        /// Creates a new <see cref="OperationBatch"/> based on a context.
        /// </summary>
        /// <param name="context">The context.</param>
        public OperationBatch(SyncContext context)
        {
            Arguments.IsNotNull(context, nameof(context));
            Context = context;
        }

        /// <summary>
        /// The reason for an abort if the batch has been aborted.
        /// </summary>
        public PushStatus? AbortReason { get; private set; }

        /// <summary>
        /// The <see cref="SyncContext"/> pushing this batch.
        /// </summary>
        internal SyncContext Context { get; }

        /// <summary>
        /// The <see cref="IOfflineStore"/> being used for persistent storage.
        /// </summary>
        internal IOfflineStore OfflineStore { get => Context.OfflineStore; }

        /// <summary>
        /// The errors that occur while interacting with the store.
        /// </summary>
        internal List<Exception> OtherErrors { get; } = new();

        /// <summary>
        /// The JSON serializer settings.
        /// </summary>
        internal DatasyncSerializerSettings SerializerSettings { get => Context.ServiceClient.Serializer.SerializerSettings; }

        /// <summary>
        /// Changes the status of the batch to aborted with a specific reason.
        /// </summary>
        /// <param name="reason">The abort reason.</param>
        /// <exception cref="ArgumentException">If the abort reason doesn't indicate an abort.</exception>
        public void Abort(PushStatus reason)
        {
            if (reason == PushStatus.Complete)
            {
                throw new ArgumentException("Invalid reason (Complete is not aborted).", nameof(reason));
            }
            AbortReason = reason;
        }

        /// <summary>
        /// Adds an error to the list of synchronization errors.
        /// </summary>
        /// <param name="operation">The operation causing the error</param>
        /// <param name="status">The HTTP status code returned by server.</param>
        /// <param name="rawResult">Raw response of the table operation.</param>
        /// <param name="result">Response of the table operation.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task AddErrorAsync(TableOperation operation, HttpStatusCode? status, string rawResult, JObject result, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(operation, nameof(operation));
            var error = new TableOperationError(operation, Context, status, rawResult, result);
            await OfflineStore.UpsertAsync(SystemTables.SyncErrors, new[] { error.Serialize() }, false, cancellationToken);
        }

        /// <summary>
        /// Loads a single page from the SyncErrors table into memory.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="nextLink">The next link.</param>
        /// <returns>A page of operation error objects.</returns>
        private async Task<Page<TableOperationError>> GetNextPageAsync(string query, string nextLink, CancellationToken cancellationToken)
        {
            if (nextLink != null)
            {
                var requestUri = new Uri(nextLink);
                query = requestUri.Query.TrimStart('?');
            }
            Page<JObject> sourcePage = await Context.GetNextPageAsync(SystemTables.SyncErrors, query, cancellationToken).ConfigureAwait(false);
            return new Page<TableOperationError>()
            {
                Count = sourcePage.Count,
                NextLink = sourcePage.NextLink,
                Items = sourcePage.Items?.Select(err => TableOperationError.Deserialize(err, SerializerSettings))
            };
        }

        /// <summary>
        /// Checks if there are any unhandled sync errors or handler errors recorded for this batch.
        /// </summary>
        /// <param name="errors">List of all sync errors.</param>
        /// <returns><c>true</c> if there are any errors to be concerned with.</returns>
        public bool HasErrors(IEnumerable<TableOperationError> errors)
            => errors?.Any(e => !e.Handled) == true || OtherErrors.Count > 0;

        /// <summary>
        /// Loads all the sync errors in local store that are recorded for this batch.
        /// </summary>
        /// <param name="tableNames">The list of tables to load errors for.  If empty, all tables are loaded.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A list of sync errors.</returns>
        public async Task<IList<TableOperationError>> LoadErrorsAsync(string[] tableNames, CancellationToken cancellationToken = default)
        {
            var enumerable = new FuncAsyncPageable<TableOperationError>(nextLink => GetNextPageAsync("", nextLink, cancellationToken));
            var errors = new List<TableOperationError>();
            await foreach (var error in enumerable)
            {
                if (tableNames == null || tableNames.Length == 0 || tableNames.Contains(error.TableName))
                {
                    error.Context = Context;
                    errors.Add(error);
                }
            }
            return errors;
        }

        /// <summary>
        /// Loads all the sync errors in local store that are recorded for this batch.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A list of sync errors.</returns>
        public async Task<IList<TableOperationError>> LoadErrorsAsync(CancellationToken cancellationToken = default)
            => await LoadErrorsAsync(null, cancellationToken);
    }
}
