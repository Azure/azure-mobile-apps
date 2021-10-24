using Microsoft.Datasync.Client.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// A test authentication provider that just takes an AuthenticationToken and
    /// adds it to the request.
    /// </summary>
    public class TestAuthenticationProvider : AuthenticationProvider
    {
        public TestAuthenticationProvider(AuthenticationToken token)
        {
            Token = token;
        }

        internal AuthenticationToken Token { get; set; }

        internal string HeaderName { get; set; } = "X-ZUMO-AUTH";

        public override Task LoginAsync() => Task.CompletedTask;

        /// <summary>
        /// The delegating handler for this request - injects the authorization header into the request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>The response (asynchronously)</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request.Headers.Contains(HeaderName))
            {
                request.Headers.Remove(HeaderName);
            }
            request.Headers.Add(HeaderName, Token.Token);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
