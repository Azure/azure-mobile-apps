// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// The options for the Azure App Service authentication handler.
    /// </summary>
    public class AzureAppServiceAuthenticationOptions : AuthenticationSchemeOptions
    {
        public AzureAppServiceAuthenticationOptions()
        {
            Events = new object();
        }

        /// <summary>
        /// If set to true, force-enable the authentication handler.  If set to falce,
        /// the authentication handler is enabled only if the <c>WEBSITE_AUTH_ENABLED</c>
        /// environment variable is set to true.
        /// </summary>
        public bool ForceEnable { get; set; }
    }
}
