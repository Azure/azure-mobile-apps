// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class MobileAppSettingsProviderTests
    {
        private static MethodInfo isSetMethod = typeof(MobileAppSettingsProviderTests).GetMethod("IsSet", BindingFlags.Static | BindingFlags.NonPublic);

        [Fact]
        public void GetMobileAppSettings_CallsInitializeSettingsAndReturnsResult()
        {
            // Arrange
            MobileAppSettingsDictionary settings = new MobileAppSettingsDictionary();
            Mock<MobileAppSettingsProvider> settingsProviderMock = new Mock<MobileAppSettingsProvider>() { CallBase = true };
            settingsProviderMock.Protected()
                .Setup<MobileAppSettingsDictionary>("InitializeSettings")
                .Returns(settings)
                .Verifiable();

            // Act
            MobileAppSettingsDictionary actualSettings = settingsProviderMock.Object.GetMobileAppSettings();

            // Assert
            Assert.Same(settings, actualSettings);
            settingsProviderMock.Verify();
        }

        [Fact]
        public void GetMobileAppSettings_ReturnsSameInstance()
        {
            // Arrange
            MobileAppSettingsProvider settingsProvider = new MobileAppSettingsProvider();

            // Act
            MobileAppSettingsDictionary settings1 = settingsProvider.GetMobileAppSettings();
            MobileAppSettingsDictionary settings2 = settingsProvider.GetMobileAppSettings();

            // Assert
            Assert.Same(settings1, settings2);
        }

        [Fact]
        public void GetMobileAppSettings_SetsAllKnownProperties()
        {
            // Arrange
            MobileAppSettingsProvider settingsProvider = new MobileAppSettingsProvider();

            // Act
            MobileAppSettingsDictionary actual = settingsProvider.GetMobileAppSettings();

            // Assert
            PropertyInfo[] properties = typeof(MobileAppSettingsDictionary).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                // Skipping dictionary accessors
                if (p.Name == "Item")
                {
                    continue;
                }

                if (p.CanWrite)
                {
                    MethodInfo isSet = isSetMethod.MakeGenericMethod(p.PropertyType);
                    bool result = (bool)isSet.Invoke(this, new object[] { p.GetValue(actual) });
                    Assert.True(result, string.Format("Property '{0}' was not set. Please ensure that the value is present in app.config for this test project and that the MobileAppSettingsProvider sets the value.", p.Name));
                }
            }

            Assert.True(actual.Connections.Count > 0);
        }

        [Theory]
        [InlineData("custom.azurewebsites.net", "custom.azurewebsites.net")]
        [InlineData("", "myapp.azurewebsites.net")]
        [InlineData(null, "myapp.azurewebsites.net")]
        public void GetMobileAppSettings_ReadsWebsiteHostNameEnvironmentVariable_ConfiguresHostName(string settingValue, string expectedValue)
        {
            Environment.SetEnvironmentVariable(MobileAppSettingsProvider.WebsiteHostNameEnvironmentVariableName, "myapp.azurewebsites.net");

            Mock<MobileAppSettingsProvider> settingsProviderMock = new Mock<MobileAppSettingsProvider>() { CallBase = true };
            NameValueCollection appSettings = new NameValueCollection();
            appSettings[MobileAppSettingsKeys.HostName] = settingValue;
            settingsProviderMock.Protected()
                .Setup<NameValueCollection>("GetAppSettings")
                .Returns(appSettings)
                .Verifiable();

            // Act
            MobileAppSettingsDictionary settings = settingsProviderMock.Object.GetMobileAppSettings();

            Assert.Equal(settings.HostName, expectedValue);

            Environment.SetEnvironmentVariable(MobileAppSettingsProvider.WebsiteHostNameEnvironmentVariableName, null);
        }

        [Theory]
        [InlineData("mysubscriptionid", "mysubscriptionid")]
        [InlineData("", "de6db429-0822-4034-8456-fccf4deb3841")]
        [InlineData(null, "de6db429-0822-4034-8456-fccf4deb3841")]
        public void GetMobileAppSettings_ReadsWebsiteOwnerEnvironmentVariable_ConfiguresSubscriptionId(string settingValue, string expectedValue)
        {
            Environment.SetEnvironmentVariable(MobileAppSettingsProvider.WebsiteOwnerEnvironmentVariableName, "de6db429-0822-4034-8456-fccf4deb3841+MyTestRG3-EastUSwebspace");

            Mock<MobileAppSettingsProvider> settingsProviderMock = new Mock<MobileAppSettingsProvider>() { CallBase = true };
            NameValueCollection appSettings = new NameValueCollection();
            appSettings[MobileAppSettingsKeys.SubscriptionId] = settingValue;
            settingsProviderMock.Protected()
                .Setup<NameValueCollection>("GetAppSettings")
                .Returns(appSettings)
                .Verifiable();

            // Act
            MobileAppSettingsDictionary settings = settingsProviderMock.Object.GetMobileAppSettings();

            Assert.Equal(settings.SubscriptionId, expectedValue);

            Environment.SetEnvironmentVariable(MobileAppSettingsProvider.WebsiteOwnerEnvironmentVariableName, null);
        }

        [Fact]
        public void GetMobileAppSettings_SetsCustomProperties()
        {
            // Arrange
            MobileAppSettingsProvider settingsProvider = new MobileAppSettingsProvider();

            // Act
            MobileAppSettingsDictionary actual = settingsProvider.GetMobileAppSettings();

            // Assert
            Assert.Equal(actual["SampleKey"], "SampleValue");
        }

        private static bool IsSet<T>(T value)
        {
            return !EqualityComparer<T>.Default.Equals(value, default(T));
        }
    }
}
