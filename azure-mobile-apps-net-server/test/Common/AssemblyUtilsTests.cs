// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Reflection;
using Microsoft.Azure.Mobile.Server.Mocks;
using Moq;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class AssemblyUtilsTests
    {
        private const string FileVersion = "FileVersion";
        private const string InfoVersion = "InfoVersion";

        private AssemblyFileVersionAttribute fileAttr;
        private Mock<AssemblyMock> asmMock;
        private AssemblyMock asm;

        public AssemblyUtilsTests()
        {
            this.fileAttr = new AssemblyFileVersionAttribute(FileVersion);
            this.asmMock = new Mock<AssemblyMock>();
            this.asm = this.asmMock.Object;
        }

        [Fact]
        public void GetExecutingAssemblyFileVersionOrDefault_ReturnsExpectedVersion()
        {
            // Arrange
            this.asmMock.Setup(a => a.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true))
                .Returns(new Attribute[] { this.fileAttr })
                .Verifiable();

            // Act
            string actual = AssemblyUtils.GetExecutingAssemblyFileVersionOrDefault(this.asm);

            // Assert
            Assert.Equal("FileVersion", actual);
            this.asmMock.Verify();
        }

        [Fact]
        public void GetExecutingAssemblyFileVersionOrDefault_HandlesMissingFileVersion()
        {
            // Arrange
            this.asmMock.Setup(a => a.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true))
                .Returns(new Attribute[] { })
                .Verifiable();

            // Act
            string actual = AssemblyUtils.GetExecutingAssemblyFileVersionOrDefault(this.asm);

            // Assert
            Assert.Equal("<unknown>", actual);
            this.asmMock.Verify();
        }
    }
}
