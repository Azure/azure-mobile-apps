using Azure.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    /// <summary>
    /// Test token provider that implements a simple access token that can be specified.
    /// </summary>
    public class TestTokenCredential : TokenCredential
    {
        public TestTokenCredential() { }

        public TestTokenCredential(string accessToken)
        {
            AccessToken = accessToken;
        }

        public string AccessToken { get; set; }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var expiresOn = DateTimeOffset.UtcNow.AddSeconds(60);
            return new AccessToken(AccessToken, expiresOn);
        }

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = await Task.Run(() => GetToken(requestContext, cancellationToken)).ConfigureAwait(false);
            return token;
        }
    }
}
