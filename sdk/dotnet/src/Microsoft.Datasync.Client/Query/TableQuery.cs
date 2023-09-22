// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq;
using Microsoft.Datasync.Client.Query.OData;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Datasync.Client.Query
{
    /// <summary>
    /// Represents a query that can be evaluated against an OData table.
    /// </summary>
    /// <remarks>
    /// Rather than implenting <see cref="IQueryable{T}"/> directly, we've implemented the
    /// portion of the LINQ query pattern we support on a datasync service.  You can use
    /// the <see cref="ITableQuery{T}"/> instance to build up a query using normal LINQ
    /// patterns.
    /// </remarks>
    internal class TableQuery<T> : ITableQuery<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableQuery{T}"/> class.
        /// </summary>
        /// <param name="table">The table being queried.</param>
        internal TableQuery(IRemoteTable<T> table)
            : this(table, null, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableQuery{T}"/> class.
        /// </summary>
        /// <param name="table">The table being queried.</param>
        /// <param name="query"> The encapsulated <see cref="IQueryable{T}"/>.</param>
        /// <param name="parameters"> The optional user-defined query string parameters to include with the query.</param>
        /// <param name="requestTotalCount">If <c>true</c>, include the total count of items that will be returned with this query (without considering paging).</param>
        internal TableQuery(IRemoteTable<T> table, IQueryable<T> query, IDictionary<string, string> parameters, bool requestTotalCount)
        {
            Arguments.IsNotNull(table, nameof(table));

            Parameters = parameters ?? new Dictionary<string, string>();
            Query = query ?? Array.Empty<T>().AsQueryable();
            RemoteTable = table;
            RequestTotalCount = requestTotalCount;
        }

        /// <summary>
        /// Converts the current query into an OData query string for use by the
        /// other <c>To*</c> methods.
        /// </summary>
        /// <param name="includeParameters">If <c>true</c>, include the optional parameters.</param>
        /// <returns>The OData query string representing this query</returns>
        internal string ToODataString(bool includeParameters = true)
            => new QueryTranslator<T>(this).Translate().ToODataString(includeParameters ? Parameters : null);

        /// <summary>
        /// The associated offline table.
        /// </summary>
        internal bool IsOfflineEnabled { get; set; }

        #region ITableQuery<T>
        /// <summary>
        /// The user-defined query string parameters to include with the query when
        /// sent to the remote service.
        /// </summary>
        public IDictionary<string, string> Parameters { get; }

        /// <summary>
        /// The underlying <see cref="IQueryable{T}"/> associated with this query.
        /// </summary>
        public IQueryable<T> Query { get; set; }

        /// <summary>
        /// The <see cref="IRemoteTable{T}"/> being queried.
        /// </summary>
        public IRemoteTable<T> RemoteTable { get; }

        /// <summary>
        /// If <c>true</c>, include the total count of items that will be returned with this query
        /// (without considering paging).
        /// </summary>
        public bool RequestTotalCount { get; private set; }
        #endregion

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
                Parameters[ODataOptions.IncludeDeleted] = "true";
            }
            else
            {
                Parameters.Remove(ODataOptions.IncludeDeleted);
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
            RequestTotalCount = enabled;
            return this;
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Query = Query.OrderBy(keySelector);
            return this;
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Query = Query.OrderByDescending(keySelector);
            return this;
        }

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">Type representing the projected result of the query.</typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            IRemoteTable<U> remoteTable = new RemoteTable<U>(RemoteTable.TableName, RemoteTable.ServiceClient, isProjection: true);
            return new TableQuery<U>(remoteTable, Query.Select(selector), Parameters, RequestTotalCount) { IsOfflineEnabled = IsOfflineEnabled };
        }

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
            Query = Query.Skip(count);
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
            Query = Query.Take(count);
            return this;
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Query = ((IOrderedQueryable<T>)Query).ThenBy(keySelector);
            return this;
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            Query = ((IOrderedQueryable<T>)Query).ThenByDescending(keySelector);
            return this;
        }

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            Query = Query.Where(predicate);
            return this;
        }

        /// <summary>
        /// Adds the parameter to the list of user-defined parameters to send with the
        /// request.
        /// </summary>
        /// <param name="key">The parameter key</param>
        /// <param name="value">The parameter value</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> WithParameter(string key, string value)
        {
            Arguments.IsNotNullOrWhitespace(key, nameof(key));
            Arguments.IsNotNullOrWhitespace(value, nameof(value));
            if (key.StartsWith("$") || key.StartsWith("__"))
            {
                throw new ArgumentException($"Parameter '{key}' is invalid", nameof(key));
            }
            Parameters[key] = value;
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
            Arguments.IsNotNull(parameters, nameof(parameters));
            parameters.ToList().ForEach(param =>
            {
                if (param.Key.StartsWith("$") || param.Key.StartsWith("__"))
                {
                    throw new ArgumentException($"Parameter '{param.Key}' is invalid", nameof(parameters));
                }
                Parameters[param.Key] = param.Value;
            });
            return this;
        }

        /// <summary>
        /// Count the number of items that would be returned by the provided query, without returning all the values.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        public Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        {
            if (IsOfflineEnabled)
            {
                var offlineTable = new OfflineTable<T>(RemoteTable.TableName, RemoteTable.ServiceClient);
                return offlineTable.CountItemsAsync(this, cancellationToken);
            }
            return RemoteTable.CountItemsAsync(this, cancellationToken);
        }

        /// <summary>
        /// Returns the result of the query as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/></returns>
        public IAsyncEnumerable<T> ToAsyncEnumerable()
        {
            if (IsOfflineEnabled)
            {
                var offlineTable = new OfflineTable<T>(RemoteTable);
                return offlineTable.GetAsyncItems(this);
            }
            return RemoteTable.GetAsyncItems(this);
        }
        #endregion
    }
}
