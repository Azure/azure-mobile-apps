// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Commands;
using Microsoft.Datasync.Client.Utils;
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
        /// Returns a <see cref="LazyObservableCollection{T}"/> for the table.
        /// </summary>
        /// <typeparam name="T">The type of entity being returned</typeparam>
        /// <param name="table">The table to query</param>
        /// <param name="pageCount">The number of items per page to return</param>
        /// <param name="errorHandler">An <see cref="IAsyncExceptionHandler"/> for reporting errors.</param>
        /// <returns>A <see cref="LazyObservableCollection{T}"/> representing all the items in the table.</returns>
        public static LazyObservableCollection<T> ToLazyObservableCollection<T>(this IDatasyncTable<T> table, int pageCount, IAsyncExceptionHandler errorHandler = null)
            => new InternalLazyObservableCollection<T>(table.ToAsyncEnumerable(), pageCount, errorHandler);

        /// <summary>
        /// Returns a <see cref="LazyObservableCollection{T}"/> for the table.
        /// </summary>
        /// <typeparam name="T">The type of entity being returned</typeparam>
        /// <param name="table">The table to query</param>
        /// <param name="errorHandler">An <see cref="IAsyncExceptionHandler"/> for reporting errors.</param>
        /// <returns>A <see cref="LazyObservableCollection{T}"/> representing all the items in the table.</returns>

        public static LazyObservableCollection<T> ToLazyObservableCollection<T>(this IDatasyncTable<T> table, IAsyncExceptionHandler errorHandler = null)
            => new InternalLazyObservableCollection<T>(table.ToAsyncEnumerable(), errorHandler);

        /// <summary>
        /// Returns an <see cref="IAsyncEnumerable{T}"/> for the query.
        /// </summary>
        /// <typeparam name="T">The type of entity being returned</typeparam>
        /// <param name="query">The query to execute</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> representing the items returned by executing the query.</returns>
        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ITableQuery<T> query)
            => query.ToAsyncPageable();

        /// <summary>
        /// Returns a <see cref="LazyObservableCollection{T}"/> for the query.
        /// </summary>
        /// <typeparam name="T">The type of entity being returned</typeparam>
        /// <param name="query">The query to execute</param>
        /// <param name="pageCount">The number of items per page to return</param>
        /// <param name="errorHandler">An <see cref="IAsyncExceptionHandler"/> for reporting errors.</param>
        /// <returns>A <see cref="LazyObservableCollection{T}"/> representing all the items in the table.</returns>
        public static LazyObservableCollection<T> ToLazyObservableCollection<T>(this ITableQuery<T> query, int pageCount, IAsyncExceptionHandler errorHandler = null)
            => new InternalLazyObservableCollection<T>(query.ToAsyncEnumerable(), pageCount, errorHandler);

        /// <summary>
        /// Returns a <see cref="LazyObservableCollection{T}"/> for the query.
        /// </summary>
        /// <typeparam name="T">The type of entity being returned</typeparam>
        /// <param name="query">The query to execute</param>
        /// <param name="errorHandler">An <see cref="IAsyncExceptionHandler"/> for reporting errors.</param>
        /// <returns>A <see cref="LazyObservableCollection{T}"/> representing all the items in the table.</returns>

        public static LazyObservableCollection<T> ToLazyObservableCollection<T>(this ITableQuery<T> query, IAsyncExceptionHandler errorHandler = null)
            => new InternalLazyObservableCollection<T>(query.ToAsyncEnumerable(), errorHandler);
    }
}
