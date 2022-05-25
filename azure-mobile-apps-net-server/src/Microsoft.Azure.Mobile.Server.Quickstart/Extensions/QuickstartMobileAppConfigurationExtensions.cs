// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using Microsoft.Azure.Mobile.Server.Tables.Config;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public static class QuickstartMobileAppConfigurationExtensions
    {
        public static MobileAppConfiguration UseDefaultConfiguration(this MobileAppConfiguration options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            return options.AddTables(
                    new MobileAppTableConfiguration()
                        .MapTableControllers()
                        .AddEntityFramework())
                .MapApiControllers()
                .AddMobileAppHomeController();
        }
    }
}