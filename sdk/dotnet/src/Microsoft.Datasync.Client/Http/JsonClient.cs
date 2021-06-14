// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A <see cref="HttpClient"/> wrapper that sends and receives JSON
    /// payloads from a service.
    /// </summary>
    public class JsonClient
    {
        /// <summary>
        /// Creates a new <see cref="JsonClient"/> based on the endpoint provided and using
        /// default options.
        /// </summary>
        /// <param name="endpoint">The endpoint (which must be a valid endpoint)</param>
        public JsonClient(string endpoint)
            : this(endpoint, new DatasyncClientOptions())
        {
        }

        /// <summary>
        /// Creates a new <see cref="JsonClient"/> based on the endpoint provided and using
        /// default options.
        /// </summary>
        /// <param name="endpoint">The endpoint (which must be a valid endpoint)</param>
        public JsonClient(Uri endpoint)
            : this(endpoint, new DatasyncClientOptions())
        {
        }

        /// <summary>
        /// Creates a new <see cref="JsonClient"/> based on the endpoint provided and using
        /// the specified options.
        /// </summary>
        /// <param name="endpoint">The endpoint (which must be a valid endpoint)</param>
        /// <param name="options">The <see cref="DatasyncClientOptions"/> for this client</param>
        public JsonClient(string endpoint, DatasyncClientOptions options)
            : this(new Uri(endpoint, UriKind.Absolute), options)
        {
        }

        /// <summary>
        /// Creates a new <see cref="JsonClient"/> based on the endpoint provided and using
        /// the specified options.
        /// </summary>
        /// <param name="endpoint">The endpoint (which must be a valid endpoint)</param>
        /// <param name="options">The <see cref="DatasyncClientOptions"/> for this client</param>
        public JsonClient(Uri endpoint, DatasyncClientOptions options)
        {
            ValidateEndpoint(endpoint, nameof(endpoint));
            Endpoint = NormalizeEndpoint(endpoint);
            ClientOptions = options ?? throw new ArgumentNullException(nameof(options));

            // Create the HttpClient
            HttpMessageHandler = CreateHttpPipeline(ClientOptions.HttpPipeline);
            HttpClient = new HttpClient(HttpMessageHandler);
            AddDefaultRequestHeaders(HttpClient.DefaultRequestHeaders, ClientOptions);
        }

        /// <summary>
        /// The normalized endpoint this client is communicating with.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// The <see cref="DatasyncClientOptions"/> being used.
        /// </summary>
        public DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// The <see cref="HttpMessageHandler"/> that is the start of the HTTP pipeline.
        /// </summary>
        internal HttpMessageHandler HttpMessageHandler { get; }

        /// <summary>
        /// The <see cref="HttpClient"/> being used for communication.
        /// </summary>
        internal HttpClient HttpClient { get; }

        /// <summary>
        /// Adds the default header set for every request.
        /// </summary>
        /// <param name="headers">The <see cref="HttpRequestHeaders"/> for the client</param>
        /// <param name="options">The <see cref="DatasyncClientOptions"/> for the client</param>
        internal static void AddDefaultRequestHeaders(HttpRequestHeaders headers, DatasyncClientOptions options)
        {
            headers.Add(DatasyncClientHeaders.ProtocolVersion, options.ProtocolVersion);
        }

        /// <summary>
        /// Converts a list of <see cref="HttpMessageHandler"/> objects into a set of nested message handlers.
        /// Each handler must be a <see cref="DelegatingHandler"/>, except for the last one, which may optionally
        /// be a <see cref="HttpClientHandler"/>.  If the pipeline does not contain a <see cref="HttpClientHandler"/>,
        /// then a default one will be provided.
        /// </summary>
        /// <param name="messageHandlers">The list of message handlers</param>
        /// <returns>The root message handler for the pipeline.</returns>
        /// <exception cref="ArgumentException">One of the message handlers was expected to be a DelegatingHandler but wasn't.</exception>
        private HttpMessageHandler CreateHttpPipeline(IEnumerable<HttpMessageHandler> messageHandlers)
        {
            HttpMessageHandler pipeline = messageHandlers.LastOrDefault() ?? GetDefaultClientHandler();
            if (pipeline is DelegatingHandler dh && dh.InnerHandler == null)
            {
                dh.InnerHandler = GetDefaultClientHandler();
                pipeline = dh;
            }

            foreach (HttpMessageHandler messageHandler in messageHandlers.Reverse().Skip(1))
            {
                if (messageHandler is DelegatingHandler handler)
                {
                    handler.InnerHandler = pipeline;
                    pipeline = handler;
                }
                else
                {
                    throw new ArgumentException("All handlers in the HttpPipeline option except the last one must be a DelegatingHandler", nameof(messageHandlers));
                }
            }

            return pipeline;
        }

        /// <summary>
        /// Returns a default <see cref="HttpClientHandler"/> that supports automatic gzip decompression.
        /// </summary>
        /// <returns>A <see cref="HttpClientHandler"/></returns>
        private HttpMessageHandler GetDefaultClientHandler()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
            }
            return handler;
        }

        /// <summary>
        /// A normalized endpoint does not have any query parameters or fragments, and has a slash on the end
        /// so that it can be used with relative Uris with ease.
        /// </summary>
        /// <param name="endpoint">The endpoint to normalize</param>
        /// <returns>The normalized endpoint</returns>
        internal static Uri NormalizeEndpoint(Uri endpoint)
            => new UriBuilder(endpoint).WithQuery(string.Empty).WithFragment(string.Empty).WithTrailingSlash().Uri;

        /// <summary>
        /// Tests to see if the provided endpoint is valid.  If it isn't valid, an appropriate
        /// exception is thrown.
        /// </summary>
        /// <param name="endpoint">The endpoint name</param>
        /// <param name="paramName">The parameter name</param>
        /// <exception cref="ArgumentNullException">if <paramref name="endpoint"/> is null</exception>
        /// <exception cref="UriFormatException">if <paramref name="paramName"/> is a non-supported Uri format</exception>
        internal static void ValidateEndpoint(Uri endpoint, string paramName)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(paramName);
            }
            if (!endpoint.IsAbsoluteUri)
            {
                throw new UriFormatException($"{paramName} must be an absolute URI");
            }
            if (endpoint.Scheme == Uri.UriSchemeHttp && !endpoint.IsLoopback)
            {
                throw new UriFormatException($"{paramName} is insecure (and not localhost)");
            }
            if (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)
            {
                throw new UriFormatException($"{paramName} must use HTTP scheme");
            }
        }
    }
}
