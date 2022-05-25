// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class MobileServiceUIAuthentication : MobileServicePKCEAuthentication
    {
        internal static AuthenticationHelper CurrentAuthenticator;

        /// <summary>
        /// Instantiates a new instance of <see cref="MobileServiceUIAuthentication"/>.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// The authentication provider.
        /// </param>
        /// <param name="uriScheme">
        /// The uri scheme.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        public MobileServiceUIAuthentication(MobileServiceClient client, string provider, string uriScheme, IDictionary<string, string> parameters)
            : base(client, provider, uriScheme, parameters)
        {
        }

        /// <summary>
        /// Provides login UI to start authentication process.
        /// </summary>
        /// <returns>
        /// Task that will complete with the authorization code when the user finishes authentication.
        /// </returns>
        public override Task<string> GetAuthorizationCodeAsync()
        {
            var tcs = new TaskCompletionSource<string>();

            if (CurrentAuthenticator != null)
            {
                tcs.TrySetException(new InvalidOperationException("Authentication is already in progress."));
                return tcs.Task;
            }

            CurrentAuthenticator = new AuthenticationHelper();

            CurrentAuthenticator.Completed += (o, e) =>
            {
                if (!e.IsAuthenticated)
                {
                    tcs.TrySetException(new InvalidOperationException("Authentication was cancelled by the user."));
                }
                else
                {
                    tcs.TrySetResult(e.AuthorizationCode);
                }
                CurrentAuthenticator = null;
            };

            CurrentAuthenticator.Error += (o, e) =>
            {
                tcs.TrySetException(new InvalidOperationException(e.Message));
                CurrentAuthenticator = null;
            };

            var browserLaunched = Windows.System.Launcher.LaunchUriAsync(LoginUri);
            return tcs.Task;
        }
    }
}