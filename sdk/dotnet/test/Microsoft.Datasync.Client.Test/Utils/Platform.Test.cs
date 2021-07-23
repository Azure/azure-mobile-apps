// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage]
    public class Platform_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "ApplicationStorage")]
        public void ApplicationStorage_IsNotNull()
        {
            Assert.NotNull(Platform.ApplicationStorage);
            Assert.IsAssignableFrom<IApplicationStorage>(Platform.ApplicationStorage);
        }

        [Fact]
        [Trait("Method", "Instance")]
        public void Instance_IsNotNull()
        {
            Assert.NotNull(Platform.Instance);
            Assert.IsAssignableFrom<IPlatform>(Platform.Instance);
        }

        [Fact]
        [Trait("Method", "PlaformInformation")]
        public void PlatformInformation_IsNotNull()
        {
            Assert.NotNull(Platform.PlatformInformation);
            Assert.IsAssignableFrom<IPlatformInformation>(Platform.PlatformInformation);
        }

        [Fact]
        [Trait("Method", "UserAgentDetails")]
        public void UserAgentDetails_IsNotNUll()
        {
            Assert.NotNull(Platform.UserAgentDetails);
            Assert.Contains("lang=Managed", Platform.UserAgentDetails);
            Assert.Contains("os=", Platform.UserAgentDetails);
            Assert.Contains("arch=", Platform.UserAgentDetails);
            Assert.Contains("version=", Platform.UserAgentDetails);
        }

        [Fact]
        [Trait("Method", "InstallationId")]
        public void InstallationId_IsNotNull()
        {
            Platform.ApplicationStorage.RemoveValue("installationId");

            // This may seem silly, but it's intended to ensure that the installation ID
            // doesn't change over successive calls
            var installationId = Platform.InstallationId;
            Assert.NotNull(installationId);
            Assert.Equal(installationId, Platform.InstallationId);
        }

        [Fact]
        [Trait("Method", "AssemblyVersion")]
        public void AssemblyVersion_IsNotNull()
        {
            Assert.NotNull(Platform.AssemblyVersion);
            Assert.NotEmpty(Platform.AssemblyVersion);
        }
    }
}
