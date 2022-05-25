// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// Defines the keys we look for in <see cref="MobileAppSettingsDictionary"/> to identify known settings.
    /// </summary>
    public static class MobileAppSettingsKeys
    {
        /// <summary>
        /// The key identifying the application setting containing the host name of the mobile service.
        /// </summary>
        public static readonly string HostName = "MS_WebsiteHostName";

        /// <summary>
        /// The key identifying the application setting containing the subscription id for the service.
        /// </summary>
        public static readonly string SubscriptionId = "MS_SubscriptionId";

        /// <summary>
        /// The key identifying the connection string setting containing the default connection string for table controllers.
        /// </summary>
        public static readonly string TableConnectionString = "MS_TableConnectionString";

        /// <summary>
        /// The key identifying the connection string setting containing the connection string for the Azure Notification Hub
        /// associated with this service.
        /// </summary>
        public static readonly string NotificationHubConnectionString = "MS_NotificationHubConnectionString";

        /// <summary>
        /// The key identifying the application setting containing the name of the Azure Notification Hub associated with this service.
        /// </summary>
        public static readonly string NotificationHubName = "MS_NotificationHubName";

        /// <summary>
        /// The key identifying whether the system will enforce version checks on all incoming Mobile App API calls
        /// </summary>
        public static readonly string SkipVersionCheck = "MS_SkipVersionCheck";
    }
}