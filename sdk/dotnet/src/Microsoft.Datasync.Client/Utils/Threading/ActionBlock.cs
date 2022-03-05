// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// A queue for executing asynchronous tasks in a FIFO fashion.
    /// </summary>
    internal class ActionBlock : IDisposable
    {
        readonly AsyncLock queueLock = new();

        /// <summary>
        /// Posts a message to the queue.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task PostAsync(Func<Task> action, CancellationToken cancellationToken = default)
        {
            using (await queueLock.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                await action();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                queueLock.Dispose();
            }
        }
    }
}
