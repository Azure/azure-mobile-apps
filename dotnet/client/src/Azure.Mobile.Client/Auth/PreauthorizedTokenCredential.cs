using Azure.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Mobile.Client.Auth
{
    /// <summary>
    /// A trivial implementation of the <see cref="TokenCredential"/> that allows the developer to
    /// update the AccessToken on their own schedule.  This is then included in the Authorization header
    /// on each request.
    /// </summary>
    public class PreauthorizedTokenCredential : TokenCredential
    {
        /// <summary>
        /// Create a new instance of a <see cref="PreauthorizedTokenCredential"/>
        /// without specifying an AccessToken immediately.
        /// </summary>
        public PreauthorizedTokenCredential()
        {
        }

        /// <summary>
        /// Create a new instance of a <see cref="PreauthorizedTokenCredential"/>
        /// and specify the initial value of the AccessToken.
        /// </summary>
        /// <param name="accessToken">Th initial value of the <see cref="AccessToken"/></param>
        public PreauthorizedTokenCredential(string accessToken)
        {
            AccessToken = accessToken;
        }

        /// <summary>
        /// The access token to provide back to the service.  This can be updated as needed.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// See <see cref="TokenCredential"/>
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            // Use this token for the next minute - then ask me again
            var expiresOn = DateTimeOffset.UtcNow.AddSeconds(60);
            return new AccessToken(AccessToken, expiresOn);
        }

        /// <summary>
        /// See <see cref="TokenCredential"/>
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var token = await Task.Run(() => GetToken(requestContext, cancellationToken)).ConfigureAwait(false);
            return token;
        }
    }
}
