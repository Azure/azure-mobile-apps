// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// Contains settings for a service such as the connection strings, host name, subscription ID, etc.
    /// The <see cref="MobileAppSettingsDictionary"/> provides typed properties for known settings such as <see cref="M:HostName"/>
    /// and <see cref="M:SubscriptionId"/> as well as <see cref="IDictionary{TKey,TValue}"/> access for all other settings.
    /// </summary>
    /// <remarks>Any property values set on an instance will only stay in effect for the lifetime of the current <see cref="AppDomain"/>.
    /// To change the settings in a persistent manner, please update them using a mechanism provided by the service host.</remarks>
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "Not intended for serialization")]
    [Serializable]
    public class MobileAppSettingsDictionary : Dictionary<string, string>
    {
        private readonly Dictionary<string, ConnectionSettings> connections = new Dictionary<string, ConnectionSettings>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileAppSettingsDictionary"/> class.
        /// </summary>
        public MobileAppSettingsDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileAppSettingsDictionary"/> class with the specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing information about the <see cref="MobileAppSettingsDictionary"/> to be initialized.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that indicates the source destination and context information of a serialized stream.</param>
        protected MobileAppSettingsDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the set of connection strings for the service.
        /// </summary>
        public IDictionary<string, ConnectionSettings> Connections
        {
            get
            {
                return this.connections;
            }
        }

        /// <summary>
        /// Gets or sets the host name of the service.
        /// </summary>
        public virtual string HostName
        {
            get
            {
                return this.GetSetting(MobileAppSettingsKeys.HostName);
            }

            set
            {
                this.SetSetting(MobileAppSettingsKeys.HostName, value);
            }
        }

        /// <summary>
        /// Gets or sets the SubscriptionId of the service.
        /// </summary>
        public virtual string SubscriptionId
        {
            get
            {
                return this.GetSetting(MobileAppSettingsKeys.SubscriptionId);
            }

            set
            {
                this.SetSetting(MobileAppSettingsKeys.SubscriptionId, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the Notification Hub associated with this service for handling
        /// push notifications.
        /// </summary>
        public virtual string NotificationHubName
        {
            get
            {
                return this.GetSetting(MobileAppSettingsKeys.NotificationHubName);
            }

            set
            {
                this.SetSetting(MobileAppSettingsKeys.NotificationHubName, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ZUMO-API-VERSION header checks are skipped for all
        /// calls to MobileApp controllers.
        /// </summary>
        public virtual bool SkipVersionCheck
        {
            get
            {
                bool skipCheck = false;
                var check = this.GetSetting(MobileAppSettingsKeys.SkipVersionCheck);
                if (bool.TryParse(check, out skipCheck))
                {
                    return skipCheck;
                }
                return false;
            }
            set
            {
                this.SetSetting(MobileAppSettingsKeys.SkipVersionCheck, value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="KeyNotFoundException"/>, and a set operation creates a new element with the specified key.</returns>
        public new string this[string key]
        {
            get
            {
                try
                {
                    return base[key];
                }
                catch (KeyNotFoundException)
                {
                    string msg = RResources.Settings_KeyNotFound.FormatForUser(key);
                    throw new KeyNotFoundException(msg);
                }
            }

            set
            {
                base[key] = value;
            }
        }

        private string GetSetting(string name)
        {
            string value;
            if (this.TryGetValue(name, out value))
            {
                return value;
            }

            return null;
        }

        private void SetSetting(string name, string value)
        {
            // If null then we remove the setting if present
            if (value == null)
            {
                this.Remove(name);
            }
            else
            {
                this[name] = value;
            }
        }
    }
}