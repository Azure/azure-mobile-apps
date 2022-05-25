// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Dispatcher;
using Microsoft.Azure.Mobile.Server.Cache;
using Microsoft.Azure.Mobile.Server.Config;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        private const string MobileAppOptionsKey = "MS_MobileAppOptions";
        private const string MobileAppSettingsProviderKey = "MS_MobileAppSettingsProvider";
        private const string CachePolicyProviderKey = "MS_CachePolicyProvider";
        private const string MobileAppControllerConfigProviderKey = "MS_MobileAppControllerConfigProvider";

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration"/> registered with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <returns>The registered instance.</returns>
        public static MobileAppConfiguration GetMobileAppConfiguration(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            return config.Properties.GetValueOrDefault<MobileAppConfiguration>(MobileAppOptionsKey);
        }

        /// <summary>
        /// Registers a <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration"/> with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <param name="options">The instance to register.</param>
        public static void SetMobileAppConfiguration(this HttpConfiguration config, MobileAppConfiguration options)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[MobileAppOptionsKey] = options;
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppSettingsProvider"/> registered with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <returns>The registered instance.</returns>
        public static IMobileAppSettingsProvider GetMobileAppSettingsProvider(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IMobileAppSettingsProvider provider = null;
            if (!config.Properties.TryGetValue(MobileAppSettingsProviderKey, out provider))
            {
                provider = new MobileAppSettingsProvider();
                config.Properties[MobileAppSettingsProviderKey] = provider;
            }

            return provider;
        }

        /// <summary>
        /// Registers an <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppSettingsProvider"/> with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <param name="provider">The instance to register.</param>
        public static void SetMobileAppSettingsProvider(this HttpConfiguration config, IMobileAppSettingsProvider provider)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[MobileAppSettingsProviderKey] = provider;
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Mobile.Server.Cache.ICachePolicyProvider"/> registered with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <returns>The registered instance.</returns>
        public static ICachePolicyProvider GetCachePolicyProvider(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            ICachePolicyProvider provider = null;
            if (!config.Properties.TryGetValue(CachePolicyProviderKey, out provider))
            {
                provider = new CachePolicyProvider();
                config.Properties[CachePolicyProviderKey] = provider;
            }

            return provider;
        }

        /// <summary>
        /// Registers an <see cref="Microsoft.Azure.Mobile.Server.Cache.ICachePolicyProvider"/> with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <param name="provider">The instance to register.</param>
        public static void SetCachePolicyProvider(this HttpConfiguration config, ICachePolicyProvider provider)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[CachePolicyProviderKey] = provider;
        }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppControllerConfigProvider"/> registered with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <returns>The registered instance.</returns>
        public static IMobileAppControllerConfigProvider GetMobileAppControllerConfigProvider(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IMobileAppControllerConfigProvider provider = null;
            if (!config.Properties.TryGetValue(MobileAppControllerConfigProviderKey, out provider))
            {
                provider = new MobileAppControllerConfigProvider();
                config.Properties[MobileAppControllerConfigProviderKey] = provider;
            }

            return provider;
        }

        /// <summary>
        /// Registers an <see cref="Microsoft.Azure.Mobile.Server.Config.IMobileAppControllerConfigProvider"/> with the current <see cref="System.Web.Http.HttpConfiguration" />.
        /// The registered class provides the base settings for controllers using the <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppControllerAttribute" />.
        /// </summary>
        /// <param name="config">The current <see cref="System.Web.Http.HttpConfiguration"/>.</param>
        /// <param name="configProvider">The instance to register.</param>
        public static void SetMobileAppControllerConfigProvider(this HttpConfiguration config, IMobileAppControllerConfigProvider configProvider)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[MobileAppControllerConfigProviderKey] = configProvider;
        }

        internal static HashSet<string> GetMobileAppControllerNames(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IAssembliesResolver assemblyResolver = config.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllerTypeResolver = config.Services.GetHttpControllerTypeResolver();
            Type[] controllerTypes = controllerTypeResolver.GetControllerTypes(assemblyResolver).ToArray();

            // Add controllers that have the MobileAppController attribute
            IEnumerable<string> matches = controllerTypes
                .Where(t => t.GetCustomAttributes(typeof(MobileAppControllerAttribute), true).Any())
                .Select(t => t.Name.Substring(0, t.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length));

            HashSet<string> result = new HashSet<string>(matches, StringComparer.OrdinalIgnoreCase);
            return result;
        }
    }
}