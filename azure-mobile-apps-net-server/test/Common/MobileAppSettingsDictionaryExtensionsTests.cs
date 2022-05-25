// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Microsoft.Azure.Mobile.Server.Config;
using Moq;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class MobileAppSettingsDictionaryExtensionsTests
    {
        private const string ConStr = "Endpoint=sb://example.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=Hjahdh76ahsgfjakfhYHhshSDFFhsFkOlaskdfjlajg=";
        private const string ConStrName = "SomeConnectionStringName";

        private MobileAppSettingsDictionary settings;
        private Mock<IMobileAppSettingsProvider> settingsProviderMock;

        public MobileAppSettingsDictionaryExtensionsTests()
        {
            this.settings = new MobileAppSettingsDictionary();
            this.settingsProviderMock = new Mock<IMobileAppSettingsProvider>();
            this.settingsProviderMock.Setup(p => p.GetMobileAppSettings())
                .Returns(this.settings);
        }

        [Fact]
        public void GetConnectionString_ReturnsConnectionString()
        {
            // Arrange
            this.settings.Connections.Add(ConStrName, new ConnectionSettings(ConStrName, ConStr));

            // Act
            string actual = this.settings.GetConnectionString(ConStrName);

            // Assert
            Assert.Equal(ConStr, actual);
        }

        [Fact]
        public void GetConnectionString_ReturnsAppSettingsIfConnectionStringNotSet()
        {
            // Arrange
            this.settings[ConStrName] = ConStr;

            // Act
            string actual = this.settings.GetConnectionString(ConStrName);

            // Assert
            Assert.Equal(ConStr, actual);
        }

        [Fact]
        public void GetConnectionString_PrefersConnectionStringOverAppSetting()
        {
            // Arrange
            this.settings.Connections.Add(ConStrName, new ConnectionSettings(ConStrName, ConStr));
            this.settings[ConStrName] = "NotUsed";

            // Act
            string actual = this.settings.GetConnectionString(ConStrName);

            // Assert
            Assert.Equal(ConStr, actual);
        }

        [Fact]
        public void GetConnectionString_ReturnsNullIfNeitherConnectionStringNorAppSettingSet()
        {
            Assert.Null(this.settings.GetConnectionString(ConStrName));
        }
    }
}
