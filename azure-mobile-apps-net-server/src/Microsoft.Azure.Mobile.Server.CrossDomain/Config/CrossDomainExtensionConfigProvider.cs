// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.CrossDomain.Config
{
    public class CrossDomainExtensionConfigProvider : IMobileAppExtensionConfigProvider
    {
        private IEnumerable<string> domains;
        private const string CrossDomainControllerName = "crossdomain";

        public const string CrossDomainBridgeRouteName = "CrossDomain";
        public const string CrossDomainLoginReceiverRouteName = "CrossDomainLoginReceiver";

        public CrossDomainExtensionConfigProvider()
        {
        }

        public CrossDomainExtensionConfigProvider(IEnumerable<string> domains)
        {
            this.domains = domains;
        }

        public void Initialize(HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (this.domains != null)
            {
                config.SetCrossDomainOrigins(this.domains);
            }

            // register the controller as an exclusion so it does not map to /api
            MobileAppConfiguration mobileAppConfig = config.GetMobileAppConfiguration();
            mobileAppConfig.AddBaseRouteExclusion(CrossDomainControllerName);

            HttpRouteCollectionExtensions.MapHttpRoute(
                config.Routes,
                name: CrossDomainBridgeRouteName,
                routeTemplate: "crossdomain/bridge",
                defaults: new { controller = CrossDomainControllerName });

            HttpRouteCollectionExtensions.MapHttpRoute(
                config.Routes,
                name: CrossDomainLoginReceiverRouteName,
                routeTemplate: "crossdomain/loginreceiver",
                defaults: new { controller = CrossDomainControllerName });
        }
    }
}