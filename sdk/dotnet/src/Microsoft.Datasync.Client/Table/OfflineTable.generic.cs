// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Serializer = ServiceClient.Serializer;
        }

        /// <summary>
        /// Creates a new <see cref="OfflineTable{T}"/> instance to perform typed requests to an offline table.
        /// </summary>
        /// <param name="remoteTable">The associated remote table.</param>
        internal OfflineTable(IRemoteTable<T> remoteTable) : base(remoteTable.TableName, remoteTable.ServiceClient)
        {
            RemoteTable = remoteTable;
            Serializer = remoteTable.ServiceClient.Serializer;
        }

        /// <summary>
        /// The remote table associated with this table.
        /// </summary>
        internal IRemoteTable<T> RemoteTable { get; }

        /// <summary>
        /// The serializer to use when serializing to JSON.
        /// </summary>
        internal ServiceSerializer Serializer { get; }

        #region IOfflineTable<T>
        /// <summary>
        /// Creates a blank query for the current table.
        /// </summary>
        /// <returns>A query against the table.</returns>
        public ITableQuery<T> CreateQuery()
            => new TableQuery<T>(RemoteTable) { IsOfflineEnabled = true };

        /// <summary>
        /// Count the number of items that would be returned by the provided query, without returning
        /// all the values.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        public Task<long> CountItemsAsync(ITableQuery<T> query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));
            return CountItemsAsync(((TableQuery<T>)query).ToODataString(true), cancellationToken);
        }

        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation complete.</returns>
        public Task DeleteItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            var value = Serializer.Serialize(instance) as JObject;
            return DeleteItemAsync(value, cancellationToken);
        }

        /// <summary>
        /// Returns all instances from the table as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<T> GetAsyncItems()
            => GetAsyncItems(CreateQuery());

        /// <summary>
        /// Executes a query against the offline table.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<U> GetAsyncItems<U>(ITableQuery<U> query)
        {
            Arguments.IsNotNull(query, nameof(query));
            return new FuncAsyncPageable<U>(nextLink => GetNextPageAsync(query, nextLink));
        }

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public new async Task<T> GetItemAsync(string id, CancellationToken cancellationToken)
        {
            var value = await base.GetItemAsync(id, cancellationToken).ConfigureAwait(false);
            return value == null ? default : Serializer.Deserialize<T>(value);
        }

        /// <summary>
        /// Inserts the instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        public async Task InsertItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            var value = Serializer.Serialize(instance) as JObject;
            value = ServiceSerializer.RemoveSystemProperties(value, out _);
            JObject inserted = await InsertItemAsync(value, cancellationToken).ConfigureAwait(false);
            Serializer.Deserialize(inserted, instance);
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
            Arguments.IsNotNull(query, nameof(query));
            return PullItemsAsync(((TableQuery<T>)query).ToODataString(true), options, cancellationToken);
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
            Arguments.IsNotNull(query, nameof(query));
            return PurgeItemsAsync(((TableQuery<T>)query).ToODataString(true), options, cancellationToken);
        }

        /// <summary>
        /// Refreshes the current instance with the latest values from the table.
        /// </summary>
        /// <param name="instance">The instance to refresh.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        public async Task RefreshItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            string id = Serializer.GetId(instance, allowDefault: true);
            if (id == null)
            {
                return; // refresh is not supposed to throw if your object does not have an ID.
            }
            Arguments.IsValidId(id, nameof(instance)); // If it's an invalid ID, but not null then throw.

            JObject refreshed = await base.GetItemAsync(id, cancellationToken).ConfigureAwait(false);
            if (refreshed == null)
            {
                throw new InvalidOperationException("Item not found in offline store.");
            }
            Serializer.Deserialize<T>(refreshed, instance);
        }

        /// <summary>
        /// Replaces the current instance with the provided instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        public Task ReplaceItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            var value = Serializer.Serialize(instance) as JObject;
            return ReplaceItemAsync(value, cancellationToken);
        }
        #endregion

        #region ILinqMethods<T>
        /// <summary>
        /// Ensure the query will get the deleted records.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this request.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> IncludeDeletedItems(bool enabled = true)
            => CreateQuery().IncludeDeletedItems(enabled);

        /// <summary>
        /// Ensure the query will get the total count for all the records that would have been returned
        /// ignoring any take paging/limit clause specified by client or server.
        /// </summary>
        /// <param name="enabled">If <c>true</c>, enables this requst.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> IncludeTotalCount(bool enabled = true)
            => CreateQuery().IncludeTotalCount(enabled);

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => CreateQuery().OrderBy(keySelector);

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => CreateQuery().OrderByDescending(keySelector);

        /// <summary>
        /// Applies the specified selection to the source query.
        /// </summary>
        /// <typeparam name="U">Type representing the projected result of the query.</typeparam>
        /// <param name="selector">The selector function.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector)
            => CreateQuery().Select(selector);

        /// <summary>
        /// Applies the specified skip clause to the source query.
        /// </summary>
        /// <param name="count">The number to skip.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Skip(int count)
            => CreateQuery().Skip(count);

        /// <summary>
        /// Applies the specified take clause to the source query.
        /// </summary>
        /// <param name="count">The number to take.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Take(int count)
            => CreateQuery().Take(count);

        /// <summary>
        /// Applies the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => CreateQuery().ThenBy(keySelector);

        /// <summary>
        /// Applies the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by.</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => CreateQuery().ThenByDescending(keySelector);

        /// <summary>
        /// Applies the specified filter predicate to the source query.
        /// </summary>
        /// <param name="predicate">The filter predicate.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
            => CreateQuery().Where(predicate);

        /// <summary>
        /// Adds the parameter to the list of user-defined parameters to send with the
        /// request.
        /// </summary>
        /// <param name="key">The parameter key</param>
        /// <param name="value">The parameter value</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> WithParameter(string key, string value)
            => CreateQuery().WithParameter(key, value);

        /// <summary>
        /// Applies to the source query the specified string key-value
        /// pairs to be used as user-defined parameters with the request URI
        /// query string.
        /// </summary>
        /// <param name="parameters">The parameters to apply.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)
            => CreateQuery().WithParameters(parameters);

        /// <summary>
        /// Count the number of items that would be returned by the provided query, without returning all the values.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the number of items that will be in the result set when the query finishes.</returns>
        public Task<long> LongCountAsync(CancellationToken cancellationToken = default)
            => CountItemsAsync("", cancellationToken);

        /// <summary>
        /// Returns the result of the query as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/></returns>
        public IAsyncEnumerable<T> ToAsyncEnumerable()
            => CreateQuery().ToAsyncEnumerable();
        #endregion

        /// <summary>
        /// Gets the next page of items from the list.  If the <c>nextLink</c> is set, use that for
        /// the query; otherwise use the <c>query</c>
        /// </summary>
        /// <param name="query">The initial query.</param>
        /// <param name="nextLink">The next link.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        private async Task<Page<U>> GetNextPageAsync<U>(ITableQuery<U> query, string nextLink, CancellationToken cancellationToken = default)
        {
            var page = await base.GetNextPageAsync(((TableQuery<U>)query).ToODataString(true), nextLink, cancellationToken).ConfigureAwait(false);
            return new Page<U>
            {
                Count = page.Count,
                Items = page.Items.Select(m => Serializer.Deserialize<U>(m)).ToArray(),
                NextLink = page.NextLink
            };
        }
    }
}
