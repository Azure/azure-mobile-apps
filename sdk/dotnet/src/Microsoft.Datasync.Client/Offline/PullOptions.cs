// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The options used to configure a pull operation.
    /// </summary>
    public class PullOptions
    {
        /// <summary>
        /// If <c>true</c>, other tables will be pushed prior to pulling this table
        /// if this table is dirty.  If <c>false</c>, only this table will be pushed
        /// if this table is dirty.
        /// </summary>
        public bool PushOtherTables { get; set; } = true;

        /// <summary>
        /// If set, this is used as a query ID.  The query ID is a key to store
        /// a delta-token.  If not set, the query ID will be generated from the
        /// table name and query string provided.
        /// </summary>
        public string QueryId { get; set; }
    }
}
