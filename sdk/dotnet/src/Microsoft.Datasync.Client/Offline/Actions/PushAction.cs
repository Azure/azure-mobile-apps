// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Exceptions;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Offline.Operations;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Actions
{
    /// <summary>
    /// A synchronization action responsible for pushing pending operations to the service.
    /// </summary>
    internal class PushAction : SyncAction
    {
        public PushAction(SyncContext context,  IEnumerable<string> tableNames, CancellationToken cancellationToken)
            : base(context.OperationsQueue, context.OfflineStore, cancellationToken)
        {
            SyncContext = context;
            TableNames = tableNames;
        }

        /// <summary>
        /// The associated service client for sending requests tot he service.
        /// </summary>
        private DatasyncClient ServiceClient { get => SyncContext.ServiceClient; }

        /// <summary>
        /// The synchronization context in use.
        /// </summary>
        private SyncContext SyncContext { get; }

        /// <summary>
        /// The synchronization handler for handling completed operations.
        /// </summary>
        private ISyncHandler SyncHandler { get => SyncContext.SyncHandler; }

        /// <summary>
        /// Limit the pushed operations to these tables.
        /// </summary>
        private IEnumerable<string> TableNames { get; }

        /// <summary>
        /// Executes the push operation.
        /// </summary>
        /// <returns>A task that completes when the action is finished.</returns>
        public override async Task ExecuteAsync()
        {
            var batch = new OperationBatch(SyncHandler, OfflineStore, SyncContext);
            List<TableOperationError> syncErrors = new();
            PushStatus batchStatus = PushStatus.InternalError;

            try
            {
                batchStatus = await ExecuteBatchAsync(batch, syncErrors, CancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(ex);
            }

            await FinalizePushAsync(batch, batchStatus, syncErrors, CancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a prepared batch of operations.
        /// </summary>
        /// <param name="batch">The batch to execute.</param>
        /// <param name="syncErrors">The accumulator for the synchronization errors.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the push status when complete.</returns>
        private async Task<PushStatus> ExecuteBatchAsync(OperationBatch batch, List<TableOperationError> syncErrors, CancellationToken cancellationToken)
        {
            // Register for cancellation
            CancellationToken.Register(() => batch.Abort(PushStatus.CancelledByToken));

            try
            {
                await ExecuteAllOperationsAsync(batch, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(ex);
            }

            PushStatus batchStatus = batch.AbortReason.GetValueOrDefault(PushStatus.Complete);
            try
            {
                syncErrors.AddRange(await batch.LoadSyncErrorsAsync(ServiceClient.Serializer.SerializerSettings, cancellationToken).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(new LocalStoreException("Failed to read errors from the local store.", ex));
            }
            return batchStatus;
        }

        /// <summary>
        /// Do any clean-up and finalize the push result.
        /// </summary>
        /// <param name="batch">The batch being processed.</param>
        /// <param name="batchStatus">The status of the batch.</param>
        /// <param name="syncErrors">Any sync errors during the push</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the push batch is finalized.</returns>
        private async Task FinalizePushAsync(OperationBatch batch, PushStatus batchStatus, IEnumerable<TableOperationError> syncErrors, CancellationToken cancellationToken)
        {
            var result = new PushCompletionResult(syncErrors, batchStatus);
            try
            {
                await batch.SyncHandler.OnPushCompleteAsync(result, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                batch.OtherErrors.Add(ex);
            }

            if (batchStatus != PushStatus.Complete || batch.HasErrors(syncErrors))
            {
                List<TableOperationError> unhandledErrors = syncErrors.Where(e => !e.Handled).ToList();
                Exception inner = batch.OtherErrors.Any() ? new AggregateException(batch.OtherErrors) : null;
                result = new PushCompletionResult(unhandledErrors, batchStatus);
                TaskSource.TrySetException(new PushFailedException(result, inner));
            }
            else
            {
                TaskSource.SetResult(0);
            }
        }

        /// <summary>
        /// Executes all the operations in the batch.
        /// </summary>
        /// <param name="batch">The batch to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the push is complete.</returns>
        private async Task ExecuteAllOperationsAsync(OperationBatch batch, CancellationToken cancellationToken)
        {
            TableOperation operation = await OperationsQueue.PeekAsync(0, TableNames, cancellationToken).ConfigureAwait(false);
            while (operation != null)
            {
                using (await OperationsQueue.LockItemAsync(operation.ItemId, cancellationToken).ConfigureAwait(false))
                {
                    bool success = await ExecuteOperationAsync(operation, batch, cancellationToken).ConfigureAwait(false);
                    if (batch.AbortReason.HasValue)
                    {
                        break;
                    }

                    if (success)
                    {
                        await OperationsQueue.DeleteAsync(operation.Id, operation.Version, cancellationToken).ConfigureAwait(false);
                    }

                    operation = await OperationsQueue.PeekAsync(operation.Sequence, TableNames, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Executes a single operation within the queue.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="batch">The operation batch being executed.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> ExecuteOperationAsync(TableOperation operation, OperationBatch batch, CancellationToken cancellationToken)
        {
            if (operation.IsCancelled || CancellationToken.IsCancellationRequested)
            {
                return false;
            }

            operation.Table = ServiceClient.GetRemoteTable(operation.TableName) as RemoteTable;
            await LoadOperationItemAsync(operation, batch, cancellationToken).ConfigureAwait(false);
            if (operation.Item == null || CancellationToken.IsCancellationRequested)
            {
                return false;
            }

            await TryUpdateOperationStateAsync(operation, TableOperationState.Attempted, batch, cancellationToken).ConfigureAwait(false);
            operation.Item = ServiceSerializer.RemoveSystemProperties(operation.Item, out string version);
            if (version != null)
            {
                operation.Item[SystemProperties.JsonVersionProperty] = version;
            }

            JObject result = null;
            Exception error = null;
            try
            {
                result = await batch.SyncHandler.ExecuteTableOperationAsync(operation, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                await TryUpdateOperationStateAsync(operation, TableOperationState.Failed, batch, cancellationToken).ConfigureAwait(false);
                if (TryAbortBatch(batch, error))
                {
                    return false; // There is no error to save in syn cerrors and no result to capture.
                }
            }

            if (error == null && result.Value<string>(SystemProperties.JsonIdProperty) != null && operation.CanWriteResultToStore)
            {
                await TryStoreOperationAsync(() => OfflineStore.UpsertAsync(operation.TableName, result, true, cancellationToken), batch, "Failed to update the item in the local store.");
            }
            else if (error != null)
            {
                HttpStatusCode? statusCode = null;
                string rawResult = null;
                if (error is DatasyncInvalidOperationException iox && iox.Response != null)
                {
                    statusCode = iox.Response.StatusCode;

                    try
                    {
                        rawResult = await iox.Response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                        result = ServiceSerializer.ValidItemOrNull(ServiceClient.Serializer.ParseToJToken(rawResult));
                    }
                    catch
                    {
                        // Deliberately ignore JSON parsing errors.
                    }
                }

                var syncError = new TableOperationError(operation, SyncContext, statusCode, rawResult, result);
                await batch.AddSyncErrorAsync(syncError, cancellationToken).ConfigureAwait(false);
            }

            return error == null;
        }

        /// <summary>
        /// Loads the item referenced by ID in the operation from the persistent store.
        /// </summary>
        /// <param name="operation">The operation being executed.</param>
        /// <param name="batch">The batch being processed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item is loaded into the operation.</returns>
        private async Task LoadOperationItemAsync(TableOperation operation, OperationBatch batch, CancellationToken cancellationToken)
        {
            if (operation.Item != null)
            {
                return;
            }

            await TryStoreOperationAsync(async () =>
            {
                operation.Item = await OfflineStore.GetItemAsync(operation.TableName, operation.ItemId, cancellationToken).ConfigureAwait(false);
            }, batch, "Failed to read the item from the local store.");

            if (operation.Item == null)
            {
                var item = new JObject(new JProperty(SystemProperties.JsonIdProperty, operation.ItemId));
                var syncError = new TableOperationError(operation, SyncContext, null, null, item);
                await batch.AddSyncErrorAsync(syncError, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Aborts the batch by setting the push status, returning true if there is
        /// a matching reason for the exception.
        /// </summary>
        /// <param name="batch">The batch being processed.</param>
        /// <param name="ex">The exception that was generated.</param>
        /// <returns>true if the exception indicates we should abort the batch.</returns>
        private static bool TryAbortBatch(OperationBatch batch, Exception ex)
        {
            if (ex is HttpRequestException || ex is TimeoutException)
            {
                batch.Abort(PushStatus.CancelledByNetworkError);
            }
            else if (ex is DatasyncInvalidOperationException iox && iox.Response?.StatusCode == HttpStatusCode.Unauthorized)
            {
                batch.Abort(PushStatus.CancelledByAuthenticationError);
            }
            else if (ex is PushAbortedException)
            {
                batch.Abort(PushStatus.CancelledByOperation);
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Trys to update the operation status in the persistent store.
        /// </summary>
        /// <param name="operation">The operation to update.</param>
        /// <param name="state">The new state of the operation.</param>
        /// <param name="batch">The batch being executed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the state is updated.</returns>
        private async Task TryUpdateOperationStateAsync(TableOperation operation, TableOperationState state, OperationBatch batch, CancellationToken cancellationToken)
        {
            operation.State = state;
            await TryStoreOperationAsync(() => OperationsQueue.UpdateAsync(operation, cancellationToken), batch, "Failed to update operation in the local store.").ConfigureAwait(false);
        }

        /// <summary>
        /// Trys to execute a store operation against the local store.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="batch">The batch being executed.</param>
        /// <param name="errorMessage">An error message, for if the update fails.</param>
        /// <returns>A task that completes when the store operation is completed.</returns>
        private static async Task TryStoreOperationAsync(Func<Task> action, OperationBatch batch, string errorMessage)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                batch.Abort(PushStatus.CancelledByOfflineStoreError);
                throw new LocalStoreException(errorMessage, ex);
            }
        }
    }
}
