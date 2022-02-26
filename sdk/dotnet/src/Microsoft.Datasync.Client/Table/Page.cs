// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// The JSON constants for handling pages of items.
    /// </summary>
    internal static class Page
    {
        internal const string JsonCountProperty = "count";
        internal const string JsonItemsProperty = "items";
        internal const string JsonNextLinkProperty = "nextLink";
    }

    /// <summary>
    /// The model for the response from a query operation.
    /// </summary>
    public class Page<T> where T : notnull
    {
        /// <summary>
        /// The items in a page.
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// The number of items that would be returned by the query,
        /// if not for paging.
        /// </summary>
        public long? Count { get; set; }

        /// <summary>
        /// The Uri to the nexty page in the result set.
        /// </summary>
        public Uri NextLink { get; set; }
    }
}