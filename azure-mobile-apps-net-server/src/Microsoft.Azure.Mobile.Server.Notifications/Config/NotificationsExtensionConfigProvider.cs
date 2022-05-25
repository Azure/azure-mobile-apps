// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.Notifications.Config
{
    public class NotificationsExtensionConfigProvider : IMobileAppExtensionConfigProvider
    {
        private const string PushRoutePrefix = "push/installations/";
        private const string NotificationInstallationsRouteName = "NotificationInstallations";
        private const string PushControllerName = "notificationinstallations";

        public void Initialize(HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            // register the controller as an exclusion so it does not map to /api
            MobileAppConfiguration mobileAppConfig = config.GetMobileAppConfiguration();
            mobileAppConfig.AddBaseRouteExclusion(PushControllerName);

            HttpRouteCollectionExtensions.MapHttpRoute(
                config.Routes,
                name: NotificationInstallationsRouteName,
                routeTemplate: PushRoutePrefix + "{installationId}",
                defaults: new { controller = PushControllerName });
        }
    }
}