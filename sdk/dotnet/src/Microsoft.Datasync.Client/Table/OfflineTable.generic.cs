// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Provides the operations that can be done against an offline table 
    /// with strongly type models.
    /// </summary>
    internal class OfflineTable<T> : OfflineTable, IOfflineTable<T>
    {
        /// <summary>
        /// Creates a new <see cref="OfflineTable{T}"/> instance to perform
        /// typed requests to an offline table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="serviceClient">The service client that created this table.</param>
        public OfflineTable(string tableName, DatasyncClient serviceClient) : base(tableName, serviceClient)
        {
            RemoteTable = ServiceClient.GetRemoteTable<T>(tableName);
        }

        /// <summary>
        /// The remote table associated with this table.
        /// </summary>
        internal IRemoteTable<T> RemoteTable { get; }

        #region IOfflineTable<T>
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
        /// Executes a query against the offline table.
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
        public new Task<T> GetItemAsync(string id, CancellationToken cancellationToken)
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
        /// Pulls the items matching the provided query from the remote table.
        /// </summary>
        /// <typeparam name="U">The type of the data transfer object (DTO) or model that is returned by the query.</typeparam>
        /// <param name="query">The OData query that determines which items to pull from the remote table.</param>
        /// <param name="options">The options used to configure the pull operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pull operation has finished.</returns>
        public Task PullItemsAsync<U>(ITableQuery<U> query, PullOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all the items in the offline table that match the query.
        /// </summary>
        /// <typeparam name="U">The type of the data transfer object (DTO) or model that is returned by the query.</typeparam>
        /// <param name="query">An OData query that determines which items to delete.</param>
        /// <param name="options">The options used to configure the purge operation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the purge operation has finished.</returns>
        public Task PurgeItemsAsync<U>(ITableQuery<U> query, PurgeOptions options, CancellationToken cancellationToken = default)
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
        #endregion

        #region ILinqMethods<T>
        public ITableQuery<T> IncludeDeletedItems(bool enabled = true)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> IncludeTotalCount(bool enabled = true)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> Skip(int count)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> Take(int count)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<T> ToAsyncEnumerable()
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> WithParameter(string key, string value)
        {
            throw new NotImplementedException();
        }

        public ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
