// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class IconicTileTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            IconicTile tile = new IconicTile
            {
                BackgroundColor = "background color",
                Count = 10,
                IconImage = new Uri("http://localhost/path1"),
                Id = "id",
                SmallIconImage = new Uri("http://localhost/path2"),
                WideContent1 = "wide content1",
                WideContent2 = "wide content2",
                WideContent3 = "wide content3",
                Title = "title",
                Version = "2.0",
            };

            // Assert
            SerializationAssert.VerifySerialization(tile, "{\"iconImage\":\"http://localhost/path1\",\"smallIconImage\":\"http://localhost/path2\",\"wideContent1\":\"wide content1\",\"wideContent2\":\"wide content2\",\"wideContent3\":\"wide content3\",\"backgroundColor\":\"background color\",\"id\":\"id\",\"count\":10,\"title\":\"title\"}");
        }
    }
}
