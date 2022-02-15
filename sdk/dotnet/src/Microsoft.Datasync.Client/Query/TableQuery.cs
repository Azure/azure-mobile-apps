// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace Microsoft.Datasync.Client.Query
{
    /// <summary>
    /// The concrete implementation of the <see cref="ITableQuery{T}"/> interface that provides search
    /// capabilities for OData services.
    /// </summary>
    /// <typeparam name="T">The type of the item being queried</typeparam>
    internal class TableQuery<T> : ITableQuery<T>
    {
        private const string IncludeCountQueryParameter = "$count";
        private const string IncludeDeletedQueryParameter = "__includedeleted";

        /// <summary>
        /// Creates a new (empty) <see cref="TableQuery{T}"/> that will be used to
        /// query the provided table.
        /// </summary>
        /// <param name="table">The table associated with this query.</param>
        internal TableQuery(IRemoteTable<T> table)
        {
            Validate.IsNotNull(table, nameof(table));
            Table = table;
            Query = Array.Empty<T>().AsQueryable();
            QueryParameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Generates a new <see cref="TableQuery{T}"/> based on a source query, but
        /// with a new <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="queryable">The replacement <see cref="IQueryable{T}"/>.</param>
        private TableQuery(TableQuery<T> source, IQueryable<T> queryable)
        {
            Query = queryable;
            QueryParameters = source.QueryParameters;
            SkipCount = source.SkipCount;
            TakeCount = source.TakeCount;
            Table = source.Table;
        }

        /// <summary>
        /// Generates a new <see cref="TableQuery{T}"/> by specifying all the required parameters.
        /// </summary>
        /// <param name="table">The table associated with this query.</param>
        /// <param name="queryable">The underlying <see cref="IQueryable{T}"/> for this query.</param>
        /// <param name="parameters">Any user-defined parameters for this query.</param>
        /// <param name="skipCount">The current skip count.</param>
        /// <param name="takeCount">The current take count.</param>
        private TableQuery(IRemoteTable<T> table, IQueryable<T> queryable, IDictionary<string, string> parameters, int skipCount, int takeCount)
        {
            Query = queryable;
            QueryParameters = parameters ?? new Dictionary<string, string>();
            SkipCount = skipCount;
            TakeCount = takeCount;
            Table = table;
        }

        /// <summary>
        /// The <see cref="IQueryable{T}"/> representing the underlying LINQ query.
        /// </summary>
        internal IQueryable<T> Query { get; set; }

        /// <summary>
        /// The additional query parameters to be sent.
        /// </summary>
        internal IDictionary<string, string> QueryParameters { get; }

        /// <summary>
        /// The $skip component of the query.
        /// </summary>
        internal int SkipCount { get; set; } = 0;

        /// <summary>
        /// The table that is associated with this query.
        /// </summary>
        public IRemoteTable<T> Table { get; }

        /// <summary>
        /// The $top component of the query.
        /// </summary>
        internal int TakeCount { get; set; } = 0;

        #region ILinqMethods<T>
        /// <summary>
        /// Ensure the query will get the deleted records.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this request.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> IncludeDeletedItems(bool enabled = true)
        {
            if (enabled)
            {
                QueryParameters[IncludeDeletedQueryParameter] = "true";
            }
            else
            {
                QueryParameters.Remove(IncludeDeletedQueryParameter);
            }
            return this;
        }

        /// <summary>
        /// Ensure the query will get the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or server.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this requst.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> IncludeTotalCount(bool enabled = true)
        {
            if (enabled)
            {
                QueryParameters[IncludeCountQueryParameter] = "true";
            }
            else
            {
                QueryParameters.Remove(IncludeCountQueryParameter);
            }
            return this;
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => new TableQuery<T>(this, Query.OrderBy(keySelector));

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => new TableQuery<T>(this, Query.OrderByDescending(keySelector));

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">Type representing the projected result of the query.</typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector)
            => new TableQuery<U>(new RemoteTable<U>(Table as RemoteTable), Query.Select(selector), QueryParameters, SkipCount, TakeCount);

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">The number to skip.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Skip(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(count)} must be positive");
            }
            SkipCount += count;
            return this;
        }

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">The number to take.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Take(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(count)} must be positive");
            }
            TakeCount = (TakeCount == 0) ? count : Math.Min(count, TakeCount);
            return this;
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => new TableQuery<T>(this, ((IOrderedQueryable<T>)Query).ThenBy(keySelector));

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => new TableQuery<T>(this, ((IOrderedQueryable<T>)Query).ThenByDescending(keySelector));

        /// <summary>
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        public AsyncPageable<T> ToAsyncPageable(CancellationToken cancellationToken = default)
            => Table.GetAsyncItems<T>(ToQueryString(), cancellationToken);

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
            => new TableQuery<T>(this, Query.Where(predicate));

        /// <summary>
        /// Adds the parameter to the list of user-defined parameters to send with the
        /// request.
        /// </summary>
        /// <param name="key">The parameter key</param>
        /// <param name="value">The parameter value</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> WithParameter(string key, string value)
        {
            Validate.IsNotNullOrWhitespace(key, nameof(key));
            Validate.IsNotNullOrWhitespace(value, nameof(value));
            if (key.StartsWith("$") || key.StartsWith("__"))
            {
                throw new ArgumentException($"Parameter '{key}' is invalid", nameof(key));
            }
            QueryParameters[key] = value;
            return this;
        }
            

        /// <summary>
        /// Applies to the source query the specified string key-value
        /// pairs to be used as user-defined parameters with the request URI
        /// query string.
        /// </summary>
        /// <param name="parameters">The parameters to apply.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            Validate.IsNotNullOrEmpty(parameters, nameof(parameters));
            parameters.ToList().ForEach(param =>
            {
                if (param.Key.StartsWith("$") || param.Key.StartsWith("__"))
                {
                    throw new ArgumentException($"Parameter '{param.Key}' is invalid", nameof(parameters));
                }
                QueryParameters[param.Key] = param.Value;
            });
            return this;
        }
        #endregion

        /// <summary>
        /// Converts the current query into an OData query string for use by the
        /// other To* methods.
        /// </summary>
        /// <returns>The OData query string representing this query</returns>
        internal string ToQueryString()
            => new QueryTranslator<T>(this, Table.ClientOptions).Translate().ToODataString();
    }
}
