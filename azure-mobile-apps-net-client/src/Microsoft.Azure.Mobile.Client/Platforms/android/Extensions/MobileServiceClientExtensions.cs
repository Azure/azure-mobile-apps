// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Microsoft.WindowsAzure.MobileServices.Internal;

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
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="context">The Android Context to display the Login UI in.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <returns>The user record for the authenticated user.</returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, Context context, MobileServiceAuthenticationProvider provider, string uriScheme)
            => LoginAsync(client, context, provider, uriScheme, parameters: null);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="context">The Android Context to display the Login UI in.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        /// <returns>The user record for the authenticated user.</returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, Context context, MobileServiceAuthenticationProvider provider, string uriScheme, IDictionary<string, string> parameters)
            => LoginAsync(client, context, provider.ToString(), uriScheme, parameters);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="context">The Android Context to display the Login UI in.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <returns>The user record for the authenticated user.</returns>
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, Context context, string provider, string uriScheme)
            => LoginAsync(client, context, provider, uriScheme, parameters: null);

        /// <summary>
        /// Log a user into a Mobile Services application given a provider name.
        /// </summary>
        /// <param name="client">The MobileServiceClient instance to login with</param>
        /// <param name="context">The Android Context to display the Login UI in.</param>
        /// <param name="provider">Authentication provider to use.</param>
        /// <param name="uriScheme">The URL scheme of the application.</param>
        /// <param name="parameters">Provider specific extra parameters that are sent as query string parameters to login endpoint.</param>
        /// <returns>The user record for the authenticated user.</returns>
#pragma warning disable IDE0060 // Remove unused parameter
        public static Task<MobileServiceUser> LoginAsync(this MobileServiceClient client, Context context, string provider, string uriScheme, IDictionary<string, string> parameters)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var auth = new MobileServiceUIAuthentication(client, provider, uriScheme, parameters);
            return auth.LoginAsync();
        }

        /// <summary>
        /// Extension method to get a <see cref="Push"/> object made from an existing 
        /// <see cref="MobileServiceClient"/>.
        /// </summary>
        /// <param name="client">The <see cref="MobileServiceClient"/> to create with.</param>
        /// <returns>The <see cref="Push"/> object used for registering for notifications.</returns>
        public static Push GetPush(this IMobileServiceClient client) => new Push(client);
    }
}
