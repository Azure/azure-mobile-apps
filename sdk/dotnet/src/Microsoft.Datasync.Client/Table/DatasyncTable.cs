// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Internal;
using System;
using System.Collections.Generic;
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
    }
}
