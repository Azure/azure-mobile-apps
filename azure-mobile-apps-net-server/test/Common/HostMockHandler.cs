// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Microsoft.Azure.Mobile.Mocks
{
    /// <summary>
    /// <see cref="DelegatingHandler"/> which inserts OWIN specific properties in the request path.
    /// </summary>
    public class HostMockHandler : DelegatingHandler
    {
        public HostMockHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetOwinContext(new OwinContext());
            return base.SendAsync(request, cancellationToken);
        }
    }
}
