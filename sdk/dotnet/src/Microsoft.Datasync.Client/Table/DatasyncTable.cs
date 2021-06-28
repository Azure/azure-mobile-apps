// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Internal;
using Microsoft.Datasync.Client.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Provides access to a typed datasync service table.
    /// </summary>
    /// <typeparam name="T">The type of the entity within the table.</typeparam>
    public class DatasyncTable<T> : IDatasyncTable<T> where T : notnull
    {
        /// <summary>
        /// Creates a new <see cref="DatasyncTable{T}"/> using information from the <see cref="DatasyncClient"/>.
        /// </summary>
        /// <param name="endpoint">The base <see cref="Uri"/> for the table.</param>
        /// <param name="client">The <see cref="InternalHttpClient"/> to use for communication.</param>
        /// <param name="options">The <see cref="DatasyncClientOptions"/> to use in processing requests and responses.</param>
        internal DatasyncTable(Uri endpoint, InternalHttpClient client, DatasyncClientOptions options)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));
            Validate.IsNotNull(client, nameof(client));
            Validate.IsNotNull(options, nameof(options));

            Endpoint = new UriBuilder(endpoint).Normalized().Uri;
            HttpClient = client;
            ClientOptions = options;
        }

        /// <summary>
        /// The fully-qualified Uri where the datasync table service is located.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// The <see cref="HttpClient"/> used for communication.
        /// </summary>
        internal InternalHttpClient HttpClient { get; }

        /// <summary>
        /// The <see cref="DatasyncClientOptions"/> used for adjusting requests/responses.
        /// </summary>
        public DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// Creates a new item within the table.
        /// </summary>
        /// <param name="item">The item to add to the table.</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<HttpResponse<T>> CreateItemAsync(T item, CancellationToken token = default)
        {
            Validate.IsNotNull(item, nameof(item));

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint).WithJsonPayload(item, ClientOptions.SerializerOptions);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    return response;
                }
            }
            else if (message.IsConflictStatusCode())
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    throw new ConflictException<T>(response);
                }
            }
            else
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                throw new RequestFailedException(response);
            }
        }

        /// <summary>
        /// Deletes an existing item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse"/> object.</returns>
        public virtual async Task<HttpResponse> DeleteItemAsync(string id, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(HttpMethod.Delete, itemUri).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                return await HttpResponse.FromResponseAsync(message, token).ConfigureAwait(false);
            }
            else if (message.IsConflictStatusCode())
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    throw new ConflictException<T>(response);
                }
            }
            else
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                throw new RequestFailedException(response);
            }
        }

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query string to send to the service</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        public virtual AsyncPageable<U> GetAsyncItems<U>(string query = "", CancellationToken token = default) where U : notnull
            => new FuncAsyncPageable<U>(nextLink => GetPageOfItemsAsync<U>(query, nextLink, token));

        /// <summary>
        /// Get a single page of items produced as a result of a query against the server.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned.</typeparam>
        /// <param name="requestUri">The <see cref="Uri"/> to request</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="Response{T}"/> object with a <see cref="Page{T}"/> of items in it.</returns>
        /// <exception cref="RequestFailedException">If the request fails for any reason</exception>
        internal virtual async Task<HttpResponse<Page<U>>> GetPageOfItemsAsync<U>(string query = "", string? requestUri = null, CancellationToken token = default) where U : notnull
        {
            Uri uri = requestUri != null ? new Uri(requestUri) : new UriBuilder(Endpoint).WithQuery(query).Uri;
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                return  await HttpResponse.FromResponseAsync<Page<U>>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
            }
            else
            {
                var response = await HttpResponse.FromResponseAsync(message, token).ConfigureAwait(false);
                throw new RequestFailedException(response);
            }
        }

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<HttpResponse<T>> GetItemAsync(string id, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(HttpMethod.Get, itemUri).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    return response;
                }
            }
            else
            {
                var response = await HttpResponse.FromResponseAsync(message).ConfigureAwait(false);
                throw (response.StatusCode == 304)
                    ? new NotModifiedException(response)
                    : new RequestFailedException(response);
            }
        }

        /// <summary>
        /// Replace the item with a new copy of the item.  Note that the item must have an Id (either called Id,
        /// or decorated with the <see cref="KeyAttribute"/> attribute) that is a string and set.
        /// </summary>
        /// <param name="item">the replacement item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<HttpResponse<T>> ReplaceItemAsync(T item, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsNotNull(item, nameof(item));
            var id = Utils.GetIdFromItem(item);
            Validate.IsValidId(id, nameof(item));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(HttpMethod.Put, itemUri)
                .WithJsonPayload(item, ClientOptions.SerializerOptions).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    return response;
                }
            }
            else if (message.IsConflictStatusCode())
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    throw new ConflictException<T>(response);
                }
            }
            else
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                throw new RequestFailedException(response);
            }
        }

        /// <summary>
        ///Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<HttpResponse<T>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));
            Validate.IsNotNullOrEmpty(changes, nameof(changes));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), itemUri)
                .WithJsonPayload(changes, ClientOptions.SerializerOptions).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    return response;
                }
            }
            else if (message.IsConflictStatusCode())
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                if (!response.HasContent)
                {
                    throw new RequestFailedException(response);
                }
                else
                {
                    throw new ConflictException<T>(response);
                }
            }
            else
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                throw new RequestFailedException(response);
            }
        }

        /// <summary>
        /// Converts the current table to a table with a new type, but the same endpoint, client, and options.
        /// </summary>
        /// <typeparam name="U">The new type of the supported items</typeparam>
        /// <returns>The new table</returns>
        public IDatasyncTable<U> WithType<U>() where U : notnull
            => new DatasyncTable<U>(Endpoint, HttpClient, ClientOptions);

        /// <summary>
        /// Request the total count of items that are available with the query
        /// (without paging)
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> IncludeTotalCount(bool enabled = true)
            => new DatasyncTableQuery<T>(this).IncludeTotalCount(enabled);

        /// <summary>
        /// Request that deleted items are returned.
        /// </summary>
        /// <param name="enabled">Set the request to enabled or disabled</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> IncludeDeletedItems(bool enabled = true)
            => new DatasyncTableQuery<T>(this).IncludeDeletedItems(enabled);

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this).OrderBy(keySelector);

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this).OrderByDescending(keySelector);

        /// <summary>
        /// Apply the specified selection to the source query
        /// </summary>
        /// <typeparam name="U">The type of the projection</typeparam>
        /// <param name="selector">The selector function</param>
        /// <returns>The composed query</returns>
        public ITableQuery<U> Select<U>(Expression<Func<T, U>> selector) where U : notnull
            => new DatasyncTableQuery<T>(this).Select(selector);

        /// <summary>
        /// Apply the specified skip clause to the source query.
        /// </summary>
        /// <remarks>
        /// Skip clauses are cumulative.
        /// </remarks>
        /// <param name="count">The number of items to skip</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> Skip(int count)
            => new DatasyncTableQuery<T>(this).Skip(count);

        /// <summary>
        /// Apply the specified take clause to the source query.
        /// </summary>
        /// <remarks>
        /// The minimum take clause is the one that is used.
        /// </remarks>
        /// <param name="count">The number of items to take</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> Take(int count)
            => new DatasyncTableQuery<T>(this).Take(count);

        /// <summary>
        /// Apply the specified ascending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this).ThenBy(keySelector);

        /// <summary>
        /// Apply the specified descending order clause to the source query.
        /// </summary>
        /// <typeparam name="TKey">The type of the member being ordered by</typeparam>
        /// <param name="keySelector">The expression selecting the member to order by</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            => new DatasyncTableQuery<T>(this).ThenByDescending(keySelector);

        /// <summary>
        /// Execute the query, returning an <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> to iterate over the items</returns>
        public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken token = default)
            => new DatasyncTableQuery<T>(this).ToAsyncEnumerable(token);

        /// <summary>
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        public AsyncPageable<T> ToAsyncPageable(CancellationToken token = default)
            => new DatasyncTableQuery<T>(this).ToAsyncPageable(token);

        /// <summary>
        /// Execute the query, returning an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> to iterate over the items</returns>
        public IEnumerable<T> ToEnumerable(CancellationToken token = default)
            => new DatasyncTableQuery<T>(this).ToEnumerable(token);

        /// <summary>
        /// Execute the query, returning a <see cref="List{T}"/>
        /// </summary>
        /// <returns>A <see cref="List{T}"/> to iterate over the items</returns>
        public ValueTask<List<T>> ToListAsync(CancellationToken token = default)
            => new DatasyncTableQuery<T>(this).ToListAsync(token);

        /// <summary>
        /// Applies the specified filter to the source query.
        /// </summary>
        /// <remarks>
        /// Consecutive Where clauses are 'AND' together.
        /// </remarks>
        /// <param name="predicate">The predicate to use as the filter</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> Where(Expression<Func<T, bool>> predicate)
            => new DatasyncTableQuery<T>(this).Where(predicate);

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
            => new DatasyncTableQuery<T>(this).WithParameter(key, value);

        /// <summary>
        /// Add the provided parameters to the query
        /// </summary>
        /// <remarks>
        /// Parameters are cumulative.
        /// </remarks>
        /// <param name="parameters">A dictionary of parameters</param>
        /// <returns>The composed query</returns>
        public ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)
            => new DatasyncTableQuery<T>(this).WithParameters(parameters);
    }
}
