// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Moq;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class MobileAppAppBuilderExtensionTests
    {
        private HttpConfiguration config;
        private Mock<IAppBuilder> appBuilderMock;
        private IAppBuilder appBuilder;
        private Mock<IMobileAppSettingsProvider> settingsProviderMock;
        private MobileAppSettingsDictionary settings;

        public MobileAppAppBuilderExtensionTests()
        {
            this.config = new HttpConfiguration();

            this.settingsProviderMock = new Mock<IMobileAppSettingsProvider>();
            this.settings = new MobileAppSettingsDictionary();
            this.settingsProviderMock.Setup(s => s.GetMobileAppSettings())
                .Returns(this.settings);

            this.config.SetMobileAppSettingsProvider(this.settingsProviderMock.Object);

            this.appBuilderMock = new Mock<IAppBuilder>();
            this.appBuilder = this.appBuilderMock.Object;
        }

        [Fact]
        public void UseMobileAppAuthentication_ConfiguresServiceAuthentication_WhenAuthEnabled()
        {
            // Arrange
            MobileAppConfiguration configOptions = new MobileAppConfiguration();
            this.config.SetMobileAppConfiguration(configOptions);
            AppServiceAuthenticationOptions options = new AppServiceAuthenticationOptions();

            this.appBuilderMock.Setup(p => p.Use(It.IsAny<object>(), It.IsAny<object[]>()))
                .Callback<object, object[]>((mockObject, mockArgs) =>
                {
                    options = (AppServiceAuthenticationOptions)mockArgs[1];
                })
                .Returns(this.appBuilder)
                .Verifiable();

            // Act
            this.appBuilder.UseAppServiceAuthentication(options);

            // Assert
            this.appBuilderMock.Verify(p => p.Use(It.IsAny<object>(), It.IsAny<object[]>()), Times.Once);
            Assert.Equal("MobileApp", options.AuthenticationType);
        }
    }
}