using System;

namespace Todo.NetStandard.Common
{
    /// <summary>
    /// The configuration for the backend service
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Where you deployed your backend service
        /// </summary>
        public static Uri BackendService = new Uri("https://todoaspnetcoreserver20200803165803.azurewebsites.net");

        /// <summary>
        /// The Application Id (or Client Id) for your AAD application.
        /// </summary>
        public static string AadClientId = "some-client-id";
    }
}
