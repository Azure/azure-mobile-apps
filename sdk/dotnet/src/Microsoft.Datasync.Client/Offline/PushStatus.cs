// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The state in which push completed.
    /// </summary>
    public enum PushStatus
    {
        /// <summary>
        /// All operations were completed, possibly with errors.
        /// </summary>
        Complete = 0,

        /// <summary>
        /// The push was aborted due to a network error.
        /// </summary>
        CancelledByNetworkError,

        /// <summary>
        /// The push was aborted due to an authentication error.
        /// </summary>
        CancelledByAuthenticationError,

        /// <summary>
        /// The push was aborted due to an error from the offline store.
        /// </summary>
        CancelledByOfflineStoreError,

        /// <summary>
        /// The push was aborted because of a <see cref="System.Threading.CancellationToken"/>
        /// </summary>
        CancelledByToken,

        /// <summary>
        /// The push was aborted by a <see cref="Queue.TableOperation"/>.
        /// </summary>
        CancelledByOperation,

        /// <summary>
        /// The push failed due to an internal error.
        /// </summary>
        InternalError = Int32.MaxValue
    }
}
