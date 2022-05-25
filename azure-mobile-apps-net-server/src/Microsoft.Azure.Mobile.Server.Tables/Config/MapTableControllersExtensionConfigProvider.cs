// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.Tables.Config
{
    /// <summary>
    /// </summary>
    public class MapTableControllersExtensionConfigProvider : IMobileAppExtensionConfigProvider
    {
        private const string TablesRouteName = "Tables";

        /// <summary>
        /// </summary>
        /// <param name="config">HttpConfiguration</param>
        public void Initialize(HttpConfiguration config)
        {
            HashSet<string> tableControllerNames = config.GetTableControllerNames();
            SetRouteConstraint<string> tableControllerConstraint = new SetRouteConstraint<string>(tableControllerNames, matchOnExcluded: false);

            // register all TableControllers as exclusions so they do not map to /api
            MobileAppConfiguration mobileAppConfig = config.GetMobileAppConfiguration();
            foreach (string controllerName in tableControllerNames)
            {
                mobileAppConfig.AddBaseRouteExclusion(controllerName);
            }

            HttpRouteCollectionExtensions.MapHttpRoute(
                config.Routes,
                name: TablesRouteName,
                routeTemplate: "tables/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { controller = tableControllerConstraint });
        }
    }
}