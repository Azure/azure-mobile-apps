// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Linq
{
    /// <summary>
    /// An implementation of the <see cref="ITableQuery{T}"/>, but attached
    /// to the table instead of a standalone query object.
    /// </summary>
    public static class IDatasyncTableExtensions
    {
        /// <summary>
        /// Request the total count of items that are available with the query
        /// (without paging)
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> IncludeTotalCount<T>(this IDatasyncTable<T> table, bool enabled = true) where T : notnull
            => new DatasyncTableQuery<T>(table).IncludeTotalCount(enabled);

        /// <summary>
        /// Request that deleted items are returned.
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> IncludeDeletedItems<T>(this IDatasyncTable<T> table, bool enabled = true) where T : notnull
            => new DatasyncTableQuery<T>(table).IncludeDeletedItems(enabled);

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> OrderBy<T, TKey>(this IDatasyncTable<T> table, Expression<Func<T, TKey>> keySelector) where T : notnull
            => new DatasyncTableQuery<T>(table).OrderBy(keySelector);

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> OrderByDescending<T, TKey>(this IDatasyncTable<T> table, Expression<Func<T, TKey>> keySelector) where T : notnull
            => new DatasyncTableQuery<T>(table).OrderByDescending(keySelector);

        /// <summary>
        /// Apply the specified selection to the source query
        /// </summary>
        /// <typeparam name="U">The type of the projection</typeparam>
        /// <param name="selector">The selector function</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<U> Select<T, U>(this IDatasyncTable<T> table, Expression<Func<T, U>> selector) where T : notnull where U : notnull
            => new DatasyncTableQuery<T>(table).Select(selector);

        /// <summary>
        /// Apply the specified skip clause to the source query.
        /// </summary>
        /// <remarks>
        /// Skip clauses are cumulative.
        /// </remarks>
        /// <param name="count">The number of items to skip</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> Skip<T>(this IDatasyncTable<T> table, int count) where T : notnull
            => new DatasyncTableQuery<T>(table).Skip(count);

        /// <summary>
        /// Apply the specified take clause to the source query.
        /// </summary>
        /// <remarks>
        /// The minimum take clause is the one that is used.
        /// </remarks>
        /// <param name="count">The number of items to take</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> Take<T>(this IDatasyncTable<T> table, int count) where T : notnull
            => new DatasyncTableQuery<T>(table).Take(count);

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> ThenBy<T, TKey>(this IDatasyncTable<T> table, Expression<Func<T, TKey>> keySelector) where T : notnull
            => new DatasyncTableQuery<T>(table).ThenBy(keySelector);

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> ThenByDescending<T, TKey>(this IDatasyncTable<T> table, Expression<Func<T, TKey>> keySelector) where T : notnull
            => new DatasyncTableQuery<T>(table).ThenByDescending(keySelector);

        /// <summary>
        /// Execute the query, returning an <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> to iterate over the items</returns>
        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IDatasyncTable<T> table, CancellationToken token = default) where T : notnull
            => new DatasyncTableQuery<T>(table).ToAsyncEnumerable(token);

        /// <summary>
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        public static AsyncPageable<T> ToAsyncPageable<T>(this IDatasyncTable<T> table, CancellationToken token = default) where T : notnull
            => new DatasyncTableQuery<T>(table).ToAsyncPageable(token);

        /// <summary>
        /// Execute the query, returning an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> to iterate over the items</returns>
        public static IEnumerable<T> ToEnumerable<T>(this IDatasyncTable<T> table, CancellationToken token = default) where T : notnull
            => new DatasyncTableQuery<T>(table).ToEnumerable(token);

        /// <summary>
        /// Execute the query, returning a <see cref="List{T}"/>
        /// </summary>
        /// <returns>A <see cref="List{T}"/> to iterate over the items</returns>
        public static ValueTask<List<T>> ToListAsync<T>(this IDatasyncTable<T> table, CancellationToken token = default) where T : notnull
            => new DatasyncTableQuery<T>(table).ToListAsync(token);

        /// <summary>
        /// Applies the specified filter to the source query.
        /// </summary>
        /// <remarks>
        /// Consecutive Where clauses are 'AND' together.
        /// </remarks>
        /// <param name="predicate">The predicate to use as the filter</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> Where<T>(this IDatasyncTable<T> table, Expression<Func<T, bool>> predicate) where T : notnull
            => new DatasyncTableQuery<T>(table).Where(predicate);

        /// <summary>
        /// Add the provided parameter to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> WithParameter<T>(this IDatasyncTable<T> table, string key, string value) where T : notnull
            => new DatasyncTableQuery<T>(table).WithParameter(key, value);

        /// <summary>
        /// Add the provided parameters to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="parameters">A dictionary of parameters</param>
        /// <returns>The composed query</returns>
        public static ITableQuery<T> WithParameters<T>(this IDatasyncTable<T> table, IEnumerable<KeyValuePair<string, string>> parameters) where T : notnull
            => new DatasyncTableQuery<T>(table).WithParameters(parameters);
    }
}
