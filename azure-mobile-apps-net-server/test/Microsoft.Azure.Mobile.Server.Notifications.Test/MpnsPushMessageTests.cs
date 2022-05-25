// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server.Notifications;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class MpnsPushMessageTests
    {
        private static readonly Dictionary<string, string> Templates = new Dictionary<string, string>()
        {
            { "FlipTileEmpty", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification Version=\"2.0\" xmlns:wp=\"WPNotification\">\r\n  <wp:Tile Template=\"FlipTile\">\r\n    <wp:Count Action=\"Clear\" />\r\n    <wp:Title Action=\"Clear\" />\r\n    <wp:SmallBackgroundImage Action=\"Clear\" />\r\n    <wp:WideBackgroundImage Action=\"Clear\" />\r\n    <wp:WideBackBackgroundImage Action=\"Clear\" />\r\n    <wp:WideBackContent Action=\"Clear\" />\r\n    <wp:BackgroundImage Action=\"Clear\" />\r\n    <wp:BackBackgroundImage Action=\"Clear\" />\r\n    <wp:BackTitle Action=\"Clear\" />\r\n    <wp:BackContent Action=\"Clear\" />\r\n  </wp:Tile>\r\n</wp:Notification>" },
            { "FlipTile", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification Version=\"2.0\" xmlns:wp=\"WPNotification\">\r\n  <wp:Tile Id=\"TileID\" Template=\"FlipTile\">\r\n    <wp:Count>100</wp:Count>\r\n    <wp:Title>Title</wp:Title>\r\n    <wp:SmallBackgroundImage>http://localhost/some/path/1</wp:SmallBackgroundImage>\r\n    <wp:WideBackgroundImage>http://localhost/some/path/2</wp:WideBackgroundImage>\r\n    <wp:WideBackBackgroundImage>http://localhost/some/path/3</wp:WideBackBackgroundImage>\r\n    <wp:WideBackContent>Back of wide Tile size content</wp:WideBackContent>\r\n    <wp:BackgroundImage IsRelative=\"true\">/some/path/4</wp:BackgroundImage>\r\n    <wp:BackBackgroundImage IsRelative=\"true\">/some/path/5</wp:BackBackgroundImage>\r\n    <wp:BackTitle>你好</wp:BackTitle>\r\n    <wp:BackContent>世界</wp:BackContent>\r\n  </wp:Tile>\r\n</wp:Notification>" },
            { "CycleTileEmpty", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification Version=\"2.0\" xmlns:wp=\"WPNotification\">\r\n  <wp:Tile Template=\"CycleTile\">\r\n    <wp:Count Action=\"Clear\" />\r\n    <wp:Title Action=\"Clear\" />\r\n    <wp:SmallBackgroundImage Action=\"Clear\" />\r\n    <wp:CycleImage1 Action=\"Clear\" />\r\n    <wp:CycleImage2 Action=\"Clear\" />\r\n    <wp:CycleImage3 Action=\"Clear\" />\r\n    <wp:CycleImage4 Action=\"Clear\" />\r\n    <wp:CycleImage5 Action=\"Clear\" />\r\n    <wp:CycleImage6 Action=\"Clear\" />\r\n    <wp:CycleImage7 Action=\"Clear\" />\r\n    <wp:CycleImage8 Action=\"Clear\" />\r\n    <wp:CycleImage9 Action=\"Clear\" />\r\n  </wp:Tile>\r\n</wp:Notification>" }, 
            { "CycleTile", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification Version=\"2.0\" xmlns:wp=\"WPNotification\">\r\n  <wp:Tile Id=\"TileID\" Template=\"CycleTile\">\r\n    <wp:Count>100</wp:Count>\r\n    <wp:Title>你好世界</wp:Title>\r\n    <wp:SmallBackgroundImage IsRelative=\"true\">/some/path/1</wp:SmallBackgroundImage>\r\n    <wp:CycleImage1>http://localhost/some/path/2</wp:CycleImage1>\r\n    <wp:CycleImage2>http://localhost/some/path/3</wp:CycleImage2>\r\n    <wp:CycleImage3>http://localhost/some/path/4</wp:CycleImage3>\r\n    <wp:CycleImage4>http://localhost/some/path/5</wp:CycleImage4>\r\n    <wp:CycleImage5>http://localhost/some/path/6</wp:CycleImage5>\r\n    <wp:CycleImage6>http://localhost/some/path/7</wp:CycleImage6>\r\n    <wp:CycleImage7>http://localhost/some/path/8</wp:CycleImage7>\r\n    <wp:CycleImage8>http://localhost/some/path/9</wp:CycleImage8>\r\n    <wp:CycleImage9>http://localhost/some/path/10</wp:CycleImage9>\r\n  </wp:Tile>\r\n</wp:Notification>" },
            { "IconicTileEmpty", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification Version=\"2.0\" xmlns:wp=\"WPNotification\">\r\n  <wp:Tile Template=\"IconicTile\">\r\n    <wp:Count Action=\"Clear\" />\r\n    <wp:Title Action=\"Clear\" />\r\n    <wp:IconImage Action=\"Clear\" />\r\n    <wp:SmallIconImage Action=\"Clear\" />\r\n    <wp:WideContent1 Action=\"Clear\" />\r\n    <wp:WideContent2 Action=\"Clear\" />\r\n    <wp:WideContent3 Action=\"Clear\" />\r\n    <wp:BackgroundColor Action=\"Clear\" />\r\n  </wp:Tile>\r\n</wp:Notification>" },
            { "IconicTile", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification Version=\"2.0\" xmlns:wp=\"WPNotification\">\r\n  <wp:Tile Id=\"TileID\" Template=\"IconicTile\">\r\n    <wp:Count>100</wp:Count>\r\n    <wp:Title>你好世界</wp:Title>\r\n    <wp:IconImage>http://localhost/some/path/1</wp:IconImage>\r\n    <wp:SmallIconImage IsRelative=\"true\">/some/path/2</wp:SmallIconImage>\r\n    <wp:WideContent1>WideContent1</wp:WideContent1>\r\n    <wp:WideContent2>WideContent2</wp:WideContent2>\r\n    <wp:WideContent3>WideContent3</wp:WideContent3>\r\n    <wp:BackgroundColor>color</wp:BackgroundColor>\r\n  </wp:Tile>\r\n</wp:Notification>" },
            { "ToastEmpty", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification xmlns:wp=\"WPNotification\">\r\n  <wp:Toast />\r\n</wp:Notification>" },
            { "Toast", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<wp:Notification xmlns:wp=\"WPNotification\">\r\n  <wp:Toast>\r\n    <wp:Text1>Text 1</wp:Text1>\r\n    <wp:Text2>你好世界</wp:Text2>\r\n    <wp:Param>Parameter</wp:Param>\r\n  </wp:Toast>\r\n</wp:Notification>" }
        };

        [Fact]
        public void Serializes_FlipTileEmpty()
        {
            // Arrange
            FlipTile tile = new FlipTile();
            MpnsPushMessage pushMessage = new MpnsPushMessage(tile);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["FlipTileEmpty"], actual);
        }

        [Fact]
        public void Serializes_FlipTile()
        {
            // Arrange
            FlipTile tile = new FlipTile()
            {
                Id = "TileID",
                SmallBackgroundImage = new Uri("http://localhost/some/path/1"),
                WideBackgroundImage = new Uri("http://localhost/some/path/2"),
                WideBackBackgroundImage = new Uri("http://localhost/some/path/3"),
                WideBackContent = "Back of wide Tile size content",
                BackgroundImage = new Uri("/some/path/4", UriKind.Relative),
                BackBackgroundImage = new Uri("/some/path/5", UriKind.Relative),
                BackTitle = "你好",
                BackContent = "世界",
                Count = 100,
                Title = "Title"
            };
            MpnsPushMessage pushMessage = new MpnsPushMessage(tile);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["FlipTile"], actual);
        }

        [Fact]
        public void Serializes_CycleTileEmpty()
        {
            // Arrange
            CycleTile tile = new CycleTile();
            MpnsPushMessage pushMessage = new MpnsPushMessage(tile);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["CycleTileEmpty"], actual);
        }

        [Fact]
        public void Serializes_CycleTile()
        {
            // Arrange
            CycleTile tile = new CycleTile()
            {
                Id = "TileID",
                Count = 100,
                SmallBackgroundImage = new Uri("/some/path/1", UriKind.Relative),
                CycleImage1 = new Uri("http://localhost/some/path/2"),
                CycleImage2 = new Uri("http://localhost/some/path/3"),
                CycleImage3 = new Uri("http://localhost/some/path/4"),
                CycleImage4 = new Uri("http://localhost/some/path/5"),
                CycleImage5 = new Uri("http://localhost/some/path/6"),
                CycleImage6 = new Uri("http://localhost/some/path/7"),
                CycleImage7 = new Uri("http://localhost/some/path/8"),
                CycleImage8 = new Uri("http://localhost/some/path/9"),
                CycleImage9 = new Uri("http://localhost/some/path/10"),
                Title = "你好世界"
            };
            MpnsPushMessage pushMessage = new MpnsPushMessage(tile);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["CycleTile"], actual);
        }

        [Fact]
        public void Serializes_IconicTileEmpty()
        {
            // Arrange
            IconicTile tile = new IconicTile();
            MpnsPushMessage pushMessage = new MpnsPushMessage(tile);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["IconicTileEmpty"], actual);
        }

        [Fact]
        public void Serializes_IconicTile()
        {
            // Arrange
            IconicTile tile = new IconicTile()
            {
                Id = "TileID",
                Count = 100,
                IconImage = new Uri("http://localhost/some/path/1"),
                SmallIconImage = new Uri("/some/path/2", UriKind.Relative),
                WideContent1 = "WideContent1",
                WideContent2 = "WideContent2",
                WideContent3 = "WideContent3",
                BackgroundColor = "color",
                Title = "你好世界"
            };
            MpnsPushMessage pushMessage = new MpnsPushMessage(tile);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["IconicTile"], actual);
        }

        [Fact]
        public void Serializes_ToastEmpty()
        {
            // Arrange
            Toast tile = new Toast();
            MpnsPushMessage pushMessage = new MpnsPushMessage(tile);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["ToastEmpty"], actual);
        }

        [Fact]
        public void Serializes_Toast()
        {
            // Arrange
            Toast toast = new Toast()
            {
                Text1 = "Text 1",
                Text2 = "你好世界",
                Parameter = "Parameter"
            };
            MpnsPushMessage pushMessage = new MpnsPushMessage(toast);

            // Act
            string actual = pushMessage.ToString();

            // Assert
            Assert.Equal(Templates["Toast"], actual);
        }
    }
}
