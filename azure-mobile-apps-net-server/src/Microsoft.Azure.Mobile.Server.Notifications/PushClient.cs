// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Azure.NotificationHubs;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    /// <summary>
    /// The <see cref="PushClient"/> provides a mechanism for sending notifications to mobile applications
    /// through a Notification Hub.
    /// </summary>
    public class PushClient
    {
        private readonly HttpConfiguration config;
        private NotificationHubClient hubClient;
        private bool testSend;

        /// <summary>
        /// Initializes a new instance of the <see cref="PushClient"/> with a given
        /// </summary>
        /// <param name="config">
        /// The <see cref="HttpConfiguration"/> for the current service.
        /// </param>
        public PushClient(HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            this.config = config;
        }

        /// <summary>
        /// Gets or sets the <see cref="NotificationHubClient"/> to use for sending notifications.
        /// </summary>
        public virtual NotificationHubClient HubClient
        {
            get
            {
                if (this.hubClient == null)
                {
                    IMobileAppSettingsProvider provider = this.config.GetMobileAppSettingsProvider();
                    MobileAppSettingsDictionary settings = provider.GetMobileAppSettings();
                    string hubConnectionString = settings.GetConnectionString(MobileAppSettingsKeys.NotificationHubConnectionString);
                    if (string.IsNullOrEmpty(hubConnectionString))
                    {
                        throw new InvalidOperationException(RResources.NotificationHub_NoConnectionString.FormatForUser(MobileAppSettingsKeys.NotificationHubConnectionString));
                    }

                    string hubName = settings.NotificationHubName;
                    if (string.IsNullOrEmpty(hubName))
                    {
                        throw new InvalidOperationException(RResources.NotificationHub_NoHubName.FormatForUser(MobileAppSettingsKeys.NotificationHubName));
                    }

                    this.hubClient = this.CreateNotificationHubClient(hubConnectionString, hubName, this.EnableTestSend);
                }

                return this.hubClient;
            }

            set
            {
                this.hubClient = value;
            }
        }

        /// <summary>
        /// When test send is enabled, all notifications only reach up to 10 devices for each send call and the SendNotificationAsync method
        /// return a detailed list of the outcomes for all those notification deliveries (for example, authentication errors, throttling
        /// errors, and so on).
        /// </summary>
        public virtual bool EnableTestSend
        {
            get
            {
                return this.testSend;
            }

            set
            {
                if (this.hubClient != null)
                {
                    throw new InvalidOperationException(RResources.NotificationHub_TestSend.FormatForUser("EnableTestSend", "HubClient"));
                }

                this.testSend = value;
            }
        }

        /// <summary>
        /// Sends a notification to the Notification Hub.
        /// </summary>
        /// <param name="message">The notification payload is one of <see cref="WindowsPushMessage"/>,
        /// <see cref="ApplePushMessage"/>, or <see cref="TemplatePushMessage"/>.</param>
        /// <returns>A <see cref="Task{T}"/> representing the notification send operation.</returns>
        public virtual Task<NotificationOutcome> SendAsync(IPushMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Notification notification = this.CreateNotification(message);
            return this.SendNotificationAsync(notification, string.Empty);
        }

        /// <summary>
        /// Sends a notification to the Notification Hub with a given tag expression.
        /// </summary>
        /// <param name="message">The notification payload is one of <see cref="WindowsPushMessage"/>,
        /// <see cref="ApplePushMessage"/>, or <see cref="TemplatePushMessage"/>.</param>
        /// <param name="tagExpression">A tag expression representing the combination of tags to use for this notification.</param>
        /// <returns>A <see cref="Task{T}"/> representing the notification send operation.</returns>
        public virtual Task<NotificationOutcome> SendAsync(IPushMessage message, string tagExpression)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (tagExpression == null)
            {
                throw new ArgumentNullException("tagExpression");
            }

            Notification notification = this.CreateNotification(message);
            return this.SendNotificationAsync(notification, tagExpression);
        }

        /// <summary>
        /// Sends a notification to the Notification Hub with a given tag expression.
        /// </summary>
        /// <param name="message">The notification payload is one of <see cref="WindowsPushMessage"/>,
        /// <see cref="ApplePushMessage"/>, or <see cref="TemplatePushMessage"/>.</param>
        /// <param name="tags">The set of tags to use for this notification.</param>
        /// <returns>A <see cref="Task{T}"/> representing the notification send operation.</returns>
        /// <remarks>This method is obsolete. You should use the HubClient.SendNotificationAsync() method instead.</remarks>
        [Obsolete("This method is obsolete. You should use the HubClient.SendNotificationAsync() method instead.")]
        public virtual Task<NotificationOutcome> SendAsync(IPushMessage message, IEnumerable<string> tags)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (tags == null)
            {
                throw new ArgumentNullException("tags");
            }

            Notification notification = this.CreateNotification(message);
            return this.SendNotificationAsync(notification, tags);
        }

        public virtual Task CreateOrUpdateInstallationAsync(Installation installation)
        {
            if (installation == null)
            {
                throw new ArgumentNullException("installation");
            }

            return this.HubClient.CreateOrUpdateInstallationAsync(installation);
        }

        public virtual Task DeleteInstallationAsync(string installationId)
        {
            if (installationId == null)
            {
                throw new ArgumentNullException("installationId");
            }

            return this.HubClient.DeleteInstallationAsync(installationId);
        }

        public virtual Task<CollectionQueryResult<RegistrationDescription>> GetRegistrationsByTagAsync(string tag, string continuationToken, int top)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }

            return this.HubClient.GetRegistrationsByTagAsync(tag, continuationToken, top);
        }

        /// <summary>
        /// Creates a <see cref="NotificationHubClient"/> in a mockable manner.
        /// </summary>
        /// <param name="connectionString">The connection string for the <see cref="NotificationHubClient"/>.</param>
        /// <param name="hubName">The name of the hub.</param>
        /// <param name="enableTestSend">Indicates whether test sends should be enabled.</param>
        /// <returns>A new <see cref="NotificationHubClient"/> instance.</returns>
        protected virtual NotificationHubClient CreateNotificationHubClient(string connectionString, string hubName, bool enableTestSend)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            if (hubName == null)
            {
                throw new ArgumentNullException("hubName");
            }

            return NotificationHubClient.CreateClientFromConnectionString(connectionString, hubName, enableTestSend);
        }

        /// <summary>
        /// Creates a <see cref="Notification"/> from a <see cref="IPushMessage"/>.
        /// </summary>
        /// <param name="message">The <see cref="IPushMessage"/> to create the <see cref="Notification"/> from.</param>
        /// <returns>The resulting <see cref="Notification"/>.</returns>
        protected virtual Notification CreateNotification(IPushMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            Notification notification = null;
            ApplePushMessage apnsPush;
            WindowsPushMessage wnsPush;
            MpnsPushMessage mpnsPush;
            TemplatePushMessage templatePush;
            if ((wnsPush = message as WindowsPushMessage) != null)
            {
                notification = new WindowsNotification(wnsPush.ToString(), wnsPush.Headers);
            }
            else if ((mpnsPush = message as MpnsPushMessage) != null)
            {
                notification = new MpnsNotification(mpnsPush.ToString(), mpnsPush.Headers);
            }
            else if ((apnsPush = message as ApplePushMessage) != null)
            {
                DateTime? expiration = null;
                if (apnsPush.Expiration.HasValue)
                {
                    expiration = apnsPush.Expiration.Value.DateTime;
                }
                notification = new AppleNotification(message.ToString(), expiration);
            }
            else if (message is GooglePushMessage)
            {
                notification = new GcmNotification(message.ToString());
            }
            else if ((templatePush = message as TemplatePushMessage) != null)
            {
                notification = new TemplateNotification(templatePush);
            }
            else
            {
                throw new InvalidOperationException(GetUnknownPayloadError(message));
            }

            return notification;
        }

        /// <summary>
        /// Makes <see cref="NotificationHubClient"/> send operation mockable.
        /// </summary>
        /// <param name="notification">The notification to send.</param>
        /// <param name="tagExpression">A tag expression representing the combination of tags to use for this notification.</param>
        /// <returns>A <see cref="Task{T}"/> representing the notification send operation.</returns>
        protected virtual Task<NotificationOutcome> SendNotificationAsync(Notification notification, string tagExpression)
        {
            return this.HubClient.SendNotificationAsync(notification, tagExpression);
        }

        /// <summary>
        /// Makes <see cref="NotificationHubClient"/> send operation mockable.
        /// </summary>
        /// <param name="notification">The notification to send.</param>
        /// <param name="tags">The set of tags to use for this notification.</param>
        /// <returns>A <see cref="Task{T}"/> representing the notification send operation.</returns>
        protected virtual Task<NotificationOutcome> SendNotificationAsync(Notification notification, IEnumerable<string> tags)
        {
            return this.HubClient.SendNotificationAsync(notification, tags);
        }

        private static string GetUnknownPayloadError(IPushMessage message)
        {
            string knownFormats = string.Join(", ",
                typeof(WindowsPushMessage).Name,
                typeof(MpnsPushMessage).Name,
                typeof(ApplePushMessage).Name,
                typeof(GooglePushMessage).Name,
                typeof(TemplatePushMessage).Name);
            return RResources.NotificationHub_UnknownPayload.FormatForUser(message.GetType(), knownFormats);
        }
    }
}