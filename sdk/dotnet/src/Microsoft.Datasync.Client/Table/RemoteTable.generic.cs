// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Provides the operations that can be done against a remote table 
    /// with strongly type models.
    /// </summary>
    internal class RemoteTable<T> : RemoteTable, IRemoteTable<T>
    {
        /// <summary>
        /// Creates a new <see cref="RemoteTable{T}"/> instance to perform
        /// typed requests to a remote table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="serviceClient">The service client that created this table.</param>
        internal RemoteTable(string tableName, DatasyncClient serviceClient) : base(tableName, serviceClient)
        {
            throw new NotImplementedException();
        }

        #region IRemoteTable<T>
        /// <summary>
        /// Creates a blank query for the current table.
        /// </summary>
        /// <returns>A query against the table.</returns>
        public ITableQuery<T> CreateQuery()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation complete.</returns>
        public Task DeleteItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all instances from the table as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<T> GetAsyncItems()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a query against the remote table.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<U> GetAsyncItems<U>(string query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a query against the remote table.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<U> GetAsyncItems<U>(ITableQuery<U> query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public new Task<T> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts the instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        public Task InsertItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Refreshes the current instance with the latest values from the table.
        /// </summary>
        /// <param name="instance">The instance to refresh.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        public Task RefreshItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replaces the current instance with the provided instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        public Task ReplaceItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Undeletes an item in the remote table.
        /// </summary>
        /// <remarks>
        /// This requires that the table supports soft-delete.
        /// </remarks>
        /// <param name="instance">The instance to undelete in the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation complete.</returns>
        public Task UndeleteItemsync(T instance, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ILinqMethods<T>
        /// <summary>
        /// Ensure the query will get the deleted records.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this request.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> IncludeDeletedItems(bool enabled = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ensure the query will get the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or server.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this requst.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> IncludeTotalCount(bool enabled = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">Type representing the projected result of the query.</typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">The number to skip.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Skip(int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">The number to take.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Take(int count)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the result of the query as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/></returns>
        public IAsyncEnumerable<T> ToAsyncEnumerable()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
        #endregion
    }
}
