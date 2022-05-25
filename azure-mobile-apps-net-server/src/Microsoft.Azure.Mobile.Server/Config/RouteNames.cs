// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// This class defines the route names for known routes registered by the service. This can 
    /// for example be used to create links to the routes using the <see cref="T:System.Web.Http.UrlHelper"/> class
    /// which is available on a <see cref="System.Web.Http.ApiController"/>.
    /// </summary>
    internal static class RouteNames
    {
        /// <summary>
        /// Gets the route name of the Home Controller which provides the home page.
        /// </summary>
        public static readonly string Home = "Home";

        /// <summary>
        /// Gets the route name for all TableController instances.
        /// </summary>
        public static readonly string Tables = "Tables";

        /// <summary>
        /// Gets the route name for any custom <see cref="T:System.Web.Http.ApiController"/> instances.
        /// </summary>
        public static readonly string Apis = "DefaultApis";
    }
}
