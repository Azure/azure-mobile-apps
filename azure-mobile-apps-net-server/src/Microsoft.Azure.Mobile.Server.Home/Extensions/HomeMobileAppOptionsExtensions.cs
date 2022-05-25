// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Mobile.Server.Help.Config;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public static class HomeMobileAppOptionsExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We only want this extension to apply to MobileAppConfiguration, not just any AppConfiguration")]
        public static MobileAppConfiguration AddMobileAppHomeController(this MobileAppConfiguration options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            options.RegisterConfigProvider(new HomeExtensionConfigProvider());
            return options;
        }
    }
}
