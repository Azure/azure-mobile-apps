// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Defines the kinds of operations performed against the local store.
    /// </summary>
    public enum LocalStoreOperationKind
    {
        /// <summary>
        /// Insert operation against the local store.
        /// </summary>
        Insert,
        /// <summary>
        /// Update operation against the local store.
        /// </summary>
        Update,
        /// <summary>
        /// Update or insert operation against the local store.
        /// </summary>
        Upsert,
        /// <summary>
        /// Delete operation against the local store.
        /// </summary>
        Delete,
    }
}
