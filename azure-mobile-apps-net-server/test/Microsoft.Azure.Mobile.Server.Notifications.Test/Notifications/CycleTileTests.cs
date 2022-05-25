// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class CycleTileTests
    {
        [Fact]
        public void Serialization_IsConsistent()
        {
            // Arrange
            CycleTile tile = new CycleTile
            {
                Count = 10,
                CycleImage1 = new Uri("http://localhost/path1"),
                CycleImage2 = new Uri("http://localhost/path2"),
                CycleImage3 = new Uri("http://localhost/path3"),
                CycleImage4 = new Uri("http://localhost/path4"),
                CycleImage5 = new Uri("http://localhost/path5"),
                CycleImage6 = new Uri("http://localhost/path6"),
                CycleImage7 = new Uri("http://localhost/path7"),
                CycleImage8 = new Uri("http://localhost/path8"),
                CycleImage9 = new Uri("http://localhost/path9"),
                Id = "id",
                SmallBackgroundImage = new Uri("http://localhost/path10"),
                Title = "title",
                Version = "2.0",
            };

            // Assert
            SerializationAssert.VerifySerialization(tile, "{\"smallBackgroundImage\":\"http://localhost/path10\",\"cycleImage1\":\"http://localhost/path1\",\"cycleImage2\":\"http://localhost/path2\",\"cycleImage3\":\"http://localhost/path3\",\"cycleImage4\":\"http://localhost/path4\",\"cycleImage5\":\"http://localhost/path5\",\"cycleImage6\":\"http://localhost/path6\",\"cycleImage7\":\"http://localhost/path7\",\"cycleImage8\":\"http://localhost/path8\",\"cycleImage9\":\"http://localhost/path9\",\"id\":\"id\",\"count\":10,\"title\":\"title\"}");
        }
    }
}
