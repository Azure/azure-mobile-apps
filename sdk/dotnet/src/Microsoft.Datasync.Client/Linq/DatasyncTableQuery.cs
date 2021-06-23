// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Internal;
using Microsoft.Datasync.Client.Linq.Query;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Linq
{
    /// <summary>
    /// The concrete implementation of the <see cref="ITableQuery{T}"/> interface that provides search
    /// capabilities for OData services.
    /// </summary>
    /// <typeparam name="T">The type of the item being queried</typeparam>
    internal class DatasyncTableQuery<T> : ITableQuery<T> where T : notnull
    {
        private const string IncludeCountQueryParameter = "$count";
        private const string IncludeDeletedQueryParameter = "__includedeleted";

        /// <summary>
        /// Creates a new <see cref="DatasyncTableQuery{T}"/> that is based on a specific table.
        /// </summary>
        /// <param name="table"></param>
        internal DatasyncTableQuery(IDatasyncTable<T> table)
        {
            Validate.IsNotNull(table, nameof(table));
            Table = table;
            Query = new T[0].AsQueryable();
        }

        /// <summary>
        /// Generates a new query from an existing query, but with a new queryable.
        /// </summary>
        /// <param name="source">The source query</param>
        /// <param name="queryable">The replacement queryable</param>
        private DatasyncTableQuery(DatasyncTableQuery<T> source, IQueryable<T> queryable)
        {
            Query = queryable;
            QueryParameters = source.QueryParameters;
            SkipCount = source.SkipCount;
            TakeCount = source.TakeCount;
            Table = source.Table;
        }

        /// <summary>
        /// Generates a new query by specifying all the pieces needed
        /// </summary>
        private DatasyncTableQuery(IDatasyncTable<T> table, IQueryable<T> queryable, Dictionary<string, string> parameters, int skipCount, int takeCount)
        {
            Query = queryable;
            QueryParameters = parameters;
            SkipCount = skipCount;
            TakeCount = takeCount;
            Table = table;
        }

        /// <summary>
        /// The <see cref="IQueryable{T}"/> representing the LINQ query.
        /// </summary>
        internal IQueryable<T> Query { get; set; }

        /// <summary>
        /// The additional query parmeters to be sent.
        /// </summary>
        internal Dictionary<string, string> QueryParameters { get; } = new Dictionary<string, string>();

        /// <summary>
        /// The $skip component of the query
        /// </summary>
        internal int SkipCount { get; set; } = 0;

        /// <summary>
        /// The table being queried.
        /// </summary>
        internal IDatasyncTable<T> Table { get; }

        /// <summary>
        /// The $top component of the query
        /// </summary>
        internal int TakeCount { get; set; } = 0;

        /// <summary>
        /// Request that deleted items are returned.
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> IncludeDeletedItems(bool enabled = true)
        {
            if (enabled && !QueryParameters.ContainsKey(IncludeDeletedQueryParameter))
            {
                QueryParameters.Add(IncludeDeletedQueryParameter, "true");
            }
            else if (!enabled && QueryParameters.ContainsKey(IncludeDeletedQueryParameter))
            {
                QueryParameters.Remove(IncludeDeletedQueryParameter);
            }
            return this;
        }

        /// <summary>
        /// Request the total count of items that are available with the query
        /// (without paging)
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> IncludeTotalCount(bool enabled = true)
        {
            if (enabled && !QueryParameters.ContainsKey(IncludeCountQueryParameter))
            {
                QueryParameters.Add(IncludeCountQueryParameter, "true");
            }
            else if (!enabled && QueryParameters.ContainsKey(IncludeCountQueryParameter))
            {
                QueryParameters.Remove(IncludeCountQueryParameter);
            }
            return this;
        }

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this, Query.OrderBy(keySelector));

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this, Query.OrderByDescending(keySelector));

        /// <summary>
        /// Apply the specified selection to the source query
        /// </summary>
        /// <typeparam name="U">The type of the projection</typeparam>
        /// <param name="selector">The selector function</param>
        /// <returns>The composed query</returns>
        public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector) where U : notnull
        {
            if (Table is DatasyncTable<T> sourceTable)
            {
                var selectorTable = new DatasyncTable<U>(sourceTable.Endpoint, sourceTable.HttpClient, sourceTable.ClientOptions);
                return new DatasyncTableQuery<U>(selectorTable, Query.Select(selector), QueryParameters, SkipCount, TakeCount);
            }
            throw new InvalidOperationException("The table being used for the selection is not a datasync table");
        }

        /// <summary>
        /// Apply the specified skip clause to the source query.
        /// </summary>
        /// <remarks>
        /// Skip clauses are cumulative.
        /// </remarks>
        /// <param name="count">The number of items to skip</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> Skip(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(count)} must be positive", nameof(count));
            }
            SkipCount += count;
            return this;
        }

        /// <summary>
        /// Apply the specified take clause to the source query.
        /// </summary>
        /// <remarks>
        /// The minimum take clause is the one that is used.
        /// </remarks>
        /// <param name="count">The number of items to take</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> Take(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(count)} must be positive", nameof(count));
            }
            if (count < TakeCount)
            {
                TakeCount = count;
            }
            return this;
        }

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this, ((IOrderedQueryable<T>)Query).ThenBy(keySelector));

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this, ((IOrderedQueryable<T>)Query).ThenByDescending(keySelector));

        /// <summary>
        /// Execute the query, returning an <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> to iterate over the items</returns>
        public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken token = default)
            => Table.GetAsyncItems<T>(ToODataQueryString(), token);

        /// <summary>
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        public AsyncPageable<T> ToAsyncPageable(CancellationToken token = default)
            => Table.GetAsyncItems<T>(ToODataQueryString(), token);

        /// <summary>
        /// Execute the query, returning an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> to iterate over the items</returns>
        public IEnumerable<T> ToEnumerable(CancellationToken token = default)
            => Table.GetAsyncItems<T>(ToODataQueryString(), token).ToEnumerable();

        /// <summary>
        /// Execute the query, returning a <see cref="List{T}"/>
        /// </summary>
        /// <returns>A <see cref="List{T}"/> to iterate over the items</returns>
        public ValueTask<List<T>> ToListAsync(CancellationToken token = default)
            => Table.GetAsyncItems<T>(ToODataQueryString(), token).ToListAsync(token);

        /// <summary>
        /// Converts the current query into an OData query string for use by the
        /// other To* methods.
        /// </summary>
        /// <returns>The OData query string representing this query</returns>
        internal string ToODataQueryString()
            => new QueryTranslator<T>(this, Table.ClientOptions).Translate().ToODataString();

        /// <summary>
        /// Applies the specified filter to the source query.
        /// </summary>
        /// <remarks>
        /// Consecutive Where clauses are 'AND' together.
        /// </remarks>
        /// <param name="predicate">The predicate to use as the filter</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
            => new DatasyncTableQuery<T>(this, Query.Where(predicate));

        /// <summary>
        /// Add the provided parameter to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> WithParameter(string key, string value)
        {
            Validate.IsNotNullOrWhitespace(key, nameof(key));
            Validate.IsNotNullOrWhitespace(value, nameof(value));
            QueryParameters[key] = value;
            return this;
        }

        /// <summary>
        /// Add the provided parameters to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="parameters">A dictionary of parameters</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            Validate.IsNotNullOrEmpty(parameters, nameof(parameters));
            parameters.ToList().ForEach(param => QueryParameters[param.Key] = param.Value);
            return this;
        }
    }
}
