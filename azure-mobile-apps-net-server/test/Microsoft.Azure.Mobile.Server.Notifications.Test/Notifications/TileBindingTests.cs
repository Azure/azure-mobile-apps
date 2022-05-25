// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class TileBindingTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            TileBinding tile = new TileBinding
            {
                Template = "Template",
                Fallback = "Fallback",
                Lang = "Lang",
                BaseUri = "BaseUri",
                Branding = "Branding",
                AddImageQuery = true,
                ContentId = "ContentId"
            };
            tile.Images.Add(new TileImage { Id = 1, Src = "Src", Alt = "Alt", AddImageQuery = true });
            tile.Texts.Add(new TileText { Id = 1, Lang = "Lang", Text = "Test" });

            // Assert
            SerializationAssert.VerifySerialization(tile, "{\"template\":\"Template\",\"fallback\":\"Fallback\",\"lang\":\"Lang\",\"baseUri\":\"BaseUri\",\"branding\":\"Branding\",\"addImageQuery\":true,\"contentId\":\"ContentId\",\"images\":[{\"id\":1,\"src\":\"Src\",\"alt\":\"Alt\",\"addImageQuery\":true}],\"texts\":[{\"id\":1,\"lang\":\"Lang\",\"text\":\"Test\"}]}");
        }
    }
}
