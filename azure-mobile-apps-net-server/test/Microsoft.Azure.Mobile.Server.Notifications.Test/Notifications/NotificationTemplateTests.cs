// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Test
{
    public class NotificationTemplateTests
    {
        [Fact]
        public void NotificationTemplate_Serialization_IsConsistent()
        {
            // Arrange
            NotificationTemplate testTemplate = new NotificationTemplate
            {
                Body = "myBody",
                Headers = new Dictionary<string, string>()
            };
            testTemplate.Headers["myHeader2Name"] = "myHeader2Value";
            testTemplate.Headers["myHeader1Name"] = "myHeader1Value";
            testTemplate.Headers["myHeader3Name"] = "myHeader3Value";

            // Assert
            SerializationAssert.VerifySerialization(testTemplate, "{\"body\":\"myBody\",\"headers\":{\"myHeader2Name\":\"myHeader2Value\",\"myHeader1Name\":\"myHeader1Value\",\"myHeader3Name\":\"myHeader3Value\"},\"tags\":[]}");
        }
    }
}
