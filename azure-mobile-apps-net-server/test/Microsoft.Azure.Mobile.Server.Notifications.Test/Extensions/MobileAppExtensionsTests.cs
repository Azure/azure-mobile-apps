// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Microsoft.Owin.Testing;
using Moq;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications.Test.Extensions
{
    public class MobileAppExtensionsTests
    {
        [Fact]
        public async Task MapLegacyCrossDomainController_MapsRoutesCorrectly()
        {
            // Arrange
            NotificationInstallation notification = new NotificationInstallation();
            notification.InstallationId = Guid.NewGuid().ToString();
            notification.PushChannel = Guid.NewGuid().ToString();
            notification.Platform = "wns";

            TestServer server = TestServer.Create(app =>
            {
                HttpConfiguration config = new HttpConfiguration();

                var pushClientMock = new Mock<PushClient>(config);
                pushClientMock.Setup(p => p.CreateOrUpdateInstallationAsync(It.IsAny<Installation>()))
                    .Returns(Task.FromResult(0));
                pushClientMock.Setup(p => p.GetRegistrationsByTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(this.CreateCollectionQueryResult<RegistrationDescription>()));
                pushClientMock.Setup(p => p.DeleteInstallationAsync(It.IsAny<string>()))
                    .Returns(Task.FromResult(0));

                config.SetPushClient(pushClientMock.Object);
#pragma warning disable 618
                // Suppressing Obsolete warning on AddPushNotifications
                new MobileAppConfiguration()
                    .MapApiControllers()
                    .AddPushNotifications()
                    .ApplyTo(config);
#pragma warning restore 618
                app.UseWebApi(config);
            });

            HttpClient client = server.HttpClient;
            client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");

            // Act
            var notificationsPut = await client.PutAsJsonAsync("push/installations/" + notification.InstallationId, notification);
            var notificationsDelete = await client.DeleteAsync("push/installations/" + notification.InstallationId);

            var notificationsPutApi = await client.PutAsJsonAsync("api/NotificationInstallations?installationId=" + notification.InstallationId, notification);
            var notificationsDeleteApi = await client.DeleteAsync("api/NotificationInstallations?installationId=" + notification.InstallationId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, notificationsPut.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, notificationsDelete.StatusCode);

            // api routes should not be found
            Assert.Equal(HttpStatusCode.NotFound, notificationsPutApi.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, notificationsDeleteApi.StatusCode);
        }

        // CollectionQueryResult is internal so we need to use reflection to make one for mocking purposes.
        private CollectionQueryResult<T> CreateCollectionQueryResult<T>() where T : EntityDescription
        {
            var constructor = typeof(CollectionQueryResult<T>)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single();
            return constructor.Invoke(new object[] { null, null }) as CollectionQueryResult<T>;
        }
    }
}