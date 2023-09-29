// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.ComponentModel;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The policy that is used for updating the timestamp of the last synchronization.
    /// Use this to reset the synchronization of data from the server.
    /// </summary>
    public enum TimestampUpdatePolicy
    {
        /// <summary>
        /// Use the original mechanism (pre-policy).  This means that the timestamp is
        /// reset when there is a queryId, and not reset when there is no queryId.
        /// </summary>
        [Description("Default Policy: Timestamp is only reset when there is a queryId.")]
        Default,

        /// <summary>
        /// Do not update the timestamp.
        /// </summary>
        [Description("Do not update the timestamp.")]
        NoUpdate,

        /// <summary>
        /// Update the timestamp to the timestamp for the last entity in the table.
        /// </summary>
        [Description("Update the timestamp to the time the last entity was modified.")]
        UpdateToLastEntity,

        /// <summary>
        /// Update the timestamp to the current date/time.
        /// </summary>
        [Description("Update the timestamp to the current time.")]
        UpdateToNow,

        /// <summary>
        /// Update the timestamp to the beginning of time, which will cause all records to be re-synchronized.
        /// </summary>
        /// <remarks>
        /// Only select this option if you purged all items in the table.
        /// </remarks>
        [Description("Update the timestamp so that all records will be re-synchronized.")]
        UpdateToEpoch
    }

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
        /// How should the purge operation update the timestamp for a table?  Normally,
        /// we do not change the timestamp, but if you are purging all records, you 
        /// likely want to reset the timestamp to something else.
        /// </summary>
        public TimestampUpdatePolicy TimestampUpdatePolicy { get; set; } = TimestampUpdatePolicy.Default;

        /// <summary>
        /// If set, this is used as a query ID.  The query ID is a key to store
        /// a delta-token.  If not set, the query ID will be generated from the
        /// table name and query string provided.  This resets the incremental
        /// sync state for the pull operation.
        /// </summary>
        public string QueryId { get; set; }
    }
}
