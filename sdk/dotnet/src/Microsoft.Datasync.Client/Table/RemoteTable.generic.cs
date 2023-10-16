// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

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
        internal RemoteTable(string tableName, DatasyncClient serviceClient, bool isProjection = false) : base(tableName, serviceClient)
        {
            if (!isProjection)
            {
                // ResolveTableName has a side effect of initializing the contract in the contract resolver,
                // so call it here to ensure initialization.
                serviceClient.Serializer.ResolveTableName<T>();

                // Ensure that the Id field in T is a String.
                ServiceSerializer.EnsureIdIsString<T>();
            }
        }

        #region IRemoteTable<T>
        /// <summary>
        /// Creates a blank query for the current table.
        /// </summary>
        /// <returns>A query against the table.</returns>
        public ITableQuery<T> CreateQuery()
            => new TableQuery<T>(this);

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
        public async Task DeleteItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            JObject value = ServiceClient.Serializer.Serialize(instance) as JObject;
            await TransformHttpExceptionAsync(() => DeleteItemAsync(value, cancellationToken)).ConfigureAwait(false);
            ServiceClient.Serializer.SetIdToDefault(instance);
        }

        /// <summary>
        /// Returns all instances from the table as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<T> GetAsyncItems()
            => GetAsyncItems(CreateQuery());

        /// <summary>
        /// Executes a query against the remote table.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<U> GetAsyncItems<U>(string query)
            => new FuncAsyncPageable<U>(nextLink => GetNextPageAsync<U>(query, nextLink));

        /// <summary>
        /// Executes a query against the remote table.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned by the query.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The list of items as an <see cref="IAsyncEnumerable{T}"/>.</returns>
        public IAsyncEnumerable<U> GetAsyncItems<U>(ITableQuery<U> query)
            => GetAsyncItems<U>(((TableQuery<U>)query).ToODataString(true));

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public new Task<T> GetItemAsync(string id, CancellationToken cancellationToken = default)
            => GetItemAsync(id, false, cancellationToken);


        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="includeDeleted">If <c>true</c>, a soft-deleted item will be returned; if <c>false</c>, GONE is returned.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public new async Task<T> GetItemAsync(string id, bool includeDeleted, CancellationToken cancellationToken = default)
        {
            JToken value = await base.GetItemAsync(id, includeDeleted, cancellationToken).ConfigureAwait(false);
            return ServiceClient.Serializer.Deserialize<T>(value);
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
            JObject value = ServiceClient.Serializer.Serialize(instance) as JObject;
            value = ServiceSerializer.RemoveSystemProperties(value, out _);
            JToken insertedValue = await TransformHttpExceptionAsync(() => InsertItemAsync(value, cancellationToken)).ConfigureAwait(false);
            ServiceClient.Serializer.Deserialize(insertedValue, instance);
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
            string id = ServiceClient.Serializer.GetId(instance, allowDefault: true);
            if (id == null)
            {
                return; // refresh is not supposed to throw if your object does not have an ID.
            }
            Arguments.IsValidId(id, nameof(instance));  // If it's not null and invalid, throw.

            JToken refreshed = await base.GetItemAsync(id, cancellationToken).ConfigureAwait(false);
            ServiceClient.Serializer.Deserialize<T>(refreshed, instance);
        }

        /// <summary>
        /// Replaces the current instance with the provided instance in the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns when the operation is complete.</returns>
        public async Task ReplaceItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            JObject value = ServiceClient.Serializer.Serialize(instance) as JObject;
            JToken updatedValue = await TransformHttpExceptionAsync(() => ReplaceItemAsync(value, cancellationToken)).ConfigureAwait(false);
            ServiceClient.Serializer.Deserialize(updatedValue, instance);
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
        public async Task UndeleteItemAsync(T instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            JObject value = ServiceClient.Serializer.Serialize(instance) as JObject;
            JToken updatedValue = await TransformHttpExceptionAsync(() => UndeleteItemAsync(value, cancellationToken)).ConfigureAwait(false);
            ServiceClient.Serializer.Deserialize(updatedValue, instance);
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
        /// Gets a single page of items produced as a result of a query against the server.
        /// </summary>
        /// <param name="query">The query string to send with the first request to the service.</param>
        /// <param name="nextLink">The link to the next page of items (for subsequent requests).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a page of items when complete.</returns>
        internal async Task<Page<U>> GetNextPageAsync<U>(string query, string nextLink, CancellationToken cancellationToken = default)
        {
            Page<JToken> json = await base.GetNextPageAsync(query, nextLink, cancellationToken).ConfigureAwait(false);
            Page<U> result = new() { Count = json.Count, NextLink = json.NextLink };
            result.Items = json.Items?.Select(item => ServiceClient.Serializer.Deserialize<U>(item));
            return result;
        }

        /// <summary>
        /// Executes a request and transfoms a conflict exception.
        /// </summary>
        /// <param name="action">The asynchronous request to execute.</param>
        /// <returns>The result of the execution.</returns>
        /// <exception cref="DatasyncConflictException{T}">if the response indicates a conflict.</exception>
        private async Task<JToken> TransformHttpExceptionAsync(Func<Task<JToken>> action)
        {
            try
            {
                return await action();
            }
            catch (DatasyncInvalidOperationException ex) when (ex.IsConflictStatusCode())
            {
                try
                {
                    T item = ServiceClient.Serializer.Deserialize<T>(ex.Value);
                    ex = new DatasyncConflictException<T>(ex, item);
                }
                catch
                {
                    // Deliberately empty to fall-through to throwing the original exception.
                }
                // We alter the ex in the try/catch above, so RCS1044 is a false-positive.
#pragma warning disable RCS1044 // Remove original exception from throw statement.
                throw ex;
#pragma warning restore RCS1044 // Remove original exception from throw statement.
            }
        }
    }
}
