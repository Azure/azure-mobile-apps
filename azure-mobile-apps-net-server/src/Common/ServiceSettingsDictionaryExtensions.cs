// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Azure.Mobile.Server;

namespace Microsoft.Azure.Mobile
{
    /// <summary>
    /// Provides various extension methods for the <see cref="MobileAppSettingsDictionary"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class MobileAppSettingsDictionaryExtensions
    {
        /// <summary>
        /// Gets a connection string by first looking under connection strings and then if not found
        /// under application settings.
        /// </summary>
        /// <param name="settings">The <see cref="MobileAppSettingsDictionary"/> to extract the connection string from.</param>
        /// <param name="name">The name of the connection string to look up.</param>
        /// <returns>The connection string; or null if not present.</returns>
        public static string GetConnectionString(this MobileAppSettingsDictionary settings, string name)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            ConnectionSettings connectionSettings;
            if (!settings.Connections.TryGetValue(name, out connectionSettings))
            {
                // As a backup, attempt to get the connection string as a mobile app setting
                return settings.GetValueOrDefault(name);
            }
            else
            {
                return connectionSettings.ConnectionString;
            }
        }
    }
}
