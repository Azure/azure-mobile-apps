// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    /// <summary>
    /// A mock transport that allows you to build a response and send it
    /// in response to specific requests, recording the request along the
    /// way
    /// </summary>
    public class MockTransport
    {
        public MockTransport()
        {
            Handler = new MockHandler();
            Client = new HttpClient(Handler);
        }

        private MockHandler Handler { get; set; }

        /// <summary>
        /// The <see cref="HttpClient"/> to use for sending messages
        /// </summary>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// The last <see cref="HttpRequestMessage"/> that was sent (or null)
        /// </summary>
        public HttpRequestMessage Request { get => Handler.Request; }

        /// <summary>
        /// The response to send in response to the request
        /// </summary>
        public HttpResponseMessage Response
        {
            get => Handler.Response;
            set => Handler.Response = value;
        }

        /// <summary>
        /// The delegating handler that records the request.
        /// </summary>
        class MockHandler : DelegatingHandler
        {
            /// <summary>
            /// The last <see cref="HttpRequestMessage"/> that was received.
            /// </summary>
            public HttpRequestMessage Request { get; set; }

            /// <summary>
            /// The response to send for each request
            /// </summary>
            public HttpResponseMessage Response { get; set; }

            /// <summary>
            /// Part of the <see cref="DelegatingHandler"/> contract 
            /// </summary>
            /// <param name="request"></param>
            /// <param name="token"></param>
            /// <returns></returns>
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                Request = request;
                return Task.FromResult(Response);
            }
        }
    }
}
