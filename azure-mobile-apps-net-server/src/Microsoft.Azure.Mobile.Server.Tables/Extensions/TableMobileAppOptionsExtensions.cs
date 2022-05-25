// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Azure.Mobile.Server.Tables.Config;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// Extension methods for <see cref="MobileAppConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TableMobileAppOptionsExtensions
    {
        /// <summary>
        /// Registers the specified <see cref="ITableControllerConfigProvider" /> with the <see cref="System.Web.Http.HttpConfiguration"/>.
        /// Use this to override the default <see cref="TableController"/> configuration.
        /// </summary>
        /// <param name="options">The configuration object.</param>
        /// <param name="tableConfigProvider">The provider to register.</param>
        /// <returns>The current <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We only want this extension to apply to MobileAppConfiguration, not just any AppConfiguration")]
        public static MobileAppConfiguration WithTableControllerConfigProvider(this MobileAppConfiguration options, ITableControllerConfigProvider tableConfigProvider)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (tableConfigProvider == null)
            {
                throw new ArgumentNullException("tableConfigProvider");
            }

            options.RegisterConfigProvider(new TableMobileAppExtensionConfig(tableConfigProvider));
            return options;
        }

        /// <summary>
        /// Registers a new <see cref="MobileAppTableConfiguration" /> that maps a route for all controllers that derive from <see cref="TableController"/>.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <returns>The current <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration"/>.</returns>
        public static MobileAppConfiguration AddTables(this MobileAppConfiguration config)
        {
            MobileAppTableConfiguration tableConfig = new MobileAppTableConfiguration().MapTableControllers();
            AddTables(config, tableConfig);
            return config;
        }

        /// <summary>
        /// Registers the specified <see cref="MobileAppTableConfiguration"/>.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="tableConfig"></param>
        /// <returns>The current <see cref="Microsoft.Azure.Mobile.Server.Config.MobileAppConfiguration"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We only want this extension to apply to MobileAppConfiguration, not just any AppConfiguration")]
        public static MobileAppConfiguration AddTables(this MobileAppConfiguration config, MobileAppTableConfiguration tableConfig)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.RegisterConfigProvider(new TableMobileAppConfigProvider(tableConfig));
            return config;
        }
    }
}