// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Zumo.Server.Utils
{
    /// <summary>
    /// A return type for handling paged responses.
    /// </summary>
    /// <typeparam name="T">The type of the entity to return</typeparam>
    public class PagedListResult<T> where T : class, ITableData
    {
        /// <summary>
        /// The list of entities in this result set.
        /// </summary>
        public IList<T> Values { get; set; }

        /// <summary>
        /// An opaque Uri for requesting the next page - null if no more pages.
        /// </summary>
        public Uri NextLink { get; set; }

        /// <summary>
        /// The count of items in the list without paging.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// The maximum value of the $top query parameter
        /// </summary>
        public long? MaxTop { get; set; }

        /// <summary>
        /// The size for server-side paging.
        /// </summary>
        public long? PageSize { get; set; }
    }
}
