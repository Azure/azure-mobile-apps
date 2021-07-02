// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Platforms;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Platforms
{
    [ExcludeFromCodeCoverage]
    public class PlatformInformation_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "OS")]
        public void OS_IsNotNull()
        {
            var sut = new PlatformInformation();
            Assert.NotNull(sut);
            Assert.NotNull(sut.OS);
            Assert.NotEmpty(sut.OS.Name);
            Assert.NotEmpty(sut.OS.Version);
            Assert.NotEmpty(sut.OS.Architecture);
        }

        [Fact]
        [Trait("Method", "IsEmulator")]
        public void IsEmulator_DoesNotThrow()
        {
            // We only need to make sure IsEmulator doesn't throw.
            var sut = new PlatformInformation();
            var isEmulator = sut.IsEmulator;
            Assert.True(isEmulator || !isEmulator);
        }

        [Fact]
        [Trait("Method", "UserAgentDetails")]
        public void UserAgentDetails_IsNotNull()
        {
            var sut = new PlatformInformation();
            Assert.NotNull(sut.UserAgentDetails);
            Assert.Contains("lang=Managed", sut.UserAgentDetails);
            Assert.Contains("os=", sut.UserAgentDetails);
            Assert.Contains("arch=", sut.UserAgentDetails);
            Assert.Contains("version=", sut.UserAgentDetails);
        }
    }
}
