// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Coordinates all the requests for offline operations.
    /// </summary>
    internal class SyncContext : IDisposable
    {
        private IOfflineStore _store;

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
