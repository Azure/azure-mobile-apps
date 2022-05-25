// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;

namespace System.Web.Http
{
    public static class HttpConfigurationExtensions
    {
        private const string CrossDomainOriginsKey = "MS_CrossDomainOrigins";

        public static IEnumerable<string> GetCrossDomainOrigins(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            return config.Properties.GetValueOrDefault<IEnumerable<string>>(CrossDomainOriginsKey);
        }

        public static void SetCrossDomainOrigins(this HttpConfiguration config, IEnumerable<string> crossDomainOrigins)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[CrossDomainOriginsKey] = crossDomainOrigins;
        }
    }
}
