// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class AlertPropertiesTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            AlertProperties alert = new AlertProperties
            {
                Body = "Body",
                ActionLocKey = "ActionLocKey",
                LocKey = "LocKey",
                LaunchImage = "LaunchImage"
            };
            alert.LogArgs.Add("Arg");

            // Assert
            SerializationAssert.VerifySerialization(alert, "{\"body\":\"Body\",\"action-loc-key\":\"ActionLocKey\",\"loc-key\":\"LocKey\",\"launch-image\":\"LaunchImage\",\"loc-args\":[\"Arg\"]}", new List<string> { "Item" });
        }
    }
}
