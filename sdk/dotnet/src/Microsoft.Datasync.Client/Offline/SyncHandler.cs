// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Operations;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Handles table operation errors and push completion results.
    /// </summary>
    public class SyncHandler : ISyncHandler
    {
        /// <summary>
        /// Executes a single table operation against a remote table.
        /// </summary>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the server version of the item.</returns>
        public Task<JObject> ExecuteTableOperationAsync(TableOperation operation, CancellationToken cancellationToken = default)
            => operation.ExecuteAsync(cancellationToken);

        /// <summary>
        /// An event handler that is called when a push operation has completed.
        /// </summary>
        /// <param name="result">The result of the push operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the result has been handled.</returns>
        public Task OnPushCompleteAsync(PushCompletionResult result, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
