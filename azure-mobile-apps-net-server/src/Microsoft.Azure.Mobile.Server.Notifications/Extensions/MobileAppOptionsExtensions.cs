// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Mobile.Server.Notifications.Config;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public static class MobileAppOptionsExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We only want this extension to apply to MobileAppConfiguration, not just any AppConfiguration")]
        [Obsolete("Use App Service Push instead.", error: false)]
        public static MobileAppConfiguration AddPushNotifications(this MobileAppConfiguration options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            options.RegisterConfigProvider(new NotificationsExtensionConfigProvider());
            return options;
        }
    }
}
