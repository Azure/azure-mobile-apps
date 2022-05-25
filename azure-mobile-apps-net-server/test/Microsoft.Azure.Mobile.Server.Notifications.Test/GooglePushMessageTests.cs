// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class GooglePushMessageTests
    {
        private static readonly Dictionary<string, string> Templates = new Dictionary<string, string>()
        {
            { "Demo", "{\r\n  \"data\": {\r\n    \"key1\": \"value1\",\r\n    \"key2\": \"value2\"\r\n  },\r\n  \"collapse_key\": \"demo\",\r\n  \"delay_while_idle\": true,\r\n  \"time_to_live\": 3\r\n}" },
        };

        private GooglePushMessage message = new GooglePushMessage();

        public static TheoryDataCollection<IDictionary<string, string>, TimeSpan?> CtorData
        {
            get
            {
                return new TheoryDataCollection<IDictionary<string, string>, TimeSpan?>
                {
                    { new Dictionary<string, string> { { "data1", "value1" } }, null },
                    { new Dictionary<string, string> { { "data1", "value1" }, { "你好", "世界" } }, TimeSpan.FromDays(1) },
                };
            }
        }

        [Fact]
        public void CollapseKey_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.message, m => m.CollapseKey, PropertySetter.NullRoundtrips, roundtripValue: "你好世界");
        }

        [Fact]
        public void DelayWhileIdle_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.message, m => m.DelayWhileIdle, roundtripValue: true);
        }

        [Fact]
        public void TimeToLiveInSeconds_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.message, m => m.TimeToLiveInSeconds, PropertySetter.NullRoundtrips, roundtripValue: 1024, minLegalValue: 0);
        }

        [Fact]
        public void Serializes_Demo()
        {
            // Arrange
            GooglePushMessage gcmNot = new GooglePushMessage();
            gcmNot.CollapseKey = "demo";
            gcmNot.DelayWhileIdle = true;
            gcmNot.Data.Add("key1", "value1");
            gcmNot.Data.Add("key2", "value2");
            gcmNot.TimeToLiveInSeconds = 3;

            // Act
            string actual = gcmNot.ToString();

            // Assert
            Assert.Equal(Templates["Demo"], actual);
        }

        [Theory]
        [MemberData("CtorData")]
        public void GooglePushMessage_DataTimeToLive_SetsDataAndTimeToLive(IDictionary<string, string> data, TimeSpan? timeToLive)
        {
            // Act
            GooglePushMessage gcmNot = new GooglePushMessage(data, timeToLive);

            // Assert
            foreach (string dataKey in data.Keys)
            {
                Assert.Equal(data[dataKey], gcmNot.Data[dataKey]);
            }

            if (timeToLive != null)
            {
                Assert.Equal(timeToLive.Value.TotalSeconds, gcmNot.TimeToLiveInSeconds.Value);
            }
            else
            {
                Assert.Null(gcmNot.TimeToLiveInSeconds);
            }
        }

        [Fact]
        public void GooglePushMessage_DataTimeToLive_ThrowsOnNegativeTimeSpan()
        {
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GooglePushMessage(new Dictionary<string, string>(), TimeSpan.FromMinutes(-1)));
            Assert.Equal("The value must be greater than 00:00:00.\r\nParameter name: timeToLive\r\nActual value was -00:01:00.", ex.Message);
        }

        [Fact]
        public void GooglePushMessage_DataTimeToLive_ThrowsOnTooLargeTimeSpan()
        {
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GooglePushMessage(new Dictionary<string, string>(), TimeSpan.FromDays(30)));
            Assert.Equal("The value must be less than 28.00:00:00.\r\nParameter name: timeToLive\r\nActual value was 30.00:00:00.", ex.Message);
        }

        [Fact]
        public void JsonPayload_TakesOverSerialization()
        {
            // Arrange
            GooglePushMessage gcmNot = new GooglePushMessage()
            {
                JsonPayload = "text"
            };

            // Act
            string actual = gcmNot.ToString();

            // Assert
            Assert.Equal("text", actual);
        }
    }
}
