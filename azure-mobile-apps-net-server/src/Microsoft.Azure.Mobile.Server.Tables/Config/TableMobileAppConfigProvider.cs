// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Tables.Config;

namespace Microsoft.Azure.Mobile.Server.Config
{
    internal class TableMobileAppConfigProvider : IMobileAppExtensionConfigProvider
    {
        private MobileAppTableConfiguration mobileAppTableConfig;

        public TableMobileAppConfigProvider(MobileAppTableConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.mobileAppTableConfig = config;
        }

        public void Initialize(HttpConfiguration config)
        {
            this.mobileAppTableConfig.ApplyTo(config);
        }
    }
}