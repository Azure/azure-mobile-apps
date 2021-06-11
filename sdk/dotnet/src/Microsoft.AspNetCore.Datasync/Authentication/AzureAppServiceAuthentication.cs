// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Utility methods and constants for working with Azure App Service Authentication.
    /// </summary>
    public static class AzureAppServiceAuthentication
    {
        private const string AppServiceAuthEnabled = "WEBSITE_AUTH_ENABLED";

        /// <summary>
        /// The authentication scheme name to use for setting the default scheme.
        /// </summary>
        public const string AuthenticationScheme = "AzureAppService";

        /// <summary>
        /// The display name for the authentication scheme.
        /// </summary>
        public const string DisplayName = "Azure App Service Authentication";

        /// <summary>
        /// Determines if Azure App Service Authentication is enabled.
        /// </summary>
        /// <returns>true if Azure App Service Authentication is available and enabled.</returns>
        public static bool IsEnabled()
            => Environment.GetEnvironmentVariable(AppServiceAuthEnabled)?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}
