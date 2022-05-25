// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.Tables.Config
{
    /// <summary>
    /// Provides configuration methods for Mobile App controllers that derive from <see cref="TableController"/>.
    /// </summary>
    public class MobileAppTableConfiguration : AppConfiguration
    {
        /// <summary>
        /// Maps all controllers that derive from <see cref="TableController" /> to the
        /// route template "tables/{controller}/{id}".
        /// </summary>
        /// <returns>The current <see cref="MobileAppTableConfiguration"/>.</returns>
        public MobileAppTableConfiguration MapTableControllers()
        {
            this.RegisterConfigProvider(new MapTableControllersExtensionConfigProvider());
            return this;
        }
    }
}