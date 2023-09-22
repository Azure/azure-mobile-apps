// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;

namespace Microsoft.Datasync.Client.Test.Utils;

[ExcludeFromCodeCoverage]
public class Platform_Test
{
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
        Assert.NotNull(Platform.Instance.PlatformInformation);
        Assert.IsAssignableFrom<IPlatformInformation>(Platform.Instance.PlatformInformation);
    }

    [Fact]
    [Trait("Method", "UserAgentDetails")]
    public void UserAgentDetails_IsNotNull()
    {
        Assert.NotNull(Platform.UserAgentDetails);
        Assert.Contains("lang=Managed", Platform.UserAgentDetails);
        Assert.Contains("os=", Platform.UserAgentDetails);
        Assert.Contains("arch=", Platform.UserAgentDetails);
        Assert.Contains("version=", Platform.UserAgentDetails);
    }

    [Fact]
    [Trait("Method", "AssemblyVersion")]
    public void AssemblyVersion_IsNotNull()
    {
        Assert.NotNull(Platform.AssemblyVersion);
        Assert.NotEmpty(Platform.AssemblyVersion);
    }
}