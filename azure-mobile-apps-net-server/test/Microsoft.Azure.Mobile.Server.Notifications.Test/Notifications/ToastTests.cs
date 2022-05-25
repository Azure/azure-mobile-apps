// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class ToastTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            Toast tile = new Toast
            {
                Parameter = "parameter",
                Text1 = "你好",
                Text2 = "世界",
                Version = "1.0",
            };

            // Assert
            SerializationAssert.VerifySerialization(tile, "{\"text1\":\"你好\",\"text2\":\"世界\",\"parameter\":\"parameter\"}");
        }
    }
}
