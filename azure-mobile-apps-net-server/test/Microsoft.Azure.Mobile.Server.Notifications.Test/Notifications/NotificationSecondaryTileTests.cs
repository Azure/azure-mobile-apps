// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Test
{
    public class NotificationSecondaryTileTests
    {
        [Fact]
        public void NotificationSecondaryTile_Serialization_IsConsistent()
        {
            // Arrange
            NotificationSecondaryTile testTile = new NotificationSecondaryTile
            {
                PushChannel = "myPushChannel",
                Tags = new List<string> { "tag1", "tag2" },
                Templates = new Dictionary<string, NotificationTemplate>
                {
                    {
                        "templateName1", 
                        new NotificationTemplate
                        {
                            Body = "myTemplateBody",
                            Headers = new Dictionary<string, string>
                            {
                                { "headerName1", "headerValue1" },
                                { "headerName2", "headerValue2" }
                            }
                        }
                    }
                }
            };

            // Assert
            SerializationAssert.VerifySerialization(testTile, "{\"pushChannel\":\"myPushChannel\",\"tags\":[\"tag1\",\"tag2\"],\"templates\":{\"templateName1\":{\"body\":\"myTemplateBody\",\"headers\":{\"headerName1\":\"headerValue1\",\"headerName2\":\"headerValue2\"},\"tags\":[]}}}");
        }
    }
}
