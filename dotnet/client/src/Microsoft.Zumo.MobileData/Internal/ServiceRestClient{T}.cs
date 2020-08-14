using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.MobileData.Internal
{
    /// <summary>
    /// Provides the underlying RESTful operations for a Table controller,
    /// without interpreting the results.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceRestClient<T> where T : TableData
    {
        public ServiceRestClient(Uri endpoint, TokenCredential credential, MobileTableClientOptions options)
        {
            Arguments.IsAbsoluteUri(endpoint, nameof(endpoint));
            Arguments.IsNotNull(options, nameof(options));

            Endpoint = endpoint;
            SerializerOptions = options.JsonSerializerOptions;

            var perCallPolicies = new List<HttpPipelinePolicy>();
            var perRetryPolicies = new List<HttpPipelinePolicy>();

            // Add the authentication policy only if supplied a credential.
            if (credential != null)
            {
                var scope = $"{endpoint.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped)}/.default";
                perRetryPolicies.Add(new BearerTokenAuthenticationPolicy(credential, scope));
            }

            // Builds the pipeline based on the policies provided.
            Pipeline = HttpPipelineBuilder.Build(options, perCallPolicies.ToArray(), perRetryPolicies.ToArray(), new ResponseClassifier());
        }

        /// <summary>
        /// The base <see cref="Uri"/> for the backend table controller.
        /// </summary>
        internal Uri Endpoint { get; }

        /// <summary>
        /// The <see cref="JsonSerializerOptions"/> for encoding the requests.
        /// </summary>
        internal JsonSerializerOptions SerializerOptions { get; }

        /// <summary>
        /// The Azure.Core Pipeline used for communicating with to the service
        /// </summary>
        internal HttpPipeline Pipeline { get; }

        /// <summary>
        /// Helper method to send the request to the remote service.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal Response SendRequest(Request request, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(request, nameof(request));
            return Pipeline.SendRequest(request, cancellationToken);
        }

        /// <summary>
        /// Helper method to send the request to the remote service.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal ValueTask<Response> SendRequestAsync(Request request, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(request, nameof(request));
            return Pipeline.SendRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Creates the request for a DELETE operation
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <param name="conditions">The <see cref="MatchConditions"/> for this request, if any</param>
        /// <returns></returns>
        internal Request CreateDeleteRequest(T item, MatchConditions conditions)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            Request request = Pipeline.CreateRequest();
            request.Method = RequestMethod.Delete;
            request.BuildUri(Endpoint, item.Id);
            if (item.Version != null)
            {
                request.ApplyConditionalHeaders(conditions ?? new MatchConditions { IfMatch = new ETag(item.Version) });
            }
            return request;
        }

        /// <summary>
        /// Creates a request for a GET ITEM operation
        /// </summary>
        /// <param name="id">The ID of the entity to retrieve</param>
        /// <param name="requestOptions">The <see cref="MatchConditions"/> for this request, if any</param>
        /// <returns>A pageable list of items</returns>
        internal Request CreateGetRequest(string id, MatchConditions requestOptions)
        {
            Arguments.IsNotNullOrEmpty(id, nameof(id));

            Request request = Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(Endpoint, id);
            request.ApplyConditionalHeaders(requestOptions);

            return request;
        }

        /// <summary>
        /// Creates a POST item (Create/Insert) operation request.
        /// </summary>
        /// <param name="item">The item to insert</param>
        /// <returns>A <see cref="Request"/> object ready for transmission</returns>
        internal Request CreateInsertRequest(T item)
        {
            Arguments.IsNotNull(item, nameof(item));

            Request request = Pipeline.CreateRequest();
            request.Method = RequestMethod.Post;
            request.BuildUri(Endpoint, null);
            request.ApplyConditionalHeaders(new MatchConditions { IfNoneMatch = ETag.All });
            request.Headers.Add(HttpHeader.Common.JsonContentType);
            request.Content = CreateRequestContent(item);

            return request;
        }

        /// <summary>
        /// Creates a request object for a paged response.
        /// </summary>
        /// <param name="options">The query to send</param>
        /// <param name="pageLink">The next page link, if available</param>
        /// <returns>The <see cref="Request"/> object corresponding to the request</returns>
        internal Request CreateListRequest(MobileTableQueryOptions options, string pageLink = null)
        {
            if (options == null && pageLink == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Request request = Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            var builder = request.Uri;
            if (pageLink != null)
            {
                builder.Reset(new Uri(pageLink));
                return request;
            }

            builder.Reset(Endpoint);
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
            if (options.Size >= 0)
            {
                builder.AppendQuery("$top", $"{options.Size}");
            }
            if (options.IncludeDeleted)
            {
                builder.AppendQuery("__includedeleted", "true");
            }
            builder.AppendQuery("$count", "true");
            return request;
        }

        /// <summary>
        /// Creates a PUT item (Replace) operation request.
        /// </summary>
        /// <param name="item">The item to replace</param>
        /// <returns>A <see cref="Request"/> object ready for transmission</returns>
        internal Request CreateReplaceRequest(T item, MatchConditions requestOptions)
        {
            Arguments.IsNotNull(item, nameof(item));
            Arguments.IsNotNullOrEmpty(item.Id, nameof(item.Id));

            Request request = Pipeline.CreateRequest();
            request.Method = RequestMethod.Put;
            request.BuildUri(Endpoint, item.Id);
            if (item.Version != null)
            {
                request.ApplyConditionalHeaders(requestOptions ?? new MatchConditions { IfMatch = new ETag(item.Version) });
            }
            request.Headers.Add(HttpHeader.Common.JsonContentType);
            request.Content = CreateRequestContent(item);

            return request;
        }

        /// <summary>
        /// Serializes an item into it's JSON form, ready for transmission.
        /// </summary>
        /// <param name="item">The item to serialize.</param>
        /// <returns>A set of UTF-8 bytes to transmit.</returns>
        private RequestContent CreateRequestContent(T item)
            => RequestContent.Create(JsonSerializer.SerializeToUtf8Bytes<T>(item, SerializerOptions));

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        internal Task<Response<T>> CreateResponseAsync(Response response, CancellationToken cancellationToken)
            => CreateResponseAsync<T>(response, cancellationToken);

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <typeparam name="U">The type of the model</typeparam>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        internal async Task<Response<U>> CreateResponseAsync<U>(Response response, CancellationToken cancellationToken) where U : class
        {
            U result = await JsonSerializer.DeserializeAsync<U>(response.ContentStream, SerializerOptions, cancellationToken).ConfigureAwait(false);
            return Response.FromValue(result, response);
        }

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        internal Response<T> CreateResponse(Response response, CancellationToken cancellationToken)
            => CreateResponse<T>(response, cancellationToken);

        /// <summary>
        /// Creates a typed response from an untyped response with an IO Stream
        /// </summary>
        /// <typeparam name="U">The type of the model</typeparam>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns>The typed response</returns>
        internal Response<U> CreateResponse<U>(Response response, CancellationToken cancellationToken) where U : class
        {
            var result = JsonSerializer.DeserializeAsync<U>(response.ContentStream, SerializerOptions, cancellationToken);
            return Response.FromValue(result.Result, response);
        }

        /// <summary>
        /// Deserializes the content of the response into a <see cref="PagedResult{T}"/> object.
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns></returns>
        internal ValueTask<PagedResult<T>> CreatePagedResultAsync(Response response, CancellationToken cancellationToken)
            => JsonSerializer.DeserializeAsync<PagedResult<T>>(response.ContentStream, SerializerOptions, cancellationToken);

        /// <summary>
        /// Deserializes the content of the response into a <see cref="PagedResult{T}"/> object.
        /// </summary>
        /// <param name="response">The response to process</param>
        /// <param name="cancellationToken">A lifecycle cancellation token</param>
        /// <returns></returns>
        internal PagedResult<T> CreatePagedResult(Response response, CancellationToken cancellationToken)
            => JsonSerializer.DeserializeAsync<PagedResult<T>>(response.ContentStream, SerializerOptions, cancellationToken).Result;
    }
}
