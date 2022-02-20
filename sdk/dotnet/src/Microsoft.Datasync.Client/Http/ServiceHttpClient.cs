// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Authentication;
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
    /// pipeline policies and standardized headers.
    /// </summary>
    public class ServiceHttpClient : IDisposable
    {
        /// <summary>
        /// The protocol version this library implements.
        /// </summary>
        protected const string ProtocolVersion = "3.0.0";

        /// <summary>
        /// The root of the HTTP message handler pipeline.
        /// </summary>
        protected HttpMessageHandler roothandler;

        /// <summary>
        /// The <see cref="HttpClient"/> to use for communication.
        /// </summary>
        protected HttpClient client;

        /// <summary>
        /// A factory method for creating the default <see cref="HttpClientHandler"/>.
        /// </summary>
        protected Func<HttpMessageHandler> DefaultHandlerFactory = GetDefaultHttpClientHandler;

        /// <summary>
        /// Create a new <see cref="ServiceHttpClient"/> that communicates
        /// with the provided Datasync service endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint of the Datasync service.</param>
        /// <param name="clientOptions">The client options to use in configuring the HTTP client.</param>
        internal ServiceHttpClient(Uri endpoint, DatasyncClientOptions clientOptions) : this(endpoint, null, clientOptions)
        {
        }

        /// <summary>
        /// Create a new <see cref="ServiceHttpClient"/> that communicates
        /// with the provided Datasync service endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint of the Datasync service.</param>
        /// <param name="authenticationProvider">The authentication provider to use (if any)</param>
        /// <param name="clientOptions">The client options to use in configuring the HTTP client.</param>
        internal ServiceHttpClient(Uri endpoint, AuthenticationProvider authenticationProvider, DatasyncClientOptions clientOptions)
        {
            Arguments.IsValidEndpoint(endpoint, nameof(endpoint));
            Arguments.IsNotNull(clientOptions, nameof(clientOptions));

            Endpoint = endpoint;
            
            roothandler = CreatePipeline(clientOptions.HttpPipeline ?? Array.Empty<HttpMessageHandler>());
            if (authenticationProvider != null)
            {
                authenticationProvider.InnerHandler = roothandler;
                roothandler = authenticationProvider;
            }
            client = new HttpClient(roothandler) { BaseAddress = Endpoint };
            client.DefaultRequestHeaders.Add(ServiceHeaders.ProtocolVersion, ProtocolVersion);
            if (!string.IsNullOrWhiteSpace(clientOptions.UserAgent))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(ServiceHeaders.UserAgent, clientOptions.UserAgent);
                client.DefaultRequestHeaders.Add(ServiceHeaders.InternalUserAgent, clientOptions.UserAgent);
            }
            if (clientOptions.InstallationId == null || !string.IsNullOrWhiteSpace(clientOptions.InstallationId))
            {
                client.DefaultRequestHeaders.Add(ServiceHeaders.InstallationId, clientOptions.InstallationId ?? Platform.InstallationId);
            }
        }

        /// <summary>
        /// The endpoint of the Datasync service.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// Sends a request through the HTTP pipeline to the remote service asynchronously.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/> to send to the service.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns a <see cref="HttpResponseMessage"/> when complete.</returns>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(requestMessage, nameof(requestMessage));
            try
            {
                return await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Transform a list of <see cref="HttpMessageHandler"/> objects into a chain suitable for using
        /// as the pipeline of a <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="handlers">The list of <see cref="HttpMessageHandler"/> objects to transform</param>
        /// <returns>The chained <see cref="HttpMessageHandler"/></returns>
        protected HttpMessageHandler CreatePipeline(IEnumerable<HttpMessageHandler> handlers)
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

        #region IDisposable
        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> pattern for derived classes to use
        /// </summary>
        /// <param name="disposing">True if calling from <see cref="Dispose"/> or the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                roothandler?.Dispose();
                roothandler = null;

                client?.Dispose();
                client = null;
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
