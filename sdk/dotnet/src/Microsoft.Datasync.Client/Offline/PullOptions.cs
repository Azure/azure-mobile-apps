// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The options used to configure a pull operation.
    /// </summary>
    public class PullOptions
    {
        /// <summary>
        /// When a pull operation happens, the related push operation for this
        /// table is automatically done.  If you would like to push the entire
        /// operations queue, then set this to <c>true</c>.
        /// </summary>
        public bool PushOtherTables { get; set; } = false;

        /// <summary>
        /// If set, this is used as a query ID.  The query ID is a key to store
        /// a delta-token.  If not set, the query ID will be generated from the
        /// table name and query string provided.
        /// </summary>
        public string QueryId { get; set; }
    }
}
