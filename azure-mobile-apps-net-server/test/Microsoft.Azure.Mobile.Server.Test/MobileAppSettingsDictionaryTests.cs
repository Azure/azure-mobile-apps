// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class MobileAppSettingsDictionaryTests
    {
        private readonly MobileAppSettingsDictionary settings = new MobileAppSettingsDictionary();

        public static TheoryDataCollection<string, string> CustomSettings
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { "key", null },
                    { "key", string.Empty },
                    { "key", "value" },
                    { "你好世界", null },
                    { "你好世界", string.Empty },
                    { "你好世界", "你好" },
                    { string.Empty, null },
                    { string.Empty, string.Empty },
                    { string.Empty, "你好" },
                };
            }
        }

        public static TheoryDataCollection<string> UnknownKeys
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    "key",
                    "你好世界",
                    string.Empty,
                    "unknown"
                };
            }
        }

        [Fact]
        public void SubscriptionId_Roundtrips()
        {
            this.settings[MobileAppSettingsKeys.SubscriptionId] = "DefaultSubId";
            PropertyAssert.Roundtrips(this.settings, s => s.SubscriptionId, PropertySetter.NullRoundtrips, defaultValue: "DefaultSubId", roundtripValue: "Value");
        }

        [Fact]
        public void NotificationHubName_Roundtrips()
        {
            this.settings[MobileAppSettingsKeys.NotificationHubName] = "DefaultNotHubName";
            PropertyAssert.Roundtrips(this.settings, s => s.NotificationHubName, PropertySetter.NullRoundtrips, defaultValue: "DefaultNotHubName", roundtripValue: "Value");
        }

        [Fact]
        public void HostName_Roundtrips()
        {
            this.settings[MobileAppSettingsKeys.HostName] = "myapp.azurewebsites.net";
            PropertyAssert.Roundtrips(this.settings, s => s.HostName, PropertySetter.NullRoundtrips, defaultValue: "myapp.azurewebsites.net", roundtripValue: "Value");
        }

        [Theory]
        [MemberData("CustomSettings")]
        public void CustomSetting_Roundtrips(string key, string value)
        {
            // Arrange
            this.settings.Add(key, value);

            // Act
            string actual = this.settings[key];

            // Assert
            Assert.Equal(value, actual);
        }

        [Theory]
        [MemberData("CustomSettings")]
        public void Item_Roundtrips(string key, string value)
        {
            // Arrange
            this.settings[key] = value;

            // Act
            string actual = this.settings.GetValueOrDefault(key);

            // Assert
            Assert.Equal(value, actual);
        }

        [Theory]
        [MemberData("UnknownKeys")]
        public void Item_Throws_KeyNotFoundException(string key)
        {
            KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => this.settings[key]);

            Assert.Contains("No service setting was found with key '{0}'. Please ensure that your service is initialized with the correct application settings and connection strings.".FormatForUser(key), ex.Message);
        }

        [Theory]
        [MemberData("UnknownKeys")]
        public void Item_OnInterface_Throws_KeyNotFoundException(string key)
        {
            IDictionary<string, string> dictionary = (IDictionary<string, string>)this.settings;
            KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => dictionary[key]);

            Assert.Contains("The given key was not present in the dictionary.", ex.Message);
        }
    }
}