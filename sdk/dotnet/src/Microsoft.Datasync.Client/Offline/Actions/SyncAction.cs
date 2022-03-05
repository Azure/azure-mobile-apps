using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Actions
{
    /// <summary>
    /// The base class for all the synchronization actions (i.e. Pull, Push, and Purge)
    /// </summary>
    internal abstract class SyncAction
    {
        public SyncAction(OperationsQueue queue, IOfflineStore store, CancellationToken cancellationToken = default)
        {
            OperationsQueue = queue;
            OfflineStore = store;
            TaskSource = new();
            CancellationToken = cancellationToken;
            cancellationToken.Register(() => TaskSource.TrySetCanceled());
        }

        /// <summary>
        /// A cancellation token to observe.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// The task that you can await for completion.
        /// </summary>
        public Task CompletionTask => TaskSource.Task;

        /// <summary>
        /// The offline store that the synchronization action is affecting.
        /// </summary>
        protected IOfflineStore OfflineStore { get; }

        /// <summary>
        /// The operations queue that holds the operations to be pushed.
        /// </summary>
        protected OperationsQueue OperationsQueue { get; }

        /// <summary>
        /// The task source for the async task.
        /// </summary>
        protected TaskCompletionSource<object> TaskSource { get; }

        /// <summary>
        /// The task to execute.
        /// </summary>
        /// <returns>A task that completes when the action is finished.</returns>
        public abstract Task ExecuteAsync();
    }
}
