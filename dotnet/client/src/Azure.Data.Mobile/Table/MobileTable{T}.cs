using Azure.Core;
using Azure.Core.Pipeline;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Data.Mobile
{
    /// <summary>
    /// Provides operations that handle a single Azure Mobile Table.
    /// </summary>
    /// <typeparam name="T">The type of the entity stored in the table.</typeparam>
    public class MobileTable<T> where T : TableData
    {
        /// <summary>
        /// Initializes a <see cref="MobileDataTable{T}"/> instance.
        /// </summary>
        /// <param name="client">The <see cref="MobileTableClient"/> that created this table reference.</param>
        /// <param name="endpoint">The absolute Uri to the table controller endpoint.</param>
        internal MobileTable(MobileTableClient client, Uri endpoint)
        {
            Arguments.IsAbsoluteUri(endpoint, nameof(endpoint));

            Credential = client.Credential;
            ClientOptions = client.ClientOptions;
            Endpoint = endpoint;

            var perCallPolicies = new List<HttpPipelinePolicy>();
            var perRetryPolicies = new List<HttpPipelinePolicy>();

            // Add the authentication policy only if supplied a credential.
            if (Credential != null)
            {
                var scope = $"{Endpoint.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)}/.default";
                perRetryPolicies.Add(new BearerTokenAuthenticationPolicy(Credential, scope));
            }

            // Builds the pipeline based on the policies provided.
            Pipeline = HttpPipelineBuilder.Build(
                ClientOptions, 
                perCallPolicies.ToArray(), 
                perRetryPolicies.ToArray(), 
                new ResponseClassifier());
        }

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
        internal MobileTableClientOptions ClientOptions { get; }

        /// <summary>
        /// The Azure.Core Pipeline used for communicating with to the service
        /// </summary>
        internal HttpPipeline Pipeline { get; }

        #region GetMetadata
        /// <summary>
        /// Obtains the table metadata.
        /// </summary>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual Task<Response<MobileTableMetadata>> GetMetadataAsync(CancellationToken cancellationToken = default)
            => GetMetadataAsync(new MobileTableQueryOptions(), cancellationToken);

        /// <summary>
        /// Obtains the table metadata.  The count returned is contstrained by the query provided.
        /// </summary>
        /// <param name="query">The <see cref="MobileTableQueryOptions"/> describing the query</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual async Task<Response<MobileTableMetadata>> GetMetadataAsync(MobileTableQueryOptions query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));

            using Request request = CreateGetMetadataRequest(query);
            Response response = await Pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                    return await CreateResponseAsync<MobileTableMetadata>(response, cancellationToken).ConfigureAwait(false);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Obtains the table metadata.
        /// </summary>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual Response<MobileTableMetadata> GetMetadata(CancellationToken cancellationToken = default)
            => GetMetadata(new MobileTableQueryOptions(), cancellationToken);

        /// <summary>
        /// Obtains the table metadata.  The count returned is contstrained by the query provided.
        /// </summary>
        /// <param name="query">The <see cref="MobileTableQueryOptions"/> describing the query</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The table metadata</returns>
        public virtual Response<MobileTableMetadata> GetMetadata(MobileTableQueryOptions query, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(query, nameof(query));

            using Request request = CreateGetMetadataRequest(query);
            Response response = Pipeline.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                    return CreateResponse<MobileTableMetadata>(response, cancellationToken);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Creates a request object for a paged response.
        /// </summary>
        /// <param name="query">The query to send</param>
        /// <param name="pageLink">The link to the next page</param>
        /// <returns>The <see cref="Request"/> object corresponding to the request</returns>
        private Request CreateGetMetadataRequest(MobileTableQueryOptions query)
        {
            Request request = Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            var builder = request.Uri;
            builder.Reset(Endpoint);
            if (query.Filter != null)
            {
                builder.AppendQuery("$filter", query.Filter);
            }
            if (query.OrderBy != null)
            {
                builder.AppendQuery("$orderBy", query.OrderBy);
            }
            if (query.IncludeDeleted)
            {
                builder.AppendQuery("__includedeleted", "true");
            }
            builder.AppendQuery("$count", "true");
            builder.AppendQuery("__excludeitems", "true");

            return request;
        }
        #endregion

        #region DeleteItem
        /// <summary>
        /// Deletes an item from the backend table.  By default, the item is only deleted if it matches the item version.
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="conditions">The set of conditional request options to apply as headers to the request.</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual async Task<Response> DeleteItemAsync(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            using Request request = CreateDeleteRequest(item, conditions ?? new MatchConditions { IfMatch = new ETag(item.Version) });
            Response response = await Pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                case 204:
                    return response;
                case 412:
                    throw new ConflictException<T>(CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Deletes an item from the backend table.  By default, the item is only deleted if it matches the item version.
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="conditions">The set of conditional request options to apply as headers to the request.</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A <see cref="Response"/> from the backend service.</returns>
        public virtual Response DeleteItem(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            using Request request = CreateDeleteRequest(item, conditions ?? new MatchConditions { IfMatch = new ETag(item.Version) });
            Response response = Pipeline.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                case 204:
                    return response;
                case 412:
                    throw new ConflictException<T>(CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Creates the request for a DELETE operation
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="conditions">The <see cref="MatchConditions"/> for this request, if any</param>
        /// <returns></returns>
        private Request CreateDeleteRequest(T item, MatchConditions conditions)
        {
            Request request = Pipeline.CreateRequest();

            request.Method = RequestMethod.Delete;
            request.BuildUri(Endpoint, item.Id);
            request.ApplyConditionalHeaders(conditions);

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
        public virtual async Task<Response<T>> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrEmpty(id, nameof(id));

            using Request request = CreateGetRequest(id, default);
            Response response = await Pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                    return await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Retrieves an item from the backend table, using the ID as an identifier.
        /// </summary>
        /// <param name="id">The ID of the record to retrieve</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The item as it appears in the backend service</returns>
        public virtual Response<T> GetItem(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNullOrEmpty(id, nameof(id));

            using Request request = CreateGetRequest(id, default);
            Response response = Pipeline.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                    return CreateResponse(response, cancellationToken);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Creates a request for a GET ITEM operation
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <param name="requestOptions">The <see cref="MatchConditions"/> for this request, if any</param>
        /// <returns>A pageable list of items</returns>
        private Request CreateGetRequest(string id, MatchConditions requestOptions)
        {
            Request request = Pipeline.CreateRequest();

            request.Method = RequestMethod.Get;
            request.BuildUri(Endpoint, id);
            request.ApplyConditionalHeaders(requestOptions);

            return request;
        }
        #endregion

        #region GetItems
        /// <summary>
        /// Retrieves a list of items from the service.
        /// </summary>
        /// <param name="options">The query to send to the remote server</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A pageable list of items</returns>
        public virtual AsyncPageable<T> GetItemsAsync(MobileTableQueryOptions options = null, CancellationToken cancellationToken = default)
        {
            return PageResponseEnumerator.CreateAsyncEnumerable(nextLink => GetItemsPageAsync(options, nextLink, cancellationToken));
        }

        /// <summary>
        /// Retrieves a list of items from the service.
        /// </summary>
        /// <param name="options">The query to send to the remote server</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A pageable list of items</returns>
        public virtual Pageable<T> GetItems(MobileTableQueryOptions options = null, CancellationToken cancellationToken = default)
        {
            return PageResponseEnumerator.CreateEnumerable(nextLink => GetItemsPage(options, nextLink, cancellationToken));
        }

        /// <summary>
        /// Fetches a single page in the server-side paging result of a list operation.
        /// </summary>
        /// <param name="options">The query to send to the service</param>
        /// <param name="pageLink">The link to the page</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A single page of results</returns>
        private async Task<Page<T>> GetItemsPageAsync(MobileTableQueryOptions options, string pageLink, CancellationToken cancellationToken = default)
        {
            using Request request = CreateListRequest(options, pageLink);
            Response response = await Pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                    PagedResult<T> data = await CreatePagedResultAsync(response, cancellationToken).ConfigureAwait(false);
                    return Page<T>.FromValues(data.Values, data.NextLink, response);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Fetches a single page in the server-side paging result of a list operation.
        /// </summary>
        /// <param name="options">The query to send to the service</param>
        /// <param name="pageLink">The link to the page</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>A single page of results</returns>
        private Page<T> GetItemsPage(MobileTableQueryOptions options, string pageLink, CancellationToken cancellationToken = default)
        {
            using Request request = CreateListRequest(options, pageLink);
            Response response = Pipeline.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                    PagedResult<T> data = CreatePagedResult(response, cancellationToken);
                    return Page<T>.FromValues(data.Values, data.NextLink, response);
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Creates a request object for a paged response.
        /// </summary>
        /// <param name="options">The query to send</param>
        /// <param name="pageLink">The link to the next page</param>
        /// <returns>The <see cref="Request"/> object corresponding to the request</returns>
        private Request CreateListRequest(MobileTableQueryOptions options, string pageLink)
        {
            Request request = Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            if (pageLink != null)
            {
                request.Uri.Reset(new Uri(pageLink));
            } 
            else
            {
                var builder = request.Uri;
                builder.Reset(Endpoint);
                if (options != null)
                {
                    if (options.Filter != null)
                    {
                        builder.AppendQuery("$filter", options.Filter);
                    }
                    if (options.OrderBy != null)
                    {
                        builder.AppendQuery("$orderBy", options.OrderBy);
                    }
                    if (options.Skip >= 0)
                    {
                        builder.AppendQuery("$skip", $"{options.Skip}");
                    }
                    if (options.Top >= 0)
                    {
                        builder.AppendQuery("$top", $"{options.Top}");
                    }
                    if (options.IncludeDeleted)
                    {
                        builder.AppendQuery("__includedeleted", "true");
                    }
                    if (options.IncludeCount)
                    {
                        builder.AppendQuery("$count", "true");
                    }
                }
            }
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
        public virtual async Task<Response<T>> InsertItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = CreateInsertRequest(item);
            Response response = await Pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 201:
                    return await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false);
                case 409:
                case 412:
                    throw new ConflictException<T>(await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Inserts a new item into the backend table.  The item must not already exist.  If an Id
        /// is not provided in the local item, one will be added.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The item that was stored in the backend service</returns>
        public virtual Response<T> InsertItem(T item, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));

            using Request request = CreateInsertRequest(item);
            Response response = Pipeline.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 201:
                    return CreateResponse(response, cancellationToken);
                case 409:
                case 412:
                    throw new ConflictException<T>(CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Creates a POST item (Create/Insert) operation request.
        /// </summary>
        /// <param name="item">The item to insert</param>
        /// <returns>A <see cref="Request"/> object ready for transmission</returns>
        private Request CreateInsertRequest(T item)
        {
            Request request = Pipeline.CreateRequest();

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
        /// Replaces the item in the backend service with the local item, providing additional match conditions are met.  By default, replacement
        /// only happens if the version provided matches the version on the server.
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="conditions">The <see cref="MatchConditions"/> to be met</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public virtual async Task<Response<T>> ReplaceItemAsync(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNull(item.Id, nameof(item.Id));

            using Request request = CreateReplaceRequest(item, conditions ?? new MatchConditions { IfMatch = new ETag(item.Version) });
            Response response = await Pipeline.SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

            switch (response.Status)
            {
                case 200:
                    return await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false);
                case 409:
                case 412:
                    throw new ConflictException<T>(await CreateResponseAsync(response, cancellationToken).ConfigureAwait(false));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Replaces the item in the backend service with the local item, providing additional match conditions are met.  By default, replacement
        /// only happens if the version provided matches the version on the server.
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <param name="conditions">The <see cref="MatchConditions"/> to be met</param>
        /// <param name="cancellationToken">A lifecycle token for cancelling the request.</param>
        /// <returns>The updated item, as it appears in the backend service.</returns>
        public virtual Response<T> ReplaceItem(T item, MatchConditions conditions = null, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNull(item.Id, nameof(item.Id));

            using Request request = CreateReplaceRequest(item, conditions ?? new MatchConditions { IfMatch = new ETag(item.Version) });
            Response response = Pipeline.SendRequest(request, cancellationToken);

            switch (response.Status)
            {
                case 200:
                    return CreateResponse(response, cancellationToken);
                case 409:
                case 412:
                    throw new ConflictException<T>(CreateResponse(response, cancellationToken));
                default:
                    throw new RequestFailedException(response.Status, response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Creates a PUT item (Replace) operation request.
        /// </summary>
        /// <param name="item">The item to replace</param>
        /// <returns>A <see cref="Request"/> object ready for transmission</returns>
        private Request CreateReplaceRequest(T item, MatchConditions requestOptions)
        {
            Request request = Pipeline.CreateRequest();

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
        private Task<Response<T>> CreateResponseAsync(Response response, CancellationToken cancellationToken)
            => CreateResponseAsync<T>(response, cancellationToken);

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <typeparam name="U">The type of the model</typeparam>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        private async Task<Response<U>> CreateResponseAsync<U>(Response response, CancellationToken cancellationToken) where U : class
        {
            U result = await JsonSerializer.DeserializeAsync<U>(response.ContentStream, ClientOptions.JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
            return Response.FromValue(result, response);
        }

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        private Response<T> CreateResponse(Response response, CancellationToken cancellationToken)
            => CreateResponse<T>(response, cancellationToken);

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <typeparam name="U">The type of the model</typeparam>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        private Response<U> CreateResponse<U>(Response response, CancellationToken cancellationToken) where U : class
        {
            var result = JsonSerializer.DeserializeAsync<U>(response.ContentStream, ClientOptions.JsonSerializerOptions, cancellationToken);
            return Response.FromValue(result.Result, response);
        }

        private ValueTask<PagedResult<T>> CreatePagedResultAsync(Response response, CancellationToken cancellationToken)
           => JsonSerializer.DeserializeAsync<PagedResult<T>>(response.ContentStream, ClientOptions.JsonSerializerOptions, cancellationToken);

        /// <summary>
        /// Deserializes the content of the response into a <see cref="PagedResult{T}"/> object.
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns></returns>
        private PagedResult<T> CreatePagedResult(Response response, CancellationToken cancellationToken)
            => JsonSerializer.DeserializeAsync<PagedResult<T>>(response.ContentStream, ClientOptions.JsonSerializerOptions, cancellationToken).Result;

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
