// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// Provides a default implementation of <see cref="IMobileAppSettingsProvider"/> which gets the
    /// settings for the service from the global <see cref="ConfigurationManager"/>.
    /// </summary>
    public class MobileAppSettingsProvider : IMobileAppSettingsProvider
    {
        internal const string WebsiteOwnerEnvironmentVariableName = "WEBSITE_OWNER_NAME";
        internal const string WebsiteHostNameEnvironmentVariableName = "WEBSITE_HOSTNAME";

        private readonly Lazy<MobileAppSettingsDictionary> mobileAppSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileAppSettingsProvider"/> class.
        /// </summary>
        public MobileAppSettingsProvider()
        {
            this.mobileAppSettings = new Lazy<MobileAppSettingsDictionary>(() => this.InitializeSettings());
        }

        /// <inheritdoc />
        public MobileAppSettingsDictionary GetMobileAppSettings()
        {
            return this.mobileAppSettings.Value;
        }

        /// <summary>
        /// Initializes the <see cref="MobileAppSettingsDictionary"/> provided in response to <see cref="M:GetMobileAppSettings"/>.
        /// </summary>
        /// <returns>A fully initialized <see cref="MobileAppSettingsDictionary"/>.</returns>
        protected virtual MobileAppSettingsDictionary InitializeSettings()
        {
            MobileAppSettingsDictionary settings = new MobileAppSettingsDictionary();

            NameValueCollection appSettings = this.GetAppSettings();
            foreach (string key in appSettings.AllKeys)
            {
                settings[key] = appSettings[key];
            }

            string value = null;
            if (string.IsNullOrEmpty(settings.HostName))
            {
                // Get the host name for the site from the Antares WEBSITE_HOSTNAME environment variable.
                value = Environment.GetEnvironmentVariable(WebsiteHostNameEnvironmentVariableName);
                if (!string.IsNullOrEmpty(value))
                {
                    settings.HostName = value.Trim();
                }
            }

            ConnectionStringSettingsCollection connectionSettings = ConfigurationManager.ConnectionStrings;
            foreach (ConnectionStringSettings connectionSetting in connectionSettings)
            {
                settings.Connections.Add(connectionSetting.Name,
                    new ConnectionSettings(connectionSetting.Name, connectionSetting.ConnectionString)
                    {
                        Provider = connectionSetting.ProviderName
                    });
            }

            if (string.IsNullOrEmpty(settings.SubscriptionId))
            {
                // Parse the subscription ID out of the Antares WEBSITE_OWNER_NAME environment variable.
                // This value is of the form "de6db429-0822-4034-8456-fccf4deb3841+MyTestRG3-EastUSwebspace"
                value = Environment.GetEnvironmentVariable(WebsiteOwnerEnvironmentVariableName);
                if (!string.IsNullOrEmpty(value))
                {
                    int idx = value.IndexOf("+", StringComparison.OrdinalIgnoreCase);
                    if (idx != -1)
                    {
                        string subscriptionId = value.Substring(0, idx);
                        settings.SubscriptionId = subscriptionId;
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// Gets the current application settings in a mockable manner.
        /// </summary>
        /// <returns>A <see cref="NameValueCollection"/> containing the application settings.</returns>
        protected virtual NameValueCollection GetAppSettings()
        {
            return ConfigurationManager.AppSettings;
        }
    }
}