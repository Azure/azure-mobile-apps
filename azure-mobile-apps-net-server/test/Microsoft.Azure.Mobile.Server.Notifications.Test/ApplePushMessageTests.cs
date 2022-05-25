// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class ApplePushMessageTests
    {
        private static readonly Dictionary<string, string> Templates = new Dictionary<string, string>()
        {
            { "AlertString", "{\r\n  \"aps\": {\r\n    \"alert\": \"Message received from Bob\"\r\n  },\r\n  \"acme2\": [\r\n    \"bang\",\r\n    \"whiz\"\r\n  ]\r\n}" },
            { "AlertProperties", "{\r\n  \"aps\": {\r\n    \"alert\": {\r\n      \"body\": \"Bob wants to play poker\",\r\n      \"action-loc-key\": \"PLAY\"\r\n    },\r\n    \"badge\": 5\r\n  },\r\n  \"acme1\": \"bar\",\r\n  \"acme2\": [\r\n    \"bang\",\r\n    \"whiz\"\r\n  ]\r\n}" },
            { "CloseAndViewButtons", "{\r\n  \"aps\": {\r\n    \"alert\": \"You got your emails.\",\r\n    \"badge\": 9,\r\n    \"sound\": \"bingbong.aiff\"\r\n  },\r\n  \"acme1\": \"bar\",\r\n  \"acme2\": 42\r\n}" }, 
            { "LocKeyAndArgs", "{\r\n  \"aps\": {\r\n    \"alert\": {\r\n      \"loc-key\": \"GAME_PLAY_REQUEST_FORMAT\",\r\n      \"loc-args\": [\r\n        \"Jenna\",\r\n        \"Frank\"\r\n      ]\r\n    },\r\n    \"sound\": \"chime\"\r\n  },\r\n  \"acme\": \"foo\"\r\n}" },
        };

        public static TheoryDataCollection<string, TimeSpan?> CtorData
        {
            get
            {
                return new TheoryDataCollection<string, TimeSpan?>
                {
                    { "Read this!", null },
                    { "你好世界", TimeSpan.FromMinutes(10) },
                };
            }
        }

        [Fact]
        public void Serializes_AlertString()
        {
            // Arrange
            ApplePushMessage apsNot = new ApplePushMessage();
            apsNot.Aps.Alert = "Message received from Bob";
            apsNot["acme2"] = new Collection<string> { "bang", "whiz" };

            // Act
            string actual = apsNot.ToString();

            // Assert
            Assert.Equal(Templates["AlertString"], actual);
        }

        [Fact]
        public void Serializes_AlertProperties()
        {
            ApplePushMessage apsNot = new ApplePushMessage();
            apsNot.Aps.AlertProperties.Body = "Bob wants to play poker";
            apsNot.Aps.AlertProperties.ActionLocKey = "PLAY";
            apsNot.Aps.Badge = 5;
            apsNot["acme1"] = "bar";
            apsNot["acme2"] = new Collection<string> { "bang", "whiz" };

            // Act
            string actual = apsNot.ToString();

            // Assert
            Assert.Equal(Templates["AlertProperties"], actual);
        }

        [Fact]
        public void Serializes_CloseAndViewButtons()
        {
            ApplePushMessage apsNot = new ApplePushMessage();
            apsNot.Aps.Alert = "You got your emails.";
            apsNot.Aps.Badge = 9;
            apsNot.Aps.Sound = "bingbong.aiff";
            apsNot["acme1"] = "bar";
            apsNot["acme2"] = 42;

            // Act
            string actual = apsNot.ToString();

            // Assert
            Assert.Equal(Templates["CloseAndViewButtons"], actual);
        }

        [Fact]
        public void Serializes_LocKeyAndArgs()
        {
            ApplePushMessage apsNot = new ApplePushMessage();
            apsNot.Aps.AlertProperties.LocKey = "GAME_PLAY_REQUEST_FORMAT";
            apsNot.Aps.AlertProperties.LogArgs.Add("Jenna");
            apsNot.Aps.AlertProperties.LogArgs.Add("Frank");
            apsNot.Aps.Sound = "chime";
            apsNot["acme"] = "foo";

            // Act
            string actual = apsNot.ToString();

            // Assert
            Assert.Equal(Templates["LocKeyAndArgs"], actual);
        }

        [Theory]
        [MemberData("CtorData")]
        public void ApplePushMessage_AlertExpiration_SetsAlertAndExpiration(string alert, TimeSpan? expiration)
        {
            // Act
            ApplePushMessage apsNot = new ApplePushMessage(alert, expiration);

            // Assert
            Assert.Equal(alert, apsNot.Aps.Alert);
            if (expiration != null)
            {
                Assert.True(apsNot.Expiration.HasValue);
                Assert.True(DateTimeOffset.UtcNow < apsNot.Expiration);
            }
            else
            {
                Assert.Null(apsNot.Expiration);
            }
        }

        [Fact]
        public void ApplePushMessage_AlertExpiration_ThrowsOnInvalidTimeSpan()
        {
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => new ApplePushMessage("Read This!", TimeSpan.FromMinutes(-1)));
            Assert.Equal("The value must be greater than 00:00:00.\r\nParameter name: expiration\r\nActual value was -00:01:00.", ex.Message);
        }

        [Fact]
        public void JsonPayload_TakesOverSerialization()
        {
            // Arrange
            ApplePushMessage apsNot = new ApplePushMessage()
            {
                JsonPayload = "text"
            };

            // Act
            string actual = apsNot.ToString();

            // Assert
            Assert.Equal("text", actual);
        }
    }
}
