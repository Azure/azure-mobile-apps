// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Mobile.Server.Authentication.AppService
{
    internal class AppServiceHttpClient : IDisposable
    {
        private const string XZumoAuthHeader = "x-zumo-auth";
        private const string AppServiceTokenAccessEndpointTemplate = "/.auth/me?provider={0}";
        private const string RuntimeUserAgent = "MobileAppNetServerSdk";

        private HttpClient client;
        private bool isDisposed;

        internal AppServiceHttpClient(HttpClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Calls the App Service Authentication module to retrieve the access token for the specified auth token and token provider name.
        /// </summary>
        /// <param name="webAppUri">The Web site from which to request tokens. For example: 'https://www.contoso.com'.</param>
        /// <param name="authToken">The auth token that App Service Authentication issued for the current user. Used for authentication and identification.</param>
        /// <param name="tokenProviderName">The token provider for which the associated access token will be retrieved. 'Facebook', 'Google', or 'Twitter', for example.</param>
        /// <returns>A <see cref="TokenEntry"/> with user details.</returns>
        internal virtual async Task<TokenEntry> GetRawTokenAsync(Uri webAppUri, string authToken, string tokenProviderName)
        {
            if (authToken == null)
            {
                throw new ArgumentNullException("authToken");
            }

            if (tokenProviderName == null)
            {
                throw new ArgumentNullException("tokenProviderName");
            }

            Uri requestUri = new Uri(webAppUri, AppServiceTokenAccessEndpointTemplate.FormatInvariant(tokenProviderName.ToLowerInvariant()));

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            AddHeaders(request, authToken);
            HttpResponseMessage response = await this.client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response);
            }

            if (response.Content == null)
            {
                return null;
            }

            // if the JSON returned is simply {}, return null
            JObject result = await response.Content.ReadAsAsync<JObject>();
            if (!result.HasValues)
            {
                return null;
            }

            return result.ToObject<TokenEntry>();
        }

        private static void AddHeaders(HttpRequestMessage request, string authToken)
        {
            request.Headers.Add(XZumoAuthHeader, authToken);
            request.Headers.UserAgent.ParseAdd(RuntimeUserAgent);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.isDisposed)
                {
                    this.isDisposed = true;
                    this.client.Dispose();
                }
            }
        }
    }
}