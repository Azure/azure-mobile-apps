// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Test
{
    public class NotificationInstallationTests
    {
        [Fact]
        public void NotificationInstallation_Serialization_IsConsistent()
        {
            // Arrange
            NotificationTemplate testSecondaryTemplate = new NotificationTemplate
            {
                Body = "mySecondaryBody",
                Headers = new Dictionary<string, string>()
            };
            testSecondaryTemplate.Headers["mySecondaryHeaderName"] = "mySecondaryHeaderValue";

            NotificationSecondaryTile testSecondaryTile = new NotificationSecondaryTile
            {
                PushChannel = "myPushChannel",
                Tags = new List<string>(),
                Templates = new Dictionary<string, NotificationTemplate>()
            };
            testSecondaryTile.Tags.Add("myTag");
            testSecondaryTile.Templates["mySecondaryTemplateName"] = testSecondaryTemplate;

            NotificationInstallation installation = new NotificationInstallation
            {
                PushChannel = "myPushChannel",
                Platform = "myPlatform",
                InstallationId = "myId",
                SecondaryTiles = new Dictionary<string, NotificationSecondaryTile>(),
                Templates = new Dictionary<string, NotificationTemplate>()
            };
            installation.SecondaryTiles["myTestTile"] = testSecondaryTile;
            NotificationTemplate testTemplate = new NotificationTemplate
            {
                Body = "myBody",
                Headers = new Dictionary<string, string>()
            };
            testSecondaryTemplate.Headers["myHeaderName"] = "myHeaderValue";
            installation.Templates["myTemplateName"] = testTemplate;

            // Assert
            SerializationAssert.VerifySerialization(installation, "{\"pushChannel\":\"myPushChannel\",\"platform\":\"myPlatform\",\"installationId\":\"myId\",\"templates\":{\"myTemplateName\":{\"body\":\"myBody\",\"headers\":{},\"tags\":[]}},\"secondaryTiles\":{\"myTestTile\":{\"pushChannel\":\"myPushChannel\",\"tags\":[\"myTag\"],\"templates\":{\"mySecondaryTemplateName\":{\"body\":\"mySecondaryBody\",\"headers\":{\"mySecondaryHeaderName\":\"mySecondaryHeaderValue\",\"myHeaderName\":\"myHeaderValue\"},\"tags\":[]}}}},\"tags\":[]}");
        }
    }
}
