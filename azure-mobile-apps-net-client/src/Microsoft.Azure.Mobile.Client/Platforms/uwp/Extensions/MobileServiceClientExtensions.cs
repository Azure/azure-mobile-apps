// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Extension methods for UI-based login.
    /// </summary>
    public static partial class MobileServiceClientExtensions
    {
        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The uri scheme.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, MobileServiceAuthenticationProvider provider, string uriScheme)
        {
            return LoginAsync(client, provider, uriScheme, parameters: null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The uri scheme.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, MobileServiceAuthenticationProvider provider, string uriScheme, IDictionary<string, string> parameters)
        {
            return LoginAsync(client, provider.ToString(), uriScheme, parameters);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The uri scheme.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, string provider, string uriScheme)
        {
            return LoginAsync(client, provider, uriScheme, null);
        }

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="provider">
        /// Authentication provider to use.
        /// </param>
        /// <param name="uriScheme">
        /// The uri scheme.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, string provider, string uriScheme, IDictionary<string, string> parameters)
        {
            MobileServiceUIAuthentication auth = new MobileServiceUIAuthentication(client, provider, uriScheme, parameters);
            return auth.LoginAsync();
        }

        /// <summary>
        /// Resume login process with the specified URL.
        /// </summary>
        public static void ResumeWithURL(this MobileServiceClient client, Uri uri)
        {
            MobileServiceUIAuthentication.CurrentAuthenticator?.OnResponseReceived(uri);
        }

        /// <summary>
        /// Extension method to get a <see cref="Push"/> object made from an existing <see cref="MobileServiceClient"/>.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> to create with.
        /// </param>
        /// <returns>
        /// The <see cref="Push"/> object used for registering for notifications.
        /// </returns>
        public static Push GetPush(this IMobileServiceClient client)
        {
            return new Push(client);
        }
    }
}
