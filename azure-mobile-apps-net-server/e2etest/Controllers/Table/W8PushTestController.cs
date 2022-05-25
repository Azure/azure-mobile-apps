// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Controllers
{
    public class W8PushTestController : TableController<W8PushTestEntity>
    {
        public async Task<W8PushTestEntity> PostW8PushTestEntity(W8PushTestEntity item)
        {
            var settings = this.Configuration.GetServiceSettingsProvider().GetServiceSettings();
            string notificationHubName = settings.NotificationHubName;
            string notificationHubConnection = settings.Connections[ServiceSettingsKeys.NotificationHubConnectionString].ConnectionString;
            NotificationHubClient nhClient = NotificationHubClient.CreateClientFromConnectionString(notificationHubConnection, notificationHubName);

            NotificationOutcome pushResponse = null;

            switch (item.NHNotificationType)
            {
                case "wns":
                    pushResponse = await nhClient.SendWindowsNativeNotificationAsync(item.Payload);
                    break;

                case "apns":
                    pushResponse = await nhClient.SendAppleNativeNotificationAsync(item.Payload);
                    break;

                default:
                    throw new NotImplementedException("Push is not supported on this platform");
            }
            this.Configuration.Services.GetTraceWriter().Info("Push sent: " + pushResponse, this.Request);
            return new W8PushTestEntity()
            {
                Id = "1",
                PushResponse = pushResponse.State.ToString() + "-" + pushResponse.TrackingId,
            };
        }
    }
}