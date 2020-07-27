using System;
using System.Collections.Generic;

namespace Azure.Mobile.Server.Utils
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
        public IEnumerable<T> Values { get; set; }

        /// <summary>
        /// An opaque Uri for requesting the next page - null if no more pages.
        /// </summary>
        public Uri NextLink { get; set; }

        /// <summary>
        /// The count of items in the list without paging.
        /// </summary>
        public long? Count { get; set; }
    }
}
