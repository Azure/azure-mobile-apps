// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Definition of the base operations you can perform on a datasync table.
    /// </summary>
    /// <remarks>
    /// The ID property within the DTO must either be marked with the <c>Key</c>
    /// attribute or must be named "Id".</remarks>
    /// <typeparam name="T">The type of the data stored in the datasync table</typeparam>
    internal class DatasyncTable<T> : IDatasyncTable<T>
    {
        /// <summary>
        /// Creates a new <see cref="DatasyncTable{T}"/> at the provided endpoint.
        /// </summary>
        /// <param name="relativeUri">The relative URI to the endpoint.</param>
        /// <param name="client">The <see cref="InternalHttpClient"/> to use for communication.</param>
        /// <param name="options">The client options for adjusting the request and response.</param>
        public DatasyncTable(string relativeUri, InternalHttpClient client, DatasyncClientOptions options)
        {
            Validate.IsRelativeUri(relativeUri, nameof(relativeUri));
            Validate.IsNotNull(client, nameof(client));
            Validate.IsNotNull(options, nameof(options));

            Endpoint = new Uri(client.Endpoint, relativeUri.TrimStart('/'));
            HttpClient = client;
            ClientOptions = options;
        }

        /// <summary>
        /// The <see cref="InternalHttpClient"/> to use for communication.
        /// </summary>
        internal InternalHttpClient HttpClient { get; }

        /// <summary>
        /// The base <see cref="Uri"/> for the table.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// The <see cref="DatasyncClientOptions"/> for the table.
        /// </summary>
        public DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// Creates a new item within the table.
        /// </summary>
        /// <param name="item">The item to add to the table.</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public Task<ServiceResponse<T>> CreateItemAsync(T item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an existing item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse"/> object.</returns>
        public Task<ServiceResponse> DeleteItemAsync(string id, IfMatch precondition = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query string to send to the service</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        public AsyncPageable<U> GetAsyncItems<U>(string query = "", CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public Task<ServiceResponse<T>> GetItemAsync(string id, IfNoneMatch precondition = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replace the item with a new copy of the item.  Note that the item must have an Id (either called Id,
        /// or decorated with the <see cref="KeyAttribute"/> attribute) that is a string and set.
        /// </summary>
        /// <param name="item">the replacement item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public Task<ServiceResponse<T>> ReplaceItemAsync(T item, IfMatch precondition = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public Task<ServiceResponse<T>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, IfMatch precondition = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the current table to a table with a new type, but the same endpoint, client, and options.
        /// </summary>
        /// <typeparam name="U">The new type of the supported items</typeparam>
        /// <returns>The new table</returns>
        public IDatasyncTable<U> WithType<U>() where U : notnull
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Request the total count of items that are available with the query
        /// (without paging)
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> IncludeTotalCount(bool enabled = true)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Request that deleted items are returned.
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> IncludeDeletedItems(bool enabled = true)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Apply the specified selection to the source query
        /// </summary>
        /// <typeparam name="U">The type of the projection</typeparam>
        /// <param name="selector">The selector function</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Apply the specified skip clause to the source query.
        /// </summary>
        /// <remarks>
        /// Skip clauses are cumulative.
        /// </remarks>
        /// <param name="count">The number of items to skip</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> Skip(int count)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Apply the specified take clause to the source query.
        /// </summary>
        /// <remarks>
        /// The minimum take clause is the one that is used.
        /// </remarks>
        /// <param name="count">The number of items to take</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> Take(int count)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        public AsyncPageable<T> ToAsyncPageable(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Applies the specified filter to the source query.
        /// </summary>
        /// <remarks>
        /// Consecutive Where clauses are 'AND' together.
        /// </remarks>
        /// <param name="predicate">The predicate to use as the filter</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Add the provided parameter to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="key">The key of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> WithParameter(string key, string value)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Add the provided parameters to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="parameters">A dictionary of parameters</param>
        /// <returns>The composed query</returns>
        //public ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
