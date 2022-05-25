// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Xamarin.Essentials;

namespace Microsoft.WindowsAzure.MobileServices.Internal
{
    /// <summary>
    /// Authenticator that uses Xamarin.Essentials WebAuthenticator to do the work of
    /// handling Azure App Service Authentication.
    /// </summary>
    internal class MobileServiceUIAuthentication : MobileServicePKCEAuthentication
    {
        public MobileServiceUIAuthentication(
            MobileServiceClient client, 
            string providerName, 
            string uriScheme, 
            IDictionary<string, string> parameters
        ) : base(client, providerName, uriScheme, parameters)
        {
        }

        /// <summary>
        /// Obtain the authorization code from the App Service Authentication.
        /// </summary>
        /// <returns>The authorization code</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the authentication fails.
        /// </exception>
        public override async Task<string> GetAuthorizationCodeAsync()
        {
            try
            {
                var result = await WebAuthenticator.AuthenticateAsync(LoginUri, CallbackUri).ConfigureAwait(false);
                if (!result.Properties.TryGetValue("authorization_code", out string authorization_code))
                {
                    throw new InvalidOperationException("Authentication failed: authorization_code not returned");
                }
                return Uri.UnescapeDataString(authorization_code);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Authorization failed: {ex.Message}", ex);
            }
        }
    }
}
