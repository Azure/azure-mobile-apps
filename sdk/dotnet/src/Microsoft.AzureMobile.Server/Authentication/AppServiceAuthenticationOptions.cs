// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AzureMobile.Server.Authentication
{
    /// <summary>
    /// The options for the App Service Authentication Handler.
    /// </summary>
    public class AppServiceAuthenticationOptions : AuthenticationSchemeOptions
    {
        public AppServiceAuthenticationOptions()
        {
            Events = new object();
        }

        /// <summary>
        /// If set to true, force-enable the authentication handler.  If set to false,
        /// the authentication handler is enabled only if <c>WEBSITE_AUTH_ENABLED</c>
        /// environment variable is set to true.
        /// </summary>
        public bool ForceEnable { get; set; }
    }
}
