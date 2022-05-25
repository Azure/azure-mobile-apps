// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Internal
{
    /// <summary>
    /// Provides an OAuth type token authentication scheme.
    /// </summary>
    public class MobileServiceTokenAuthentication : MobileServiceAuthentication
    {
        /// <summary>
        /// The token to send.
        /// </summary>
        private readonly JObject token;

        /// <summary>
        /// The <see cref="MobileServiceClient"/> used by this authentication session.
        /// </summary>
        private readonly MobileServiceClient client;

        /// <summary>
        /// Instantiates a new instance of <see cref="MobileServiceTokenAuthentication"/>.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// The authentication provider.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        public MobileServiceTokenAuthentication(MobileServiceClient client, string provider, JObject token, IDictionary<string, string> parameters)
            : base(client, provider, parameters)
        {
            Arguments.IsNotNull(client, nameof(client));
            Arguments.IsNotNull(token, nameof(token));

            this.client = client;
            this.token = token;
        }

        /// <summary>
        /// Provides Login logic for an existing token.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        public override Task<string> LoginAsyncOverride()
        {
            string path = string.IsNullOrEmpty(client.LoginUriPrefix)
                ? MobileServiceUrlBuilder.CombinePaths(LoginAsyncUriFragment, ProviderName)
                : MobileServiceUrlBuilder.CombinePaths(client.LoginUriPrefix, ProviderName);
            string queryString = MobileServiceUrlBuilder.GetQueryString(Parameters);
            string pathAndQuery = MobileServiceUrlBuilder.CombinePathAndQuery(path, queryString);

            return client.AlternateLoginHost != null
                ? client.AlternateAuthHttpClient.RequestWithoutHandlersAsync(HttpMethod.Post, pathAndQuery, client.CurrentUser, token.ToString())
                : client.HttpClient.RequestWithoutHandlersAsync(HttpMethod.Post, pathAndQuery, client.CurrentUser, token.ToString());
        }
    }
}
