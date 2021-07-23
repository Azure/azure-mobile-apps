// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Platforms;
using Microsoft.Datasync.Client.Utils;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Platforms
{
    [ExcludeFromCodeCoverage]
    public class CurrentPlatform_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "PlatformInformation")]
        public void PlatformInformation_IsNotNull()
        {
            var sut = new CurrentPlatform();
            Assert.NotNull(sut.PlatformInformation);
            Assert.IsAssignableFrom<IPlatformInformation>(sut.PlatformInformation);
        }
    }
}
