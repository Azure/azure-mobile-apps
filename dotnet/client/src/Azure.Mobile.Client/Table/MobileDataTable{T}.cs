using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mobile.Client.Utils;
using Azure.Mobile.Server.Utils;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Mobile.Client.Table
{
    /// <summary>
    /// Provides operations that handle a single Azure Mobile Table.
    /// </summary>
    /// <typeparam name="T">The type of the entity stored in the table.</typeparam>
    public class MobileDataTable<T> where T : TableData
    {
        private readonly HttpPipeline _pipeline;

        /// <summary>
        /// Initializes a <see cref="MobileDataTable{T}"/> instance.
        /// </summary>
        /// <param name="client">The <see cref="MobileDataClient"/> that created this table reference.</param>
        /// <param name="endpoint">The absolute Uri to the table controller endpoint.</param>
        internal MobileDataTable(MobileDataClient client, Uri endpoint)
        {
            Arguments.IsAbsoluteUri(endpoint, nameof(endpoint));

            Credential = client.Credential;
            ClientOptions = client.ClientOptions;
            Endpoint = endpoint;

            _pipeline = CreatePipeline(ClientOptions, new BearerTokenAuthenticationPolicy(Credential, GetDefaultScope(Endpoint)));
        }

        /// <summary>
        /// Creates a 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="authenticationPolicy"></param>
        /// <returns></returns>
        private static HttpPipeline CreatePipeline(MobileDataClientOptions options, HttpPipelinePolicy authenticationPolicy)
            => HttpPipelineBuilder.Build(options,
                new HttpPipelinePolicy[] { /* per-call policies */ },
                new HttpPipelinePolicy[] { authenticationPolicy },
                new ResponseClassifier());

        /// <summary>
        /// Obtains the default authentication scope.
        /// </summary>
        /// <param name="uri">The uri that we are connecting to</param>
        /// <returns>The scope string</returns>
        private static string GetDefaultScope(Uri uri)
            => $"{uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)}/.default";

        /// <summary>
        /// The base <see cref="Uri"/> for the backend table controller.
        /// </summary>
        internal Uri Endpoint { get; }

        /// <summary>
        /// The credential to use for authorization.
        /// </summary>
        internal TokenCredential Credential { get; }

        /// <summary>
        /// The client options for this connection.
        /// </summary>
        internal MobileDataClientOptions ClientOptions { get; }

        #region DeleteItem
        /// <summary>
        /// Deletes an item from the backend table, but only if the version matches. 
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual Task<Response> DeleteItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            MatchConditions requestOptions = (item.Version == null) ? default : new MatchConditions { IfMatch = new ETag(item.Version) };
            return DeleteItemAsync(item, requestOptions, cancellationToken);
        }

        /// <summary>
        /// Deletes an item from the backend table. 
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="requestOptions">The set of conditional request options to apply as headers
        /// to the request.</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual async Task<Response> DeleteItemAsync(T item, MatchConditions requestOptions, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            using Request request = CreateDeleteRequest(item, requestOptions);
            Response response = await _pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            return response.Status switch
            {
                200 => response,
                204 => response,
                412 => throw new ConflictException<T>(CreateResponse(response, cancellationToken)),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Deletes an item from the backend table, but only if the version matches. 
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual Response DeleteItem(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            MatchConditions requestOptions = (item.Version == null) ? default : new MatchConditions { IfMatch = new ETag(item.Version) };
            return DeleteItem(item, requestOptions, cancellationToken);
        }

        /// <summary>
        /// Deletes an item from the backend table. 
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="requestOptions">The set of conditional request options to apply as headers
        /// to the request.</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual Response DeleteItem(T item, MatchConditions requestOptions, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            using Request request = CreateDeleteRequest(item, requestOptions);
            Response response = _pipeline.SendRequest(request, cancellationToken);

            return response.Status switch
            {
                200 => response,
                204 => response,
                412 => throw new ConflictException<T>(CreateResponse(response, cancellationToken)),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Creates the request for a DELETE operation
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="requestOptions">The <see cref="MatchConditions"/> for this request, if any</param>
        /// <returns></returns>
        private Request CreateDeleteRequest(T item, MatchConditions requestOptions)
        {
            Request request = _pipeline.CreateRequest();

            request.Method = RequestMethod.Delete;
            request.BuildUri(Endpoint, item.Id);
            request.ApplyConditionalHeaders(requestOptions);

            return request;
        }
        #endregion

        #region GetItem
        /// <summary>
        /// Retrieves an item from the backend table, using the ID as an identifier.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The item as it appears in the backend service</returns>
        public async Task<Response<T>> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrEmpty(id, nameof(id));

            using Request request = CreateGetRequest(id, default);
            Response response = await _pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            return response.Status switch
            {
                200 => await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Retrieves an item from the backend table, using the ID as an identifier.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The item as it appears in the backend service</returns>
        public Response<T> GetItem(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrEmpty(id, nameof(id));

            using Request request = CreateGetRequest(id, default);
            Response response = _pipeline.SendRequest(request, cancellationToken);

            return response.Status switch
            {
                200 => CreateResponse(response, cancellationToken),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Creates a request for a GET ITEM operation
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <param name="requestOptions">The <see cref="MatchConditions"/> for this request, if any</param>
        /// <returns></returns>
        private Request CreateGetRequest(string id, MatchConditions requestOptions)
        {
            Request request = _pipeline.CreateRequest();

            request.Method = RequestMethod.Get;
            request.BuildUri(Endpoint, id);
            request.ApplyConditionalHeaders(requestOptions);

            return request;
        }
        #endregion

        #region InsertItem
        /// <summary>
        /// Inserts a new item into the backend table.  The item must not already exist.  If an Id
        /// is not provided in the local item, one will be added.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The item that was stored in the backend service</returns>
        public async Task<Response<T>> InsertItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = CreateInsertRequest(item);
            Response response = await _pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            return response.Status switch
            {
                201 => await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Inserts a new item into the backend table.  The item must not already exist.  If an Id
        /// is not provided in the local item, one will be added.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The item that was stored in the backend service</returns>
        public Response<T> InsertItem(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = CreateInsertRequest(item);
            Response response = _pipeline.SendRequest(request, cancellationToken);

            return response.Status switch
            {
                201 => CreateResponse(response, cancellationToken),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Creates a POST item (Create/Insert) operation request.
        /// </summary>
        /// <param name="item">The item to insert</param>
        /// <returns>A <see cref="Request"/> object ready for transmission</returns>
        private Request CreateInsertRequest(T item)
        {
            Request request = _pipeline.CreateRequest();

            request.Method = RequestMethod.Post;
            request.BuildUri(Endpoint, null);
            request.ApplyConditionalHeaders(new MatchConditions { IfNoneMatch = ETag.All });
            request.Headers.Add(HttpHeader.Common.JsonContentType);
            request.Content = CreateRequestContent(item);

            return request;
        }
        #endregion

        #region ReplaceItem
        /// <summary>
        /// Replaces the item in the backend service with the local item.  The item must have
        /// a version and the version must match what is on the backend service.
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public Task<Response<T>> ReplaceItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            MatchConditions requestOptions = (item.Version == null) ? default : new MatchConditions { IfMatch = new ETag(item.Version) };
            return ReplaceItemAsync(item, requestOptions, cancellationToken);
        }

        /// <summary>
        /// Replaces the item in the backend service with the local item, providing additional
        /// match conditions are met
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="requestOptions">The <see cref="MatchConditions"/> to be met</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public async Task<Response<T>> ReplaceItemAsync(T item, MatchConditions requestOptions, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = CreateReplaceRequest(item, requestOptions);
            Response response = await _pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            return response.Status switch
            {
                200 => await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false),
                412 => throw new ConflictException<T>(CreateResponse(response, cancellationToken)),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Replaces the item in the backend service with the local item.  The item must have
        /// a version and the version must match what is on the backend service.
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public Response<T> ReplaceItem(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            MatchConditions requestOptions = (item.Version == null) ? default : new MatchConditions { IfMatch = new ETag(item.Version) };
            return ReplaceItem(item, requestOptions, cancellationToken);
        }

        /// <summary>
        /// Replaces the item in the backend service with the local item, providing additional
        /// match conditions are met
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="requestOptions">The <see cref="MatchConditions"/> to be met</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public Response<T> ReplaceItem(T item, MatchConditions requestOptions, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = CreateReplaceRequest(item, requestOptions);
            Response response = _pipeline.SendRequest(request, cancellationToken);

            return response.Status switch
            {
                200 => CreateResponse(response, cancellationToken),
                412 => throw new ConflictException<T>(CreateResponse(response, cancellationToken)),
                _ => throw new RequestFailedException(response.Status, response.ReasonPhrase)
            };
        }

        /// <summary>
        /// Creates a PUT item (Replace) operation request.
        /// </summary>
        /// <param name="item">The item to replace</param>
        /// <returns>A <see cref="Request"/> object ready for transmission</returns>
        private Request CreateReplaceRequest(T item, MatchConditions requestOptions)
        {
            Request request = _pipeline.CreateRequest();

            request.Method = RequestMethod.Put;
            request.BuildUri(Endpoint, item.Id);
            request.ApplyConditionalHeaders(requestOptions);
            request.Headers.Add(HttpHeader.Common.JsonContentType);
            request.Content = CreateRequestContent(item);

            return request;
        }
        #endregion

        #region CreateResponse & CreateRequest
        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        private async Task<Response<T>> CreateResponseAsync(Response response, CancellationToken cancellationToken)
        {
            T result = await JsonSerializer.DeserializeAsync<T>(response.ContentStream, ClientOptions.JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            return Response.FromValue(result, response);

        }

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        private Response<T> CreateResponse(Response response, CancellationToken cancellationToken)
        {
            // TODO: There is probably a better way to do this; however, none of the System.Text.Json non-async methods take a stream.
            ValueTask<T> result = JsonSerializer.DeserializeAsync<T>(response.ContentStream, ClientOptions.JsonSerializerOptions, cancellationToken);
            return Response.FromValue(result.Result, response);
        }

        /// <summary>
        /// Serializes an item into it's JSON form, ready for transmission.
        /// </summary>
        /// <param name="item">The item to serialize.</param>
        /// <returns>A set of UTF-8 bytes to transmit.</returns>
        private RequestContent CreateRequestContent(T item)
            => RequestContent.Create(JsonSerializer.SerializeToUtf8Bytes<T>(item, ClientOptions.JsonSerializerOptions));
        #endregion
    }
}
