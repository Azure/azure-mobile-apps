// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// A class for handling a dictionary of locks.
    /// </summary>
    internal class AsyncLockDictionary
    {
        /// <summary>
        /// A single entry in the dictionary of locks.
        /// </summary>
        private sealed class LockEntry : IDisposable
        {
            public int Count;
            public readonly AsyncLock Lock = new();

            public void Dispose()
            {
                Lock.Dispose();
            }
        }

        /// <summary>
        /// The dictionary of locks.
        /// </summary>
        private readonly Dictionary<string, LockEntry> locks = new();

        /// <summary>
        /// Acquire a lock (asynchronously) from the dictionary of locks.
        /// </summary>
        /// <param name="key">The key for the lock.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>An <see cref="IDisposable"/> to release the lock when done.</returns>
        public async Task<IDisposable> AcquireAsync(string key, CancellationToken cancellationToken)
        {
            LockEntry entry;

            lock (locks)
            {
                if (!locks.TryGetValue(key, out entry))
                {
                    locks[key] = entry = new LockEntry();
                }
                entry.Count++;
            }

            IDisposable releaser = await entry.Lock.AcquireAsync(cancellationToken).ConfigureAwait(false);

            return new DisposeAction(() =>
            {
                lock (locks)
                {
                    entry.Count--;
                    releaser.Dispose();
                    if (entry.Count == 0)
                    {
                        locks.Remove(key);
                        entry.Dispose();
                    }
                }
            });
        }
    }
}
