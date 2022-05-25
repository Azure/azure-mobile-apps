// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class ApsPropertiesTests
    {
        [Fact]
        public void Serialization_WithAlertString_IsConsistent()
        {
            // Arrange
            ApsProperties aps = new ApsProperties
            {
                Alert = "Alert",
                Badge = 0,
                Sound = "Sound",
                ContentAvailable = true,
            };

            // Assert
            SerializationAssert.VerifySerialization(aps, "{\"alert\":\"Alert\",\"badge\":0,\"sound\":\"Sound\",\"content-available\":true}", new List<string> { "AlertProperties", "Item" });
        }

        [Fact]
        public void Serialization_WithAlertProperties_IsConsistent()
        {
            // Arrange
            ApsProperties aps = new ApsProperties
            {
                Badge = 0,
                Sound = "Sound",
                ContentAvailable = true,
            };
            aps.AlertProperties.Body = "Alert";

            // Assert
            SerializationAssert.VerifySerialization(aps, "{\"badge\":0,\"sound\":\"Sound\",\"content-available\":true,\"alert\":{\"body\":\"Alert\"}}", new List<string> { "Alert", "Item" });
        }
    }
}
