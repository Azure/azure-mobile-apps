// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Query
{
    /// <summary>
    /// An interface describing the LINQ methods that a remote
    /// Datasync service supports.
    /// </summary>
    /// <typeparam name="T">The type of the Data Transfer Object (DTO) or model that is being used for the query.</typeparam>
    public interface ILinqMethods<T>
    {
        /// <summary>
        /// Ensure the query will get the deleted records.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this request.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> IncludeDeletedItems(bool enabled = true);

        /// <summary>
        /// Ensure the query will get the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or server.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this requst.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> IncludeTotalCount(bool enabled = true);

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">Type representing the projected result of the query.</typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<U> Select<U>(Expression<Func<T, U>> selector);

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">The number to skip.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> Skip(int count);

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">The number to take.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> Take(int count);

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds the parameter to the list of user-defined parameters to send with the
        /// request.
        /// </summary>
        /// <param name="key">The parameter key</param>
        /// <param name="value">The parameter value</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> WithParameter(string key, string value);

        /// <summary>
        /// Applies to the source query the specified string key-value
        /// pairs to be used as user-defined parameters with the request URI
        /// query string.
        /// </summary>
        /// <param name="parameters">The parameters to apply.</param>
        /// <returns>The composed query object.</returns>
        ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters);

        /// <summary>
        /// Count the number of items that would be returned by the provided query, without returning all the values.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        Task<long> LongCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the result of the query as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/></returns>
        IAsyncEnumerable<T> ToAsyncEnumerable();
    }
}
