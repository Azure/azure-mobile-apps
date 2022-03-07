// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Queue
{
    /// <summary>
    /// The operations queue - a queue of operations waiting to be sent to the
    /// remote service.
    /// </summary>
    internal class OperationsQueue : IDisposable
    {
        private readonly AsyncLock mutex = new();

        /// <summary>
        /// Creates a new <see cref="OperationsQueue"/>
        /// </summary>
        /// <param name="store">The offline store to use for persistent storage.</param>
        internal OperationsQueue(IOfflineStore store)
        {
            OfflineStore = store;
        }

        /// <summary>
        /// Set to <c>true</c> when the operations queue is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The offline store being used for persistent storage.
        /// </summary>
        public IOfflineStore OfflineStore { get; }

        /// <summary>
        /// Acquires the queue lock, which allows the serialization of the operations queue insertions.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>An <see cref="IDisposable"/> for releasing the lock.</returns>
        public Task<IDisposable> AcquireLockAsync(CancellationToken cancellationToken = default)
            => mutex.AcquireAsync(cancellationToken);

        /// <summary>
        /// Deletes an operation from the operations queue.
        /// </summary>
        /// <param name="operation">The operation to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is removed.</returns>
        public Task DeleteOperationAsync(TableOperation operation, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Places a new operation into the queue.
        /// </summary>
        /// <param name="operation">The operation to enqueue.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is enqueued.</returns>
        public async Task EnqueueOperationAsync(TableOperation operation, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves an existing operation (by table/ID) from the operations queue.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="id">The ID of the item.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the existing operation, or <c>null</c> if no operation exists when complete.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TableOperation> GetOperationByItemIdAsync(string tableName, string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the operations queue using data from the persistent store.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operations queue is initialized.</returns>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            using (await mutex.AcquireAsync(cancellationToken).ConfigureAwait(false))
            {
                if (IsInitialized)
                {
                    return;
                }

                // TODO: Initialization

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Updates an operation within the operations queue.
        /// </summary>
        /// <param name="operation">The operation to update.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is updated.</returns>
        public async Task UpdateOperationAsync(TableOperation operation, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
