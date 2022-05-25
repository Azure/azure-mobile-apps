// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Mobile.Server.CrossDomain.Config;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public static class MobileAppOptionsExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We only want this extension to apply to MobileAppConfiguration, not just any AppConfiguration")]
        public static MobileAppConfiguration MapLegacyCrossDomainController(this MobileAppConfiguration options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            options.RegisterConfigProvider(new CrossDomainExtensionConfigProvider());
            return options;
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We only want this extension to apply to MobileAppConfiguration, not just any AppConfiguration")]
        public static MobileAppConfiguration MapLegacyCrossDomainController(this MobileAppConfiguration options, IEnumerable<string> origins)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            options.RegisterConfigProvider(new CrossDomainExtensionConfigProvider(origins));
            return options;
        }
    }
}
