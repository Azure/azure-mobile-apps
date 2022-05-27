// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// A lock that allows multiple readers and only one writer.
    /// </summary>
    internal class AsyncReaderWriterLock
    {
        private readonly Task<DisposeAction> readerReleaser, writerReleaser;
        private readonly Queue<TaskCompletionSource<DisposeAction>> waitingWriters = new();
        private TaskCompletionSource<DisposeAction> waitingReader = new();
        private int readersWaiting;

        /// <summary>
        /// If <c>-1</c>, then a write lock is in place; if greater than 0, then the number of read locks.
        /// </summary>
        private int lockStatus; // -1 means write lock, >=0 no. of read locks

        public AsyncReaderWriterLock()
        {
            readerReleaser = Task.FromResult(new DisposeAction(ReaderRelease));
            writerReleaser = Task.FromResult(new DisposeAction(WriterRelease));
        }

        /// <summary>
        /// Lock for reading.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable lock.</returns>
        public Task<DisposeAction> ReaderLockAsync(CancellationToken cancellationToken = default)
        {
            lock (waitingWriters)
            {
                bool hasPendingReaders = lockStatus >= 0;
                bool hasNoPendingWritiers = waitingWriters.Count == 0;
                if (hasPendingReaders && hasNoPendingWritiers)
                {
                    ++lockStatus;
                    return readerReleaser;
                }
                else
                {
                    ++readersWaiting;
                    return waitingReader.Task.ContinueWith(t => t.Result, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Lock for writing.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable lock.</returns>
        public Task<DisposeAction> WriterLockAsync(CancellationToken cancellationToken = default)
        {
            lock (waitingWriters)
            {
                bool hasNoPendingReaders = lockStatus == 0;
                if (hasNoPendingReaders)
                {
                    lockStatus = -1;
                    return writerReleaser;
                }
                else
                {
                    var waiter = new TaskCompletionSource<DisposeAction>();
                    waitingWriters.Enqueue(waiter);
                    return waiter.Task.ContinueWith(t => t.Result, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Release a reader lock.
        /// </summary>
        private void ReaderRelease()
        {
            TaskCompletionSource<DisposeAction> toWake = null;

            lock (waitingWriters)
            {
                --lockStatus;
                if (lockStatus == 0 && waitingWriters.Count > 0)
                {
                    lockStatus = -1;
                    toWake = waitingWriters.Dequeue();
                }
            }
            toWake?.SetResult(new DisposeAction(WriterRelease));
        }

        /// <summary>
        /// Release a writer lock.
        /// </summary>
        private void WriterRelease()
        {
            TaskCompletionSource<DisposeAction> toWake = null;
            Action wakeupAction = ReaderRelease;

            lock (waitingWriters)
            {
                if (waitingWriters.Count > 0)
                {
                    toWake = waitingWriters.Dequeue();
                    wakeupAction = WriterRelease;
                }
                else if (readersWaiting > 0)
                {
                    toWake = waitingReader;
                    lockStatus = readersWaiting;
                    readersWaiting = 0;
                    waitingReader = new TaskCompletionSource<DisposeAction>();
                }
                else
                {
                    lockStatus = 0;
                }
            }
            toWake?.SetResult(new DisposeAction(wakeupAction));
        }
    }
}
