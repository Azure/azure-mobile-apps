// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class VisualTileTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            VisualTile visual = new VisualTile
            {
                AddImageQuery = true,
                BaseUri = "BaseUri",
                Branding = "Branding",
                ContentId = "ContentId",
                Version = 2,
                Lang = "Lang",
            };
            visual.Bindings.Add(new TileBinding());

            // Assert
            SerializationAssert.VerifySerialization(visual, "{\"version\":2,\"lang\":\"Lang\",\"baseUri\":\"BaseUri\",\"branding\":\"Branding\",\"addImageQuery\":true,\"contentId\":\"ContentId\",\"bindings\":[{\"template\":null,\"fallback\":null,\"lang\":null,\"baseUri\":null,\"branding\":null,\"addImageQuery\":false,\"contentId\":null,\"images\":[],\"texts\":[]}]}");
        }
    }
}
