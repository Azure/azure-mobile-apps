// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading;

namespace Microsoft.Datasync.Client.SQLiteStore.Utils
{
    /// <summary>
    /// A lock mechanism that releases when disposed.
    /// </summary>
    internal sealed class DisposableLock : IDisposable
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
        public IDisposable AcquireLock()
        {
            semaphore.Wait();
            return new DisposeAction(() => semaphore.Release());
        }

        public void Dispose()
        {
            semaphore.Dispose();
        }
    }

    /// <summary>
    /// An <see cref="IDisposable"/> that runs an action on disposing.
    /// </summary>
    /// <remarks>
    /// This is most often used to release an asynchronous lock when disposed.
    /// </remarks>
    internal struct DisposeAction : IDisposable
    {
        bool isDisposed;
        private readonly Action action;

        public DisposeAction(Action action)
        {
            isDisposed = false;
            this.action = action;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            action();
        }
    }
}
