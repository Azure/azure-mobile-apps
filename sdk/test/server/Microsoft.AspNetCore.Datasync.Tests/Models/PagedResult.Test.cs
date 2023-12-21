// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Models;

namespace Microsoft.AspNetCore.Datasync.Tests.Models;

[ExcludeFromCodeCoverage]
public class PagedResult_Tests
{
    [Fact]
    public void PagedResult_DefaultConstructor()
    {
        PagedResult result = new();

        result.Items.Should().NotBeNull().And.BeEmpty();
        result.Count.Should().BeNull();
        result.NextLink.Should().BeNullOrEmpty();
    }
}
