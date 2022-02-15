// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Communicates with the datasync service using DTO classes.
    /// </summary>
    internal class RemoteTable<T> : RemoteTable, IRemoteTable<T>
    {
        /// <summary>
        /// Creates a new <see cref="RemoteTable{T}"/> based on the settings provided by
        /// the <see cref="DatasyncClient"/>.
        /// </summary>
        /// <param name="relativeUri">The relative URI to the table, from the endpoint defined in the <paramref name="client"/>.</param>
        /// <param name="client">The <see cref="ServiceHttpClient"/> to use for communication.</param>
        /// <param name="options">The <see cref="DatasyncClientOptions"/> to use for handling communication.</param>
        internal RemoteTable(string relativeUri, ServiceHttpClient client, DatasyncClientOptions options)
            : base(relativeUri, client, options)
        {
            Features = DatasyncFeatures.TypedTable;
        }

        /// <summary>
        /// Creates a new <see cref="RemoteTable{T}"/> based on an existing table.  This is
        /// used to create a table with the same settings but a subset of fields.
        /// </summary>
        /// <param name="source">The source table</param>
        internal RemoteTable(RemoteTable source)
            : base(source.RelativeUri, source.HttpClient, source.ClientOptions)
        {
            Features = DatasyncFeatures.TypedTable;
        }

        /// <summary>
        /// An event that is fired when the table is modified - an entity is either
        /// created, deleted, or updated.
        /// </summary>
        public override event EventHandler<TableModifiedEventArgs> TableModified;

        #region IRemoteTable<T>
        /// <summary>
        /// Creates a query based on this table.
        /// </summary>
        /// <returns>A query against this table.</returns>
        public ITableQuery<T> CreateQuery() => new TableQuery<T>(this);

        /// <summary>
        /// Deletes an existing item within the table.
        /// </summary>
        /// <param name="item">The ID of the item to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>A <see cref="ServiceResponse"/> object.</returns>
        public async Task<ServiceResponse> DeleteItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(item, nameof(item));

            ObjectReader.GetSystemProperties(item, out DatasyncClientData systemProperties);
            if (string.IsNullOrEmpty(systemProperties.Id))
            {
                throw new ArgumentException("Item does not have an ID field", nameof(item));
            }
            var precondition = systemProperties.Version == null ? null : IfMatch.Version(systemProperties.Version);
            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(Endpoint, systemProperties.Id))
                .WithFeatureHeader(Features)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
                OnItemDeleted(systemProperties.Id);
                request.Dispose();
                response.Dispose();
                return result;
            }
            else
            {
                throw await ThrowResponseException(request, response, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query string to send to the service</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        public AsyncPageable<U> GetAsyncItems<U>(string query = "", CancellationToken cancellationToken = default)
            => new FuncAsyncPageable<U>(nextLink => GetNextPageAsync<U>(query, nextLink, cancellationToken));

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <typeparam name="U">The type of the items being returned - can be a subset of properties in the table entity</typeparam>
        /// <param name="query">The query definition to send to the service</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        public AsyncPageable<U> GetAsyncItems<U>(ITableQuery<U> query, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(query, nameof(query));
            return query.ToAsyncPageable(cancellationToken);
        }

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        public new async Task<ServiceResponse<T>> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Validate.IsValidId(id, nameof(id));

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Endpoint, id))
                .WithFeatureHeader(DatasyncFeatures.TypedTable);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                request.Dispose();
                response.Dispose();
                return result;
            }
            else
            {
                throw await ThrowResponseException(request, response, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a new item within the table.
        /// </summary>
        /// <param name="item">The item to add to the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        public async Task<ServiceResponse<T>> InsertItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(item, nameof(item));

            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
                .WithFeatureHeader(Features)
                .WithContent(item, ClientOptions.SerializerOptions);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                OnItemInserted(result.Value);
                request.Dispose();
                response.Dispose();
                return result;
            }
            else
            {
                throw await ThrowResponseException(request, response, cancellationToken).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Refresh the current instance with the latest values from the table.
        /// </summary>
        /// <param name="item">The item to refresh.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The refreshed item.</returns>
        public async Task<T> RefreshItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(item, nameof(item));

            ObjectReader.GetSystemProperties(item, out DatasyncClientData systemProperties);
            if (string.IsNullOrEmpty(systemProperties.Id))
            {
                throw new ArgumentException("Item does not have an ID field", nameof(item));
            }
            var precondition = systemProperties.Version == null ? null : IfMatch.Version(systemProperties.Version);
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Endpoint, systemProperties.Id))
                .WithFeatureHeader(Features)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                request.Dispose();
                response.Dispose();
                return result.Value;
            }
            else if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return item;
            }
            else
            {
                throw await ThrowResponseException(request, response, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Replace the item with a new copy of the item.  Note that the item must have an Id (either called Id,
        /// or decorated with the <see cref="KeyAttribute"/> attribute) that is a string and set.
        /// </summary>
        /// <param name="item">the replacement item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public async Task<ServiceResponse<T>> ReplaceItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(item, nameof(item));

            ObjectReader.GetSystemProperties(item, out DatasyncClientData systemProperties);
            if (string.IsNullOrEmpty(systemProperties.Id))
            {
                throw new ArgumentException("Item does not have an ID field", nameof(item));
            }
            var precondition = systemProperties.Version == null ? null : IfMatch.Version(systemProperties.Version);
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(Endpoint, systemProperties.Id))
                .WithFeatureHeader(Features)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue)
                .WithContent(item, ClientOptions.SerializerOptions);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                OnItemReplaced(result.Value);
                request.Dispose();
                response.Dispose();
                return result;
            }
            else
            {
                throw await ThrowResponseException(request, response, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe</param>
        /// <returns>A <see cref="HttpResponse{T}"/> object with the item that was stored.</returns>
        public new async Task<ServiceResponse<T>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, IfMatch precondition, CancellationToken cancellationToken)
        {
            Validate.IsValidId(id, nameof(id));
            Validate.IsNotNullOrEmpty(changes, nameof(changes));

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), new Uri(Endpoint, id))
                .WithFeatureHeader(Features)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue)
                .WithContent(changes, ClientOptions.SerializerOptions);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<T>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                OnItemReplaced(result.Value);
                request.Dispose();
                response.Dispose();
                return result;
            }
            else
            {
                throw await ThrowResponseException(request, response, cancellationToken).ConfigureAwait(true);
            }
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
        /// Execute the query, returning an <see cref="AsyncPageable{T}"/>
        /// </summary>
        /// <returns>An <see cref="AsyncPageable{T}"/> to iterate over the items</returns>
        public AsyncPageable<T> ToAsyncPageable(CancellationToken token = default)
            => GetAsyncItems<T>("$count=true", token);

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
        /// Adds the list of parameters to the list of user-defined parameters to send
        /// with the request
        /// </summary>
        /// <param name="parameters">The parameters to apply.</param>
        /// <returns>The composed query object.</returns>
        public ITableQuery<T> WithParameters(IEnumerable<KeyValuePair<string, string>> parameters)
            => CreateQuery().WithParameters(parameters);
        #endregion

        /// <summary>
        /// Gets a single page of items produced as a result of a query against the server.
        /// </summary>
        /// <param name="query">The query string to send to the service.</param>
        /// <param name="requestUri">The request URI to send (if we're on the second or future pages)</param>
        /// <param name="token">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing the page of items.</returns>
        internal virtual async Task<ServiceResponse<Page<U>>> GetNextPageAsync<U>(string query = "", string requestUri = null, CancellationToken token = default)
        {
            Uri uri = requestUri != null ? new Uri(requestUri) : new UriBuilder(Endpoint).WithQuery(query).Uri;
            var request = new HttpRequestMessage(HttpMethod.Get, uri).WithFeatureHeader(Features);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<Page<U>>(response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
                request.Dispose();
                response.Dispose();
                return result;
            }
            else
            {
                throw await ThrowResponseException(request, response, token).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Post an "item deleted" event to the <see cref="TableModified"/> event handler.
        /// </summary>
        /// <param name="id">The ID of the item that was deleted.</param>
        private void OnItemDeleted(string id)
        {
            TableModified?.Invoke(this, new TableModifiedEventArgs
            {
                TableEndpoint = Endpoint,
                Operation = TableModifiedEventArgs.TableOperation.Delete,
                Id = id
            });
        }

        /// <summary>
        /// Post an "item inserted" event to the <see cref="TableModified"/> event handler.
        /// </summary>
        /// <param name="item">The item that was inserted.</param>
        private void OnItemInserted(T item)
        {
            ObjectReader.GetSystemProperties(item, out DatasyncClientData systemProperties);
            TableModified?.Invoke(this, new TableModifiedEventArgs
            {
                TableEndpoint = Endpoint,
                Operation = TableModifiedEventArgs.TableOperation.Create,
                Id = systemProperties.Id,
                Entity = item
            });
        }

        /// <summary>
        /// Post an "item replaced" event to the <see cref="TableModified"/> event handler.
        /// </summary>
        /// <param name="item">The replacement item.</param>
        private void OnItemReplaced(T item)
        {
            ObjectReader.GetSystemProperties(item, out DatasyncClientData systemProperties);
            TableModified?.Invoke(this, new TableModifiedEventArgs
            {
                TableEndpoint = Endpoint,
                Operation = TableModifiedEventArgs.TableOperation.Replace,
                Id = systemProperties.Id,
                Entity = item
            });
        }

        /// <summary>
        /// Throws the standard errors.  If this method returns, it wasn't a valid error condition.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> that caused the error.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/> that caused the error.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The exception to throw.</returns>
        private async Task<Exception> ThrowResponseException(HttpRequestMessage request, HttpResponseMessage response, CancellationToken token = default)
        {
            if (response.IsConflictStatusCode())
            {
                return await DatasyncConflictException<T>.CreateAsync(request, response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
            }
            else if (response.Content != null)
            {
                return new DatasyncOperationException(request, response, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            else
            {
                return new DatasyncOperationException(request, response);
            }
        }
    }
}
