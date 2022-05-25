// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.Help.Config
{
    public class HomeExtensionConfigProvider : IMobileAppExtensionConfigProvider
    {
        private const string HomeRouteName = "Home";

        public void Initialize(HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            HttpRouteCollectionExtensions.MapHttpRoute(
                config.Routes,
                name: HomeRouteName,
                routeTemplate: String.Empty,
                defaults: new { controller = "Home", action = "Index" });
        }
    }
}