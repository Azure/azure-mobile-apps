// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Zumo.MobileData
{
    /// <summary>
    /// Information about the table, potentially with a query.
    /// </summary>
    public class MobileTableMetadata
    {
        /// <summary>
        /// The number of items in the table, or (if you supplied a query),
        /// the number of items that will be returned in you iterated through
        /// all items returned by the query provided.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// The maximum value of the $top query parameter.
        /// </summary>
        public long? MaxTop { get; set; }

        /// <summary>
        /// The number of items returned per-page with server-side paging.
        /// </summary>
        public long? PageSize { get; set; }
    }
}
