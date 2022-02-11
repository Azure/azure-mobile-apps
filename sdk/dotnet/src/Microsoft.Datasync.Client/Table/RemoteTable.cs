// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Communicates with the datasync service using JSON objects.
    /// </summary>
    internal class RemoteTable : IRemoteTable
    {
        /// <summary>
        /// Creates a new <see cref="RemoteTable"/> at the provided endpoint.
        /// </summary>
        /// <param name="relativeUri">The relative URI to the endpoint.</param>
        /// <param name="client">The <see cref="ServiceHttpClient"/> to use for communication.</param>
        /// <param name="options">The client options for adjusting the request and response.</param>
        public RemoteTable(string relativeUri, ServiceHttpClient client, DatasyncClientOptions options)
        {
            Validate.IsRelativeUri(relativeUri, nameof(relativeUri));
            Validate.IsNotNull(client, nameof(client));
            Validate.IsNotNull(options, nameof(options));

            Endpoint = new Uri(client.Endpoint, relativeUri.TrimStart('/')).NormalizeEndpoint();
            HttpClient = client;
            ClientOptions = options;
        }

        /// <summary>
        /// The <see cref="ServiceHttpClient"/> to use for communication.
        /// </summary>
        internal ServiceHttpClient HttpClient { get; }

        /// <summary>
        /// The list of features to send to the remote service.
        /// </summary>
        protected DatasyncFeatures Features { get; set; } = DatasyncFeatures.UntypedTable;

        #region IRemoteTable
        /// <summary>
        /// The base <see cref="Uri"/> for the table.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// The <see cref="DatasyncClientOptions"/> for the table.
        /// </summary>
        public DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// An event that is fired when the table is modified - an entity is either
        /// created, deleted, or updated.
        /// </summary>
        public event EventHandler<TableModifiedEventArgs> TableModified;

        /// <summary>
        /// Deletes an item from the store.  If the version is provided, then the document
        /// is only deleted if the store version matches the provided version.
        /// </summary>
        /// <remarks>
        /// The item provided must have an ID, and may have a version.  All other properties
        /// on the JSON document are ignored.
        /// </remarks>
        /// <param name="item">The item to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The service response if successful.</returns>
        /// <exception cref="ArgumentException">if the item provided does not have an ID.</exception>
        /// <exception cref="DatasyncConflictException{T}">if there is a version mismatch.</exception>
        /// <exception cref="DatasyncOperationException">if an HTTP error is received from the service.</exception>
        public async Task<ServiceResponse> DeleteItemAsync(JsonDocument item, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(item, nameof(item));
            string id = item.GetId();
            Validate.IsValidId(id, nameof(item));

            var version = item.GetVersion();
            var precondition = version == null ? null : IfMatch.Version(version);
            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(Endpoint, id))
                .WithFeatureHeader(Features)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync(response, cancellationToken).ConfigureAwait(false);
                OnItemDeleted(id);
                return result;
            }
            else if (response.IsConflictStatusCode())
            {
                throw await DatasyncConflictException<JsonDocument>.CreateAsync(request, response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new DatasyncOperationException(request, response);
            }
        }

        /// <summary>
        /// Retrieves a list of items based on a query.
        /// </summary>
        /// <param name="query">The query string to send to the service.  This can be any OData compatible query string.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>An <see cref="AsyncPageable{T}"/> for retrieving the items asynchronously.</returns>
        public AsyncPageable<JsonDocument> GetAsyncItems(string query = "", CancellationToken cancellationToken = default)
            => new FuncAsyncPageable<JsonDocument>(nextLink => GetNextPageAsync(query, nextLink, cancellationToken));

        /// <summary>
        /// Retrieves an item within the table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The service response containing the object if successful.</returns>
        public async Task<ServiceResponse<JsonDocument>> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Validate.IsValidId(id, nameof(id));

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Endpoint, id)).WithFeatureHeader(Features);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return await ServiceResponse.FromResponseAsync<JsonDocument>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new DatasyncOperationException(request, response);
            }
        }

        /// <summary>
        /// Inserts an item into the store. 
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The service response containing the inserted object if successful.</returns>
        /// <exception cref="DatasyncConflictException{T}">if the item to insert has an ID and that ID already exists in the table.</exception>
        /// <exception cref="DatasyncOperationException">if an HTTP error is received from the service.</exception>
        public async Task<ServiceResponse<JsonDocument>> InsertItemAsync(JsonDocument item, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(item, nameof(item));

            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
                .WithFeatureHeader(Features)
                .WithJsonContent(item);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<JsonDocument>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                OnItemInserted(result.Value);
                request.Dispose();
                response.Dispose();
                return result;
            }
            else if (response.IsConflictStatusCode())
            {
                throw await DatasyncConflictException<JsonDocument>.CreateAsync(request, response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new DatasyncOperationException(request, response);
            }
        }

        /// <summary>
        /// Replace the item with a new copy of the item.
        /// </summary>
        /// <remarks>
        /// The item must contain an "id" field.  If the item also contains a "version" field, the version
        /// must match the version on the server.</remarks>
        /// <param name="item">the replacement item</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        /// <exception cref="DatasyncConflictException{T}">if there is a version mismatch.</exception>
        /// <exception cref="DatasyncOperationException">if an HTTP error is received from the service.</exception>
        public async Task<ServiceResponse<JsonDocument>> ReplaceItemAsync(JsonDocument item, CancellationToken cancellationToken = default)
        {
            Validate.IsNotNull(item, nameof(item));
            string id = item.GetId();
            Validate.IsValidId(id, nameof(item));

            var version = item.GetVersion();
            var precondition = version == null ? null : IfMatch.Version(version);
            var request = new HttpRequestMessage(HttpMethod.Put, new Uri(Endpoint, id))
                .WithFeatureHeader(Features)
                .WithHeader(precondition?.HeaderName, precondition?.HeaderValue)
                .WithJsonContent(item);
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if(response.IsSuccessStatusCode)
            {
                var result = await ServiceResponse.FromResponseAsync<JsonDocument>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                OnItemReplaced(result.Value);
                return result;
            }
            else if (response.IsConflictStatusCode())
            {
                throw await DatasyncConflictException<JsonDocument>.CreateAsync(request, response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new DatasyncOperationException(request, response);
            }
        }

        /// <summary>
        /// Update the specified item with the provided changes.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="changes">A list of changes to apply to the item</param>
        /// <param name="precondition">An optional <see cref="HttpCondition"/> for conditional operation</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> object with the item that was stored.</returns>
        public async Task<ServiceResponse<JsonDocument>> UpdateItemAsync(string id, IReadOnlyDictionary<string, object> changes, IfMatch precondition = null, CancellationToken cancellationToken = default)
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
                var result = await ServiceResponse.FromResponseAsync<JsonDocument>(response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
                OnItemReplaced(result.Value);
                return result;
            }
            else if (response.IsConflictStatusCode())
            {
                throw await DatasyncConflictException<JsonDocument>.CreateAsync(request, response, ClientOptions.DeserializerOptions, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new DatasyncOperationException(request, response);
            }
        }
        #endregion

        /// <summary>
        /// Gets a single page of items produced as a result of a query against the server.
        /// </summary>
        /// <param name="query">The query string to send to the service.</param>
        /// <param name="requestUri">The request URI to send (if we're on the second or future pages)</param>
        /// <param name="token">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> containing the page of items.</returns>
        internal virtual async Task<ServiceResponse<Page<JsonDocument>>> GetNextPageAsync(string query = "", string requestUri = null, CancellationToken token = default)
        {
            Uri uri = requestUri != null ? new Uri(requestUri) : new UriBuilder(Endpoint).WithQuery(query).Uri;
            var request = new HttpRequestMessage(HttpMethod.Get, uri).WithFeatureHeader(Features);
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return await ServiceResponse.FromResponseAsync<Page<JsonDocument>>(response, ClientOptions.DeserializerOptions, token).ConfigureAwait(false);
            }
            else
            {
                throw new DatasyncOperationException(request, response);
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
        private void OnItemInserted(JsonDocument item)
        {
            TableModified?.Invoke(this, new TableModifiedEventArgs
            {
                TableEndpoint = Endpoint,
                Operation = TableModifiedEventArgs.TableOperation.Create,
                Id = item.GetId(),
                Entity = item
            });
        }

        /// <summary>
        /// Post an "item replaced" event to the <see cref="TableModified"/> event handler.
        /// </summary>
        /// <param name="item">The replacement item.</param>
        private void OnItemReplaced(JsonDocument item)
        {
            TableModified?.Invoke(this, new TableModifiedEventArgs
            {
                TableEndpoint = Endpoint,
                Operation = TableModifiedEventArgs.TableOperation.Replace,
                Id = item.GetId(),
                Entity = item
            });
        }
    }
}
