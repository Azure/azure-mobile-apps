// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Mobile.Server.Authentication.Test
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            this.response = response;
        }

        public HttpRequestMessage ActualRequest { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.ActualRequest = request;
            return Task.FromResult(this.response);
        }
    }
}