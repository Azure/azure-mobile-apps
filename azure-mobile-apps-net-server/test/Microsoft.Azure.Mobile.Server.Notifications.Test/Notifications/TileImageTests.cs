// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class TileImageTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            TileImage image = new TileImage
            {
                Id = 1,
                Src = "Src",
                Alt = "Alt",
                AddImageQuery = true
            };

            // Assert
            SerializationAssert.VerifySerialization(image, "{\"id\":1,\"src\":\"Src\",\"alt\":\"Alt\",\"addImageQuery\":true}");
        }
    }
}
