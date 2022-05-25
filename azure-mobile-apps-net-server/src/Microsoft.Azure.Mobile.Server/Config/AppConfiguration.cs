// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// A default abstract implementation of <see cref="Microsoft.Azure.Mobile.Server.Config.IAppConfiguration" />.
    /// </summary>
    public abstract class AppConfiguration : IAppConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Microsoft.Azure.Mobile.Server.Config.AppConfiguration" /> class.
        /// </summary>
        protected AppConfiguration()
        {
            this.ConfigProviders = new Dictionary<Type, IMobileAppExtensionConfigProvider>();
        }

        /// <summary>
        /// Gets a list of registered <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppExtensionConfigProvider" />.
        /// </summary>
        protected IDictionary<Type, IMobileAppExtensionConfigProvider> ConfigProviders { get; private set; }

        /// <inheritdoc />
        public virtual void RegisterConfigProvider(IMobileAppExtensionConfigProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            Type providerType = provider.GetType();
            if (this.ConfigProviders.ContainsKey(providerType))
            {
                throw new ArgumentException(RResources.ExtensionProvider_AlreadyExists.FormatInvariant(providerType));
            }

            this.ConfigProviders.Add(providerType, provider);
        }

        /// <inheritdoc />
        public virtual void ApplyTo(HttpConfiguration config)
        {
            foreach (IMobileAppExtensionConfigProvider provider in this.ConfigProviders.Values)
            {
                provider.Initialize(config);
            }
        }
    }
}