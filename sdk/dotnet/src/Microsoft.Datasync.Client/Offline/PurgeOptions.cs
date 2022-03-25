// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The options used to configure a purge operation.
    /// </summary>
    public class PurgeOptions
    {
        /// <summary>
        /// If <c>true</c>, discard the pending operations.  A purge will fail
        /// if this is set to <c>false</c> and there are pending operations to
        /// send to the remote table.
        /// </summary>
        public bool DiscardPendingOperations { get; set; }

        /// <summary>
        /// If set, this is used as a query ID.  The query ID is a key to store
        /// a delta-token.  If not set, the query ID will be generated from the
        /// table name and query string provided.  This resets the incremental
        /// sync state for the pull operation.
        /// </summary>
        public string QueryId { get; set; }
    }
}
