// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class TileTextTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            TileText image = new TileText
            {
                Id = 1,
                Lang = "Lang",
                Text = "Test"
            };

            // Assert
            SerializationAssert.VerifySerialization(image, "{\"id\":1,\"lang\":\"Lang\",\"text\":\"Test\"}");
        }
    }
}
