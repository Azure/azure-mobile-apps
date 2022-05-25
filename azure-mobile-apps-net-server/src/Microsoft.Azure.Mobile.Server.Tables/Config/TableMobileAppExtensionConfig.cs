// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// </summary>
    public class TableMobileAppExtensionConfig : IMobileAppExtensionConfigProvider
    {
        private ITableControllerConfigProvider provider;

        /// <summary>
        /// </summary>
        /// <param name="provider"></param>
        public TableMobileAppExtensionConfig(ITableControllerConfigProvider provider)
        {
            this.provider = provider;
        }

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        public void Initialize(HttpConfiguration config)
        {
            config.SetTableControllerConfigProvider(this.provider);
        }
    }
}
