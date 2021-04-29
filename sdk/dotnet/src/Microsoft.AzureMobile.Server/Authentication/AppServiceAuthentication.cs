// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.AzureMobile.Server.Authentication
{
    /// <summary>
    /// Utility methods for working with App Service Authentication.
    /// </summary>
    public static class AppServiceAuthentication
    {
        private const string AppServiceAuthEnabled = "WEBSITE_AUTH_ENABLED";

        /// <summary>
        /// The authentication scheme to use for enabling App Service Authentication
        /// </summary>
        public const string AuthenticationScheme = "AppService";

        /// <summary>
        /// The display name for the authentication scheme
        /// </summary>
        public const string DisplayName = "Azure App Service Authentication";

        /// <summary>
        /// Returns true if App Service Authentication is enabled.
        /// </summary>
        /// <returns></returns>
        public static bool IsEnabled()
            => Environment.GetEnvironmentVariable(AppServiceAuthEnabled)?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}
