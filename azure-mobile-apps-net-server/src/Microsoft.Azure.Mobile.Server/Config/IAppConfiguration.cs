// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// Provides a mechanism for modifying the <see cref="HttpConfiguration"/> of a Mobile App.
    /// </summary>
    public interface IAppConfiguration
    {
        /// <summary>
        /// Adds an <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppExtensionConfigProvider" /> to the list of providers that will be
        /// called by the <see cref="Microsoft.Azure.Mobile.Server.Config.AppConfiguration.ApplyTo" /> method.
        /// </summary>
        /// <param name="provider">The <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppExtensionConfigProvider" /> to register.</param>
        void RegisterConfigProvider(IMobileAppExtensionConfigProvider provider);

        /// <summary>
        /// Calls <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppExtensionConfigProvider.Initialize" /> on every registered
        /// <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppExtensionConfigProvider" />.
        /// </summary>
        /// <param name="config">The <see cref="HttpConfiguration"/> passed to each invocation of
        /// <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppExtensionConfigProvider.Initialize" />.</param>
        void ApplyTo(HttpConfiguration config);
    }
}