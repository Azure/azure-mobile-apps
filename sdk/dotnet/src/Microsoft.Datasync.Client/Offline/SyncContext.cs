// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Operations;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Coordinates all the requests for offline operations.
    /// </summary>
    internal class SyncContext : IDisposable
    {
        private readonly DatasyncClient _client;
        private readonly SemaphoreSlim _initializationLock = new(1);
        private IOfflineStore _store;

        /// <summary>
        /// Creates a new <see cref="SyncContext"/>.
        /// </summary>
        /// <param name="client">The associated client.</param>
        public SyncContext(DatasyncClient client)
        {
            Arguments.IsNotNull(client, nameof(client));
            _client = client;
        }

        /// <summary>
        /// When <c>true</c>, the sync context has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// The local store for offline operations.
        /// </summary>
        public IOfflineStore OfflineStore
        {
            get => _store;
            set
            {
                if (_store != value)
                {
                    _store?.Dispose();
                    _store = value;
                }
            }
        }

        /// <summary>
        /// The synchronization handler.
        /// </summary>
        public ISyncHandler SyncHandler { get; private set; }

        /// <summary>
        /// Cancels the table operation, discarding the item.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> being processed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public Task CancelAndDiscardItemAsync(TableOperationError error, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancels an operation and updated the item with the version from the service.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> that is being processed.</param>
        /// <param name="item">The item to use for updating.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the update is finished.</returns>
        public Task CancelAndUpdateItemAsync(TableOperationError error, JObject item, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes an item by ID from the offline table.
        /// </summary>
        /// <param name="tableName">The name of the offline table.</param>
        /// <param name="id">The ID of the item to remove.</param>
        /// <param name="instance">The instance being deleted (for version validation).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the item is removed from the offline table.</returns>
        public async Task DeleteAsync(string tableName, string id, JObject instance, CancellationToken cancellationToken = default)
        {
            var operation = new DeleteOperation(tableName, id)
            {
                Table = _client.GetRemoteTable(tableName),
                Item = instance
            };
            await ExecuteOperationAsync(operation, instance, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Initialize the synchronization context for this offline store, using the default sync handler.
        /// </summary>
        /// <param name="store">The offline store to use.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the synchronization context is initialized.</returns>
        public Task InitializeAsync(IOfflineStore store, CancellationToken cancellationToken = default)
            => InitializeAsync(store, null, cancellationToken);

        /// <summary>
        /// Initialize the synchronization context for this offline store.
        /// </summary>
        /// <param name="store">The offline store to use.</param>
        /// <param name="handler">The synchronization handler.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the synchronization context is initialized.</returns>
        public async Task InitializeAsync(IOfflineStore store, ISyncHandler handler, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(store, nameof(store));

            try
            {
                await _initializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                if (IsInitialized)
                {
                    return;
                }

                SyncHandler = handler ?? new SyncHandler();
                OfflineStore = store;

                await OfflineStore.InitializeAsync(cancellationToken).ConfigureAwait(false);
                OperationsQueue = await OperationsQueue.LoadFromOfflineStoreAsync(OfflineStore, cancellationToken).ConfigureAwait(false);
                SyncSettingsManager = new SyncSettingsManager(OfflineStore);

                IsInitialized = true;
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        /// <summary>
        /// Updates the item within an operation for retry.
        /// </summary>
        /// <param name="error">The <see cref="TableOperationError"/> that is being processed.</param>
        /// <param name="item">The item to use for updating.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the update is finished.</returns>
        public Task UpdateOperationAsync(TableOperationError error, JObject item, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                OfflineStore?.Dispose();
            }
        }
        #endregion
    }
}
