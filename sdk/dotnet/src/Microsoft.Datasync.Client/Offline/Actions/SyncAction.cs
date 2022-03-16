// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Actions
{
    /// <summary>
    /// Base class for all the synchronization tasks (pull, push, purge)
    /// </summary>
    internal abstract class SyncAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncAction"/> class.
        /// </summary>
        /// <param name="queue">The operations queue holding the pending operations.</param>
        /// <param name="store">The persistent store used for storing synchronized items.</param>
        public SyncAction(OperationsQueue queue, IOfflineStore store)
        {
            Arguments.IsNotNull(queue, nameof(queue));
            Arguments.IsNotNull(store, nameof(store));

            OperationsQueue = queue;
            OfflineStore = store;
        }

        /// <summary>
        /// The offline store that the synchronization action is affecting.
        /// </summary>
        protected IOfflineStore OfflineStore { get; }

        /// <summary>
        /// The operations queue that holds the operations to be pushed.
        /// </summary>
        protected OperationsQueue OperationsQueue { get; }

        /// <summary>
        /// The task to execute.
        /// </summary>
        /// <returns>A task that completes when the action is finished.</returns>
        internal abstract Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
