// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Reflection;
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
        private const string idPropertyName = "Id";

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
        internal DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// Creates a new item within the table.
        /// </summary>
        /// <param name="item">The item to add to the table.</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public async Task<HttpResponse<T>> CreateItemAsync(T item, CancellationToken token = default)
        {
            Validate.IsNotNull(item, nameof(item));

            using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint).WithJsonPayload(item, ClientOptions.SerializerOptions);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
        public async Task<HttpResponse> DeleteItemAsync(string id, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(HttpMethod.Delete, itemUri).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                return await HttpResponse.FromResponseAsync(message).ConfigureAwait(false);
            }
            else if (message.IsConflictStatusCode())
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
        public async Task<HttpResponse<T>> GetItemAsync(string id, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(HttpMethod.Get, itemUri).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
        public async Task<HttpResponse<T>> ReplaceItemAsync(T item, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsNotNull(item, nameof(item));
            var id = GetIdFromItem(item);
            Validate.IsValidId(id, nameof(item));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(HttpMethod.Put, itemUri)
                .WithJsonPayload(item, ClientOptions.SerializerOptions).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
        public async Task<HttpResponse<T>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, HttpCondition? precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));
            Validate.IsNotNullOrEmpty(changes, nameof(changes));

            var itemUri = new Uri(Endpoint, id);
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), itemUri)
                .WithJsonPayload(changes, ClientOptions.SerializerOptions).WithPrecondition(precondition);
            using var message = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (message.IsSuccessStatusCode)
            {
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
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
                var response = await HttpResponse.FromResponseAsync<T>(message, ClientOptions.DeserializerOptions).ConfigureAwait(false);
                throw new RequestFailedException(response);
            }
        }

        /// <summary>
        /// Finds the value of the id field via reflection.  If there is a field marked with <see cref="KeyAttribute"/>,
        /// then that is used.  If there is a field called <see cref="Id"/> then that is used.  If neither are available,
        /// then <see cref="MemberAccessException"/> is thrown.
        /// </summary>
        /// <param name="item">The item to process</param>
        /// <returns>The id of the item</returns>
        internal string GetIdFromItem(object item)
        {
            Validate.IsNotNull(item, nameof(item));
            var idProperty = Array.Find(item.GetType().GetProperties(), prop => prop.IsDefined(typeof(KeyAttribute)))
                ?? item.GetType().GetProperty(idPropertyName);
            if (idProperty == null)
            {
                throw new MissingMemberException($"{idPropertyName} not found, and no property has the [Key] attribute.");
            }
            object idValue = idProperty.GetValue(item);
            if (idValue == null)
            {
                throw new ArgumentNullException($"{idProperty.Name} is null", nameof(item));
            }
            if (idValue is string id)
            {
                return id;
            }
            throw new MemberAccessException($"{idProperty.Name} property is not a string");
        }
    }
}
