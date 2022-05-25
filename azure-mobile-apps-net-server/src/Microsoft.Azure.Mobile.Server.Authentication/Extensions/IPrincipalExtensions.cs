// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Authentication.AppService;
using Microsoft.Azure.Mobile.Server.Properties;

namespace System.Security.Principal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IPrincipalExtensions
    {
        private const string ObjectIdentifierClaimType = @"http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string TenantIdClaimType = @"http://schemas.microsoft.com/identity/claims/tenantid";
        private static HttpClient client = new HttpClient();

        /// <summary>
        /// Gets the identity provider specific identity details for the <see cref="IPrincipal"/> making the request.
        /// </summary>
        /// <param name="principal">The <see cref="IPrincipal"/> object.</param>
        /// <param name="request">The request context.</param>
        /// <typeparam name="T">The provider type.</typeparam>
        /// <returns>The identity provider credentials if found, otherwise null.</returns>
        public static Task<T> GetAppServiceIdentityAsync<T>(this IPrincipal principal, HttpRequestMessage request) where T : ProviderCredentials, new()
        {
            return principal.GetAppServiceIdentityAsync<T>(request, client);
        }

        public static Task<T> GetAppServiceIdentityAsync<T>(this IPrincipal principal, HttpRequestMessage request, HttpClient httpClient) where T : ProviderCredentials, new()
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            // Get the token from the request
            string zumoAuthToken = request.GetHeaderOrDefault("x-zumo-auth");
            return principal.GetAppServiceIdentityAsync<T>(zumoAuthToken, httpClient);
        }

        public static Task<T> GetAppServiceIdentityAsync<T>(this IPrincipal principal, string zumoAuthToken) where T : ProviderCredentials, new()
        {
            return principal.GetAppServiceIdentityAsync<T>(zumoAuthToken, client);
        }

        public static async Task<T> GetAppServiceIdentityAsync<T>(this IPrincipal principal, string zumoAuthToken, HttpClient httpClient) where T : ProviderCredentials, new()
        {
            ClaimsPrincipal user = principal as ClaimsPrincipal;
            if (user == null)
            {
                throw new ArgumentOutOfRangeException(RResources.ParameterMustBeOfType.FormatInvariant("principal", typeof(ClaimsPrincipal).Name), (Exception)null);
            }

            if (string.IsNullOrEmpty(zumoAuthToken))
            {
                return null;
            }

            // Base the url on the issuer of the JWT
            Claim issuerClaim = user.FindFirst(JwtRegisteredClaimNames.Iss);
            if (issuerClaim == null)
            {
                throw new ArgumentOutOfRangeException(RResources.GetIdentity_ClaimsMustHaveIssuer, (Exception)null);
            }

            string issuerUrl = issuerClaim.Value;
            ProviderCredentials credentials = (ProviderCredentials)new T();
            TokenEntry tokenEntry = null;
            AppServiceHttpClient appSvcClient = new AppServiceHttpClient(httpClient);

            try
            {
                tokenEntry = await appSvcClient.GetRawTokenAsync(new Uri(issuerUrl), zumoAuthToken, credentials.Provider);
            }
            catch (HttpResponseException ex)
            {
                throw new InvalidOperationException(RResources.GetIdentity_HttpError.FormatInvariant(ex.Response.ToString()));
            }

            if (!IsTokenValid(tokenEntry))
            {
                return null;
            }

            PopulateProviderCredentials(tokenEntry, credentials);

            return (T)credentials;
        }

        internal static bool IsTokenValid(TokenEntry tokenEntry)
        {
            if (tokenEntry == null)
            {
                return false;
            }

            return true;
        }

        internal static void PopulateProviderCredentials(TokenEntry tokenEntry, ProviderCredentials credentials)
        {
            if (tokenEntry.UserClaims != null)
            {
                Collection<Claim> userClaims = new Collection<Claim>();
                foreach (ClaimSlim claim in tokenEntry.UserClaims)
                {
                    userClaims.Add(new Claim(claim.Type, claim.Value));
                }
                credentials.UserClaims = userClaims;
            }

            FacebookCredentials facebookCredentials = credentials as FacebookCredentials;
            if (facebookCredentials != null)
            {
                facebookCredentials.AccessToken = tokenEntry.AccessToken;
                facebookCredentials.UserId = tokenEntry.UserId;
                return;
            }

            GoogleCredentials googleCredentials = credentials as GoogleCredentials;
            if (googleCredentials != null)
            {
                googleCredentials.AccessToken = tokenEntry.AccessToken;
                googleCredentials.RefreshToken = tokenEntry.RefreshToken;
                googleCredentials.UserId = tokenEntry.UserId;
                googleCredentials.AccessTokenExpiration = tokenEntry.ExpiresOn;

                return;
            }

            AzureActiveDirectoryCredentials aadCredentials = credentials as AzureActiveDirectoryCredentials;
            if (aadCredentials != null)
            {
                aadCredentials.AccessToken = tokenEntry.IdToken;
                Claim objectIdClaim = credentials.UserClaims.FirstOrDefault(c => string.Equals(c.Type, ObjectIdentifierClaimType, StringComparison.OrdinalIgnoreCase));
                if (objectIdClaim != null)
                {
                    aadCredentials.ObjectId = objectIdClaim.Value;
                }
                Claim tenantIdClaim = credentials.UserClaims.FirstOrDefault(c => string.Equals(c.Type, TenantIdClaimType, StringComparison.OrdinalIgnoreCase));
                if (tenantIdClaim != null)
                {
                    aadCredentials.TenantId = tenantIdClaim.Value;
                }
                aadCredentials.UserId = tokenEntry.UserId;
                return;
            }

            MicrosoftAccountCredentials microsoftAccountCredentials = credentials as MicrosoftAccountCredentials;
            if (microsoftAccountCredentials != null)
            {
                microsoftAccountCredentials.AccessToken = tokenEntry.AccessToken;
                microsoftAccountCredentials.RefreshToken = tokenEntry.RefreshToken;
                microsoftAccountCredentials.UserId = tokenEntry.UserId;
                microsoftAccountCredentials.AccessTokenExpiration = tokenEntry.ExpiresOn;

                return;
            }

            TwitterCredentials twitterCredentials = credentials as TwitterCredentials;
            if (twitterCredentials != null)
            {
                twitterCredentials.AccessToken = tokenEntry.AccessToken;
                twitterCredentials.AccessTokenSecret = tokenEntry.AccessTokenSecret;
                twitterCredentials.UserId = tokenEntry.UserId;

                return;
            }
        }
    }
}