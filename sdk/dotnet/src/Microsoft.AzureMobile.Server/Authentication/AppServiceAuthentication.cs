using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureMobile.Server.Authentication
{
    /// <summary>
    /// Utility methods for working with App Service Authentication.
    /// </summary>
    public static class AppServiceAuthentication
    {
        private const string AppServiceAuthEnabled = "WEBSITE_AUTH_ENABLED";

        /// <summary>
        /// Returns true if App Service Authentication is enabled.
        /// </summary>
        /// <returns></returns>
        public static bool IsEnabled()
            => Environment.GetEnvironmentVariable(AppServiceAuthEnabled)?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}
