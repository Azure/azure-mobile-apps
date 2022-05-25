// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class FlipTileTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            FlipTile tile = new FlipTile
            {
                BackBackgroundImage = new Uri("http://localhost/path1"),
                BackContent = "back content",
                BackgroundImage = new Uri("http://localhost/path2"),
                BackTitle = "back title",
                Count = 10,
                Id = "id",
                SmallBackgroundImage = new Uri("http://localhost/path3"),
                Title = "title",
                Version = "2.0",
                WideBackBackgroundImage = new Uri("http://localhost/path4"),
                WideBackContent = "你好世界",
                WideBackgroundImage = new Uri("http://localhost/path5"),
            };

            // Assert
            SerializationAssert.VerifySerialization(tile, "{\"smallBackgroundImage\":\"http://localhost/path3\",\"wideBackgroundImage\":\"http://localhost/path5\",\"wideBackBackgroundImage\":\"http://localhost/path4\",\"wideBackContent\":\"你好世界\",\"backgroundImage\":\"http://localhost/path2\",\"backBackgroundImage\":\"http://localhost/path1\",\"backTitle\":\"back title\",\"backContent\":\"back content\",\"id\":\"id\",\"count\":10,\"title\":\"title\"}");
        }
    }
}
