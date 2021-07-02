// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// An internal version of the <see cref="HttpClient"/> class that provides
    /// pipeline policies, and standardized headers.
    /// </summary>
    internal class InternalHttpClient : IDisposable
    {
        /// <summary>
        /// The protocol version this library implements.
        /// </summary>
        protected const string ProtocolVersion = "3.0.0";

        /// <summary>
        /// The URI for the datasync service.
        /// </summary>
        protected readonly Uri applicationUri;

        /// <summary>
        /// The installation ID of the application.
        /// </summary>
        protected readonly string installationId;

        /// <summary>
        /// The value of the <c>User-Agent</c>  header
        /// </summary>
        protected readonly string userAgentHeaderValue;

        /// <summary>
        /// The top of the HTTP pipeline policy tree.  All requests and responses
        /// will go through this handler.
        /// </summary>
        protected HttpMessageHandler httpHandler;

        /// <summary>
        /// The <see cref="HttpClient"/> that handles communication.
        /// </summary>
        protected HttpClient httpClient;

        /// <summary>
        /// A factory method for creating the default <see cref="HttpClientHandler"/>.
        /// </summary>
        protected static Func<HttpMessageHandler> DefaultHandlerFactory = GetDefaultHttpClientHandler;

        /// <summary>
        /// Instantiates a new <see cref="InternalHttpClient"/> which performs all the requests to a datasync service.
        /// </summary>
        /// <param name="endpoint">The endpoint to communicate with</param>
        /// <param name="clientOptions">The client options for the connection</param>
        public InternalHttpClient(Uri endpoint, DatasyncClientOptions clientOptions)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));
            Validate.IsNotNull(clientOptions, nameof(clientOptions));

            applicationUri = endpoint;
            installationId = clientOptions.InstallationId;
            userAgentHeaderValue = clientOptions.UserAgent;

            httpHandler = CreatePipeline(clientOptions.HttpPipeline ?? Array.Empty<HttpMessageHandler>());
            httpClient = new HttpClient(httpHandler) { BaseAddress = applicationUri };
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation(InternalHttpHeaders.UserAgent, userAgentHeaderValue);
            httpClient.DefaultRequestHeaders.Add(InternalHttpHeaders.InternalUserAgent, userAgentHeaderValue);
            httpClient.DefaultRequestHeaders.Add(InternalHttpHeaders.ProtocolVersion, ProtocolVersion);
            httpClient.DefaultRequestHeaders.Add(InternalHttpHeaders.InstallationId, installationId);
        }

        /// <summary>
        /// Transform a list of <see cref="HttpMessageHandler"/> objects into a chain suitable for using
        /// as the pipeline of a <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="handlers">The list of <see cref="HttpMessageHandler"/> objects to transform</param>
        /// <returns>The chained <see cref="HttpMessageHandler"/></returns>
        protected static HttpMessageHandler CreatePipeline(IEnumerable<HttpMessageHandler> handlers)
        {
            HttpMessageHandler pipeline = handlers.LastOrDefault() ?? DefaultHandlerFactory();
            if (pipeline is DelegatingHandler lastPolicy && lastPolicy.InnerHandler == null)
            {
                lastPolicy.InnerHandler = DefaultHandlerFactory();
                pipeline = lastPolicy;
            }

            // Wire handlers up in reverse order
            foreach (HttpMessageHandler handler in handlers.Reverse().Skip(1))
            {
                if (handler is DelegatingHandler policy)
                {
                    policy.InnerHandler = pipeline;
                    pipeline = policy;
                }
                else
                {
                    throw new ArgumentException("All message handlers except the last one must be 'DelegatingHandler'", nameof(handlers));
                }
            }
            return pipeline;
        }

        /// <summary>
        /// Returns a <see cref="HttpClientHandler"/> that supports automatic decompression.
        /// </summary>
        protected static HttpMessageHandler GetDefaultHttpClientHandler()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
            }
            return handler;
        }

        /// <summary>
        /// Sends the <paramref name="request"/> to the remote datasync service.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="token">A <see cref="CancellationToken"/> for the request</param>
        /// <returns>The response from the server</returns>
        protected virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
        {
            Validate.IsNotNull(request, nameof(request));

            try
            {
                var response = await httpClient.SendAsync(request, token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw new DatasyncOperationException(request, response);
                }
                return response;
            }
            catch (OperationCanceledException) when (!token.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }

        #region IDisposable
        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> pattern for derived classes to use
        /// </summary>
        /// <param name="disposing">True if calling from <see cref="Dispose"/> or the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (httpHandler != null)
                {
                    httpHandler.Dispose();
                    httpHandler = null;
                }

                if (httpClient != null)
                {
                    httpClient.Dispose();
                    httpClient = null;
                }
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
