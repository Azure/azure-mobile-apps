// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A set of extension methods covering both <see cref="IDatasyncTable{T}"/> and
    /// <see cref="ITableQuery{T}"/>.
    /// </summary>
    public static class TableExtensions
    {
        /// <summary>
        /// Returns an <see cref="IAsyncEnumerable{T}"/> for the table.
        /// </summary>
        /// <typeparam name="T">The type of entity being returned</typeparam>
        /// <param name="table">The table to query</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> representing all the items in the table.</returns>
        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IDatasyncTable<T> table)
            => table.ToAsyncPageable();

        /// <summary>
        /// Returns an <see cref="IAsyncEnumerable{T}"/> for the query.
        /// </summary>
        /// <typeparam name="T">The type of entity being returned</typeparam>
        /// <param name="table">The query to execute</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> representing the items returned by executing the query.</returns>
        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ITableQuery<T> table)
            => table.ToAsyncPageable();
    }
}
