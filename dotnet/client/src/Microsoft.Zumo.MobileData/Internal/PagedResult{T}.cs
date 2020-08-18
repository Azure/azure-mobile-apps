// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Zumo.MobileData.Internal
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
        public T[] Results { get; set; }

        /// <summary>
        /// The Uri to the next page of items.
        /// </summary>
        public long? Count { get; set; }
    }
}
