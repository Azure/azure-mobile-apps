// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.SQLiteStore.Utils
{
    /// <summary>
    /// An asynchronous lock mechanism that releases when disposed.
    /// </summary>
    internal sealed class AsyncLock : IDisposable
    {
        /// <summary>
        /// The underlying lock.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new(initialCount: 1, maxCount: 1);

        /// <summary>
        /// Acquire the lock.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A task that returns an <see cref="IDisposable"/> to release the lock when the lock is acquired.</returns>
        public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            return new DisposeAction(() => semaphore.Release());
        }

        public void Dispose()
        {
            semaphore.Dispose();
        }
    }
}
