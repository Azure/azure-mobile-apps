// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
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
        /// The name of the default Id property.
        /// </summary>
        private const string idPropertyName = "Id";

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

            Endpoint = new Uri(client.Endpoint, relativeUri.TrimStart('/')).NormalizeEndpoint();
            HttpClient = client;
            ClientOptions = options;
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncTable{T}"/> at the provided endpoint.
        /// </summary>
        /// <param name="endpoint">The absolute Uri to the table endpoint.</param>
        /// <param name="client">The <see cref="InternalHttpClient"/> to use for communication.</param>
        /// <param name="options">The client options for adjusting the request and response.</param>
        public DatasyncTable(Uri endpoint, InternalHttpClient client, DatasyncClientOptions options)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));
            Validate.IsNotNull(client, nameof(client));
            Validate.IsNotNull(options, nameof(options));

            Endpoint = endpoint;
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
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<ServiceResponse<T>> CreateItemAsync(T item, CancellationToken token = default)
        {
            Validate.IsNotNull(item, nameof(item));

            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
                .WithFeatureHeader(DatasyncFeatures.TypedTable)
                .WithContent(item, ClientOptions.SerializerOptions);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);

            return (int)response.StatusCode switch
            {
                200 or 201 => await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                409 or 412 => throw await DatasyncConflictException<T>.CreateAsync(request, response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                _ => throw new DatasyncOperationException(request, response)
            };
        }

        /// <summary>
        /// Deletes an existing item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to delete.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse"/> object.</returns>
        public virtual async Task<ServiceResponse> DeleteItemAsync(string id, IfMatch precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));

            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(Endpoint, id))
                .WithFeatureHeader(DatasyncFeatures.TypedTable)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);

            return (int)response.StatusCode switch
            {
                204 => await ServiceResponse.FromResponseAsync(response, token).ConfigureAwait(false),
                409 or 412 => throw await DatasyncConflictException<T>.CreateAsync(request, response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                _ => throw new DatasyncOperationException(request, response)
            };
        }

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query string to send to the service</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        public virtual AsyncPageable<U> GetAsyncItems<U>(string query = "", CancellationToken token = default)
            => new FuncAsyncPageable<U>(nextLink => GetPageOfItemsAsync<U>(query, nextLink, token));

        /// <summary>
        /// Gets a single page of items produced as a result of a query against the server.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query string to send to the service</param>
        /// <param name="requestUri">The request URI to send (if multiple pages)</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing the page of items</returns>
        internal virtual async Task<ServiceResponse<Page<U>>> GetPageOfItemsAsync<U>(string query = "", string requestUri = null, CancellationToken token = default)
        {
            Uri uri = requestUri != null ? new Uri(requestUri) : new UriBuilder(Endpoint).WithQuery(query).Uri;

            var request = new HttpRequestMessage(HttpMethod.Get, uri)
                .WithFeatureHeader(DatasyncFeatures.TypedTable);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);

            return (int)response.StatusCode switch
            {
                200 => await ServiceResponse.FromResponseAsync<Page<U>>(response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                _ => throw new DatasyncOperationException(request, response)
            };
        }

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<ServiceResponse<T>> GetItemAsync(string id, IfNoneMatch precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Endpoint, id))
                .WithFeatureHeader(DatasyncFeatures.TypedTable)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);

            return (int)response.StatusCode switch
            {
                200 => await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                304 => throw new EntityNotModifiedException(request, response),
                _ => throw new DatasyncOperationException(request, response)
            };
        }

        /// <summary>
        /// Replace the item with a new copy of the item.  Note that the item must have an Id (either called Id,
        /// or decorated with the <see cref="KeyAttribute"/> attribute) that is a string and set.
        /// </summary>
        /// <param name="item">the replacement item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<ServiceResponse<T>> ReplaceItemAsync(T item, IfMatch precondition = null, CancellationToken token = default)
        {
            Validate.IsNotNull(item, nameof(item));
            var id = GetIdFromItem(item);
            Validate.IsValidId(id, nameof(item));

            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(Endpoint, id))
                .WithFeatureHeader(DatasyncFeatures.TypedTable)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue)
                .WithContent(item, ClientOptions.SerializerOptions);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);

            return (int)response.StatusCode switch
            {
                200 => await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                409 or 412 => throw await DatasyncConflictException<T>.CreateAsync(request, response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                _ => throw new DatasyncOperationException(request, response)
            };
        }

        /// <summary>
        ///Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        public virtual async Task<ServiceResponse<T>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, IfMatch precondition = null, CancellationToken token = default)
        {
            Validate.IsValidId(id, nameof(id));
            Validate.IsNotNullOrEmpty(changes, nameof(changes));

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(Endpoint, id))
                .WithFeatureHeader(DatasyncFeatures.TypedTable)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue)
                .WithContent(changes, ClientOptions.SerializerOptions);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);

            return (int)response.StatusCode switch
            {
                200 => await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                409 or 412 => throw await DatasyncConflictException<T>.CreateAsync(request, response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false),
                _ => throw new DatasyncOperationException(request, response)
            };
        }

        /// <summary>
        /// Finds the value of the id field via reflection.  If there is a field marked with <see cref="IdAttribute"/>,
        /// then that is used.  If there is a field called <see cref="Id"/> then that is used.  If neither are available,
        /// then <see cref="MemberAccessException"/> is thrown.
        /// </summary>
        /// <param name="item">The item to process</param>
        /// <returns>The id of the item</returns>
        internal static string GetIdFromItem(T item)
        {
            Validate.IsNotNull(item, nameof(item));

            var idProperty = Array.Find(item.GetType().GetProperties(), prop => prop.IsDefined(typeof(IdAttribute)))
                ?? item.GetType().GetProperty(idPropertyName);
            if (idProperty == null)
            {
                throw new MissingMemberException($"{idPropertyName} not found, and no property has the [Id] attribute.");
            }
            object idValue = idProperty.GetValue(item);
            if (idValue == null)
            {
                throw new ArgumentException($"{idProperty.Name} is null", nameof(item));
            }
            else if (idValue is string id)
            {
                return id;
            }
            else
            {
                throw new MemberAccessException($"{idProperty.Name} property is not a string");
            }
        }

        /// <summary>
        /// Converts the current table to a table with a new type, but the same endpoint, client, and options.
        /// </summary>
        /// <typeparam name="U">The new type of the supported items</typeparam>
        /// <returns>The new table</returns>
        public virtual IDatasyncTable<U> WithType<U>()
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
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        public AsyncPageable<T> ToAsyncPageable(CancellationToken token = default)
            => GetAsyncItems<T>("$count=true", token);

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
