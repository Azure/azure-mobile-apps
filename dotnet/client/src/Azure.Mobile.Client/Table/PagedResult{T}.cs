using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Mobile.Client.Table
{
    /// <summary>
    /// Data transfer object for a list operation with server-side paging
    /// </summary>
    /// <typeparam name="T">The type of the entity</typeparam>
    internal class PagedResult<T> where T : TableData
    {
        /// <summary>
        /// A single page of items.
        /// </summary>
        public T[] Values { get; set; }

        /// <summary>
        /// The Uri to the next page of items.
        /// </summary>
        public string NextLink { get; set; }
    }
}
