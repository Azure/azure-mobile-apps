// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Dispatcher;
using Microsoft.Azure.Mobile.Server.Tables;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/> facilitating getting the Table-related classes used
    /// by the backend.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TableHttpConfigurationExtensions
    {
        private const string TableControllerConfigProviderKey = "MS_TableControllerConfigProvider";

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tableConfigProvider"></param>
        public static void SetTableControllerConfigProvider(this HttpConfiguration config, ITableControllerConfigProvider tableConfigProvider)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            config.Properties[TableControllerConfigProviderKey] = tableConfigProvider;
        }

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ITableControllerConfigProvider GetTableControllerConfigProvider(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            ITableControllerConfigProvider tableConfigProvider;
            if (!config.Properties.TryGetValue(TableControllerConfigProviderKey, out tableConfigProvider))
            {
                tableConfigProvider = new TableControllerConfigProvider();
                config.Properties[TableControllerConfigProviderKey] = tableConfigProvider;
            }

            return tableConfigProvider;
        }

        /// <summary>
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static HashSet<string> GetTableControllerNames(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            IAssembliesResolver assemblyResolver = config.Services.GetAssembliesResolver();
            IHttpControllerTypeResolver controllerTypeResolver = config.Services.GetHttpControllerTypeResolver();
            Type[] controllerTypes = controllerTypeResolver.GetControllerTypes(assemblyResolver).ToArray();

            // Add controllers deriving from TableController
            IEnumerable<string> matches = controllerTypes
                .Where(t => typeof(TableController).IsAssignableFrom(t))
                .Select(t => t.Name.Substring(0, t.Name.Length - DefaultHttpControllerSelector.ControllerSuffix.Length));

            HashSet<string> result = new HashSet<string>(matches, StringComparer.OrdinalIgnoreCase);
            return result;
        }
    }
}