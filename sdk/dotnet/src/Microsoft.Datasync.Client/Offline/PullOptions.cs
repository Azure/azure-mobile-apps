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
        private int maxpagesize = 50;

        /// <summary>
        /// The maximum number of items to pull in a single page. Setting this
        /// too large will result in large memory usage and the potential for
        /// long delays and large retries.
        /// </summary>
        public int MaxPageSize
        {
            get => maxpagesize;
            set => maxpagesize = Arguments.IsPositiveInteger(value, nameof(value));
        }

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
