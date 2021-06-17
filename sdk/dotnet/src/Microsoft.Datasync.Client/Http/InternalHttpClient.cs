// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using Microsoft.Datasync.Client.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// An implementation of the <see cref="HttpClient"/> that provides the
    /// HTTP Pipeline and default headers that the Datasync Framework requires.
    /// </summary>
    internal class InternalHttpClient : IDisposable
    {
        /// <summary>
        /// The protocol version that this client transmits.
        /// </summary>
        internal const string ProtocolVersion = "3.0.0";
        private bool disposedValue;

        /// <summary>
        /// Create a new <see cref="InternalHttpClient"/> object for communicating
        /// with a Datasync Framework service.
        /// </summary>
        /// <param name="endpoint">The base Uri for the requests.</param>
        /// <param name="options">The client options for the request.</param>
        internal InternalHttpClient(Uri endpoint, DatasyncClientOptions options)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));
            Validate.IsNotNull(options, nameof(options));

            Endpoint = new UriBuilder(endpoint).Normalized().Uri;

            MessageHandler = CreateHttpPipeline(options.HttpPipeline);
            HttpClient = new HttpClient(MessageHandler) { BaseAddress = Endpoint };
            HttpClient.DefaultRequestHeaders.Add(DatasyncHttpHeaders.ProtocolVersion, ProtocolVersion);
        }

        /// <summary>
        /// The base Uri for relative requests to this client.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// Reference to the root <see cref="HttpMessageHandler"/> for the client.
        /// </summary>
        protected HttpMessageHandler MessageHandler { get; }

        /// <summary>
        /// Reference to the <see cref="HttpClient"/> used for communication.
        /// </summary>
        protected HttpClient HttpClient { get; }

        /// <summary>
        /// Sends an HTTP request as an asynchronous operation.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="token">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">The request is null</exception>
        /// <exception cref="InvalidOperationException">The request message was already sent by the <see cref="HttpClient"/> instance.</exception>
        /// <exception cref="HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        /// <exception cref="RequestFailedException">The request failed due to a known HTTP status code that cannot be handled.</exception>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
        {
            var response = await HttpClient.SendAsync(request, token).ConfigureAwait(false);
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotModified:
                    throw new NotModifiedException(response);
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.MethodNotAllowed:
                case HttpStatusCode.NotAcceptable:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.RequestEntityTooLarge:
                case HttpStatusCode.RequestUriTooLong:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.Unauthorized:
                    throw new RequestFailedException(response);
                default:
                    return response;
            }
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
            if (pipeline is DelegatingHandler delegatingHandler && delegatingHandler.InnerHandler == null)
            {
                delegatingHandler.InnerHandler = GetDefaultClientHandler();
                pipeline = delegatingHandler;
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
                    throw new ArgumentException("All HttpMessageHandlers except the last must be a DelegatingHandler", nameof(messageHandler));
                }
            }

            return pipeline;
        }

        /// <summary>
        /// Obtain a reference to a <see cref="HttpClientHandler"/> that support automatic gzip decompression.
        /// </summary>
        /// <returns>A <see cref="HttpClientHandler"/></returns>
        private HttpMessageHandler GetDefaultClientHandler()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip;
            }
            return handler;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MessageHandler.Dispose();
                    HttpClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
