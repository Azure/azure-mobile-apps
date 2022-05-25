// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class WindowsPushMessageTests
    {
        private static readonly Dictionary<string, string> Templates = new Dictionary<string, string>()
        {
            { "TileSquareBlock", "<tile>\r\n  <visual>\r\n    <binding template=\"TileSquareBlock\">\r\n      <text id=\"1\">Text Field 1 (block text)</text>\r\n      <text id=\"2\">Text Field 2</text>\r\n    </binding>\r\n  </visual>\r\n</tile>" },
            { "TileSquare150x150Block", "<tile>\r\n  <visual version=\"2\">\r\n    <binding template=\"TileSquare150x150Block\" fallback=\"TileSquareBlock\">\r\n      <text id=\"1\">Text Field 1 (block text)</text>\r\n      <text id=\"2\">Text Field 2</text>\r\n    </binding>\r\n  </visual>\r\n</tile>" },
            { "TileSquareImage", "<tile>\r\n  <visual>\r\n    <binding template=\"TileSquareImage\">\r\n      <image id=\"1\" src=\"image1\" alt=\"alt text\" />\r\n    </binding>\r\n  </visual>\r\n</tile>" }, 
            { "TileSquare150x150Image", "<tile>\r\n  <visual version=\"2\">\r\n    <binding template=\"TileSquare150x150Image\" fallback=\"TileSquareImage\">\r\n      <image id=\"1\" src=\"image1\" alt=\"alt text\" />\r\n    </binding>\r\n  </visual>\r\n</tile>" },
            { "TileWidePeekImageCollection01", "<tile>\r\n  <visual>\r\n    <binding template=\"TileWidePeekImageCollection01\">\r\n      <image id=\"1\" src=\"image1.png\" alt=\"larger image\" />\r\n      <image id=\"2\" src=\"image2.png\" alt=\"small image, row 1, column 1\" />\r\n      <image id=\"3\" src=\"image3.png\" alt=\"small image, row 1, column 2\" />\r\n      <image id=\"4\" src=\"image4.png\" alt=\"small image, row 2, column 1\" />\r\n      <image id=\"5\" src=\"image5.png\" alt=\"small image, row 2, column 2\" />\r\n      <text id=\"1\">Text Field 1 (larger text)</text>\r\n      <text id=\"2\">Text Field 2</text>\r\n    </binding>\r\n  </visual>\r\n</tile>" },
            { "TileWide310x150PeekImageCollection01", "<tile>\r\n  <visual version=\"2\">\r\n    <binding template=\"TileWide310x150PeekImageCollection01\" fallback=\"TileWidePeekImageCollection01\">\r\n      <image id=\"1\" src=\"image1.png\" alt=\"larger image\" />\r\n      <image id=\"2\" src=\"image2.png\" alt=\"small image, row 1, column 1\" />\r\n      <image id=\"3\" src=\"image3.png\" alt=\"small image, row 1, column 2\" />\r\n      <image id=\"4\" src=\"image4.png\" alt=\"small image, row 2, column 1\" />\r\n      <image id=\"5\" src=\"image5.png\" alt=\"small image, row 2, column 2\" />\r\n      <text id=\"1\">Text Field 1 (larger text)</text>\r\n      <text id=\"2\">Text Field 2</text>\r\n    </binding>\r\n  </visual>\r\n</tile>" },
        };

        [Fact]
        public void Serializes_TileSquareBlock()
        {
            // Arrange
            TileBinding binding = new TileBinding(new TileText { Id = 1, Text = "Text Field 1 (block text)" }, new TileText { Id = 2, Text = "Text Field 2" }) { Template = "TileSquareBlock" };
            WindowsPushMessage winNot = new WindowsPushMessage(1, binding);

            // Act
            string actual = winNot.ToString();

            // Assert
            Assert.Equal(Templates["TileSquareBlock"], actual);
        }

        [Fact]
        public void Serializes_TileSquare150X150Block()
        {
            // Arrange
            TileBinding binding = new TileBinding(new TileText { Id = 1, Text = "Text Field 1 (block text)" }, new TileText { Id = 2, Text = "Text Field 2" })
            {
                Template = "TileSquare150x150Block",
                Fallback = "TileSquareBlock"
            };
            WindowsPushMessage winNot = new WindowsPushMessage(2, binding);

            // Act
            string actual = winNot.ToString();

            // Assert
            Assert.Equal(Templates["TileSquare150x150Block"], actual);
        }

        [Fact]
        public void Serializes_TileSquareImage()
        {
            // Arrange
            TileBinding binding = new TileBinding(new TileImage { Id = 1, Src = "image1", Alt = "alt text" }) { Template = "TileSquareImage" };
            WindowsPushMessage winNot = new WindowsPushMessage(1, binding);

            // Act
            string actual = winNot.ToString();

            // Assert
            Assert.Equal(Templates["TileSquareImage"], actual);
        }

        [Fact]
        public void Serializes_TileSquare150X150Image()
        {
            // Arrange
            TileBinding binding = new TileBinding(new TileImage { Id = 1, Src = "image1", Alt = "alt text" })
            {
                Template = "TileSquare150x150Image",
                Fallback = "TileSquareImage"
            };
            WindowsPushMessage winNot = new WindowsPushMessage(2, binding);

            // Act
            string actual = winNot.ToString();

            // Assert
            Assert.Equal(Templates["TileSquare150x150Image"], actual);
        }

        [Fact]
        public void Serializes_TileWidePeekImageCollection01()
        {
            // Arrange
            Collection<TileImage> images = new Collection<TileImage>
            {
                new TileImage { Id = 1, Src = "image1.png", Alt = "larger image" },
                new TileImage { Id = 2, Src = "image2.png", Alt = "small image, row 1, column 1" },
                new TileImage { Id = 3, Src = "image3.png", Alt = "small image, row 1, column 2" },
                new TileImage { Id = 4, Src = "image4.png", Alt = "small image, row 2, column 1" },
                new TileImage { Id = 5, Src = "image5.png", Alt = "small image, row 2, column 2" },
            };
            Collection<TileText> texts = new Collection<TileText>
            {
                new TileText { Id = 1, Text = "Text Field 1 (larger text)" },
                new TileText { Id = 2, Text = "Text Field 2" },
            };
            TileBinding binding = new TileBinding(images, texts) { Template = "TileWidePeekImageCollection01" };
            WindowsPushMessage winNot = new WindowsPushMessage(1, binding);

            // Act
            string actual = winNot.ToString();

            // Assert
            Assert.Equal(Templates["TileWidePeekImageCollection01"], actual);
        }

        [Fact]
        public void Serializes_TileWide310X150PeekImageCollection01()
        {
            // Arrange
            Collection<TileImage> images = new Collection<TileImage>
            {
                new TileImage { Id = 1, Src = "image1.png", Alt = "larger image" },
                new TileImage { Id = 2, Src = "image2.png", Alt = "small image, row 1, column 1" },
                new TileImage { Id = 3, Src = "image3.png", Alt = "small image, row 1, column 2" },
                new TileImage { Id = 4, Src = "image4.png", Alt = "small image, row 2, column 1" },
                new TileImage { Id = 5, Src = "image5.png", Alt = "small image, row 2, column 2" },
            };
            Collection<TileText> texts = new Collection<TileText>
            {
                new TileText { Id = 1, Text = "Text Field 1 (larger text)" },
                new TileText { Id = 2, Text = "Text Field 2" },
            };
            TileBinding binding = new TileBinding(images, texts)
            {
                Template = "TileWide310x150PeekImageCollection01",
                Fallback = "TileWidePeekImageCollection01"
            };
            WindowsPushMessage winNot = new WindowsPushMessage(2, binding);

            // Act
            string actual = winNot.ToString();

            // Assert
            Assert.Equal(Templates["TileWide310x150PeekImageCollection01"], actual);
        }

        [Fact]
        public void XmlPayload_TakesOverSerialization()
        {
            // Arrange
            WindowsPushMessage winNot = new WindowsPushMessage()
            {
                XmlPayload = "text"
            };

            // Act
            string actual = winNot.ToString();

            // Assert
            Assert.Equal("text", actual);
        }
    }
}
