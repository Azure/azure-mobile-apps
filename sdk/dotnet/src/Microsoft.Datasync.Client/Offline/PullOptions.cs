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
        /// Internal testing - ensures that we always use a delta-token filter when
        /// pulling data.
        /// </summary>
        internal bool AlwaysPullWithDeltaToken { get; set; } = false;

        /// <summary>
        /// When a pull operation happens, the related push operation for this
        /// table is automatically done.  If you would like to push the entire
        /// operations queue, then set this to <c>true</c>.
        /// </summary>
        public bool PushOtherTables { get; set; } = false;

        /// <summary>
        /// When downloading a lot of records, it's often requested that the delta
        /// token is written only every so often instead of every record.  This is
        /// a choice between performance and consistency. If the delta-token is 
        /// skipped and then the app crashes, you'll have an inconsistent database
        /// and may have to work to get it back to a consistent state.
        /// </summary>
        /// <value>The number of records between writes of the delta-token during a pull</value>
        public int WriteDeltaTokenInterval { get; set; } = 1;

        /// <summary>
        /// If set, this is used as a query ID.  The query ID is a key to store
        /// a delta-token.  If not set, the query ID will be generated from the
        /// table name and query string provided.
        /// </summary>
        public string QueryId { get; set; }
    }
}
