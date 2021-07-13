// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A simplified LINQ-like query language for the Datasync service.  Since the datasync
    /// service does not support the extensive capabilities of LINQ, we only support the
    /// subset that OData v4 supports.
    /// </summary>
    /// <typeparam name="T">The type of entity being queried</typeparam>
    public interface ITableQuery<T> where T : notnull
    {
        /// <summary>
        /// Request that deleted items are returned.
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> IncludeDeletedItems(bool enabled = true);

        /// <summary>
        /// Request the total count of items that are available with the query
        /// (without paging)
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> IncludeTotalCount(bool enabled = true);

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Apply the specified selection to the source query
        /// </summary>
        /// <typeparam name="U">The type of the projection</typeparam>
        /// <param name="selector">The selector function</param>
        /// <returns>The composed query</returns>
        ITableQuery<U> Select<U>(Expression<Func<T, U>> selector) where U : notnull;

        /// <summary>
        /// Apply the specified skip clause to the source query.
        /// </summary>
        /// <remarks>
        /// Skip clauses are cumulative.
        /// </remarks>
        /// <param name="count">The number of items to skip</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> Skip(int count);

        /// <summary>
        /// Apply the specified take clause to the source query.
        /// </summary>
        /// <remarks>
        /// The minimum take clause is the one that is used.
        /// </remarks>
        /// <param name="count">The number of items to take</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> Take(int count);

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        AsyncPageable<T> ToAsyncPageable(CancellationToken token = default);

        /// <summary>
        /// Applies the specified filter to the source query.
        /// </summary>
        /// <remarks>
        /// Consecutive Where clauses are 'AND' together.
        /// </remarks>
        /// <param name="predicate">The predicate to use as the filter</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add the provided parameter to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> WithParameter(string key, string value);

        /// <summary>
        /// Add the provided parameters to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="parameters">A dictionary of parameters</param>
        /// <returns>The composed query</returns>
        ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters);
    }
}
