// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.NotificationHubs;
using Moq;
using Moq.Protected;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class PushClientTests
    {
        private const string ConStr = "Endpoint=sb://example.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=Hjahdh76ahsgfjakfhYHhshSDFFhsFkOlaskdfjlajg=";
        private const string HubName = "hubname";
        private static readonly string ConStrName = MobileAppSettingsKeys.NotificationHubConnectionString;

        private MobileAppSettingsDictionary settings;
        private Mock<IMobileAppSettingsProvider> settingsProviderMock;
        private HttpConfiguration config;
        private Mock<PushClient> clientMock;
        private PushClient client;

        public PushClientTests()
        {
            this.settings = new MobileAppSettingsDictionary();
            this.settingsProviderMock = new Mock<IMobileAppSettingsProvider>();
            this.settingsProviderMock.Setup(p => p.GetMobileAppSettings())
                .Returns(this.settings);

            this.config = new HttpConfiguration();
            this.config.SetMobileAppSettingsProvider(this.settingsProviderMock.Object);
            this.clientMock = new Mock<PushClient>(this.config) { CallBase = true };
            this.client = this.clientMock.Object;
        }

        public static TheoryDataCollection<IPushMessage, Type> PushMessages
        {
            get
            {
                return new TheoryDataCollection<IPushMessage, Type>
                {
                    { new WindowsPushMessage(), typeof(WindowsNotification) },
                    { new MpnsPushMessage(new CycleTile()), typeof(MpnsNotification) },
                    { new MpnsPushMessage(new FlipTile()), typeof(MpnsNotification) },
                    { new MpnsPushMessage(new IconicTile()), typeof(MpnsNotification) },
                    { new MpnsPushMessage(new Toast()), typeof(MpnsNotification) },
                    { new ApplePushMessage(), typeof(AppleNotification) },
                    { new GooglePushMessage(), typeof(GcmNotification) },
                    { new TemplatePushMessage(), typeof(TemplateNotification) },
                };
            }
        }

        [Fact]
        public void EnableTestSend_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.client, c => c.EnableTestSend, defaultValue: false, roundtripValue: true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "hubClient", Justification = "Necessary for testing.")]
        [Fact]
        public void EnableTestSend_Throws_IfSetWhenNotificationHubClientHasBeenCreated()
        {
            // Arrange
            this.settings.NotificationHubName = "HubName";
            this.settings.Connections.Add(ConStrName, new ConnectionSettings(ConStrName, ConStr));
            NotificationHubClient hubClient = this.client.HubClient;

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => this.client.EnableTestSend = true);

            // Assert
            Assert.Contains("The 'EnableTestSend' property must be set before the 'HubClient' property has been accessed", ex.Message);
        }

        [Fact]
        public void HubClient_Throws_IfNotificationHubConnectionStringAndAppSettingNotFound()
        {
            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => this.client.HubClient);

            // Assert
            Assert.Contains("Could not find a connection string named 'MS_NotificationHubConnectionString'", ex.Message);
        }

        [Fact]
        public void HubClient_Throws_IfNotificationHubNameNotFound()
        {
            // Arrange
            this.settings.Connections.Add(ConStrName, new ConnectionSettings(ConStrName, ConStr));

            // Act
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => this.client.HubClient);

            // Assert
            Assert.Contains("Could not find a service setting named 'MS_NotificationHubName'", ex.Message);
        }

        [Fact]
        public void HubClient_CreatesClient_PreferringConnectionStringOverAppSetting()
        {
            // Arrange
            this.client.EnableTestSend = true;
            this.settings.Connections.Add(ConStrName, new ConnectionSettings(ConStrName, ConStr));
            this.settings["MS_NotificationHubConnectionString"] = "NotUsed";
            this.settings[MobileAppSettingsKeys.NotificationHubName] = HubName;
            this.clientMock.Protected()
                .Setup<NotificationHubClient>("CreateNotificationHubClient", ConStr, HubName, true)
                .Verifiable();

            // Act
            NotificationHubClient actual = this.client.HubClient;

            // Assert
            Assert.Null(actual);
            this.clientMock.Verify();
        }

        [Fact]
        public void HubClient_CreatesClient_UsingAppSettingIfConnectionStringNotSet()
        {
            // Arrange
            this.client.EnableTestSend = true;
            this.settings["MS_NotificationHubConnectionString"] = ConStr;
            this.settings[MobileAppSettingsKeys.NotificationHubName] = HubName;
            this.clientMock.Protected()
                .Setup<NotificationHubClient>("CreateNotificationHubClient", ConStr, HubName, true)
                .Verifiable();

            // Act
            NotificationHubClient actual = this.client.HubClient;

            // Assert
            Assert.Null(actual);
            this.clientMock.Verify();
        }

        [Fact]
        public async Task SendAsync_ThrowsOnUnsupportedPayload()
        {
            // Arrange
            Mock<IPushMessage> messageMock = new Mock<IPushMessage>();

            // Act
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => this.client.SendAsync(messageMock.Object));

            // Assert
            Assert.Contains("The 'Castle.Proxies.ObjectProxy_1' class is not a supported notification payload. Supported payloads are: WindowsPushMessage, MpnsPushMessage, ApplePushMessage, GooglePushMessage, TemplatePushMessage.", ex.Message);
        }

        [Theory]
        [MemberData("PushMessages")]
        public void SendAsync_SendsValidNotification(IPushMessage message, Type expected)
        {
            // Arrange
            this.clientMock.Protected()
                .Setup("SendNotificationAsync", ItExpr.IsAny<Notification>(), string.Empty)
                .Callback((Notification notification, string tagExpression) =>
                    {
                        Assert.IsType(expected, notification);
                    })
                .Verifiable();

            // Act
            this.client.SendAsync(message);

            // Assert
            this.clientMock.Verify();
        }

        [Theory]
        [MemberData("PushMessages")]
        public void SendAsync_WithTagExpression_SendsValidNotification(IPushMessage message, Type expected)
        {
            // Arrange
            string tagExpression = "Hello";
            this.clientMock.Protected()
                .Setup("SendNotificationAsync", ItExpr.IsAny<Notification>(), tagExpression)
                .Callback((Notification notification, string tagExpr) =>
                {
                    Assert.IsType(expected, notification);
                })
                .Verifiable();

            // Act
            this.client.SendAsync(message, tagExpression);

            // Assert
            this.clientMock.Verify();
        }
    }
}