﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Datasync.Tests.Models;

[ExcludeFromCodeCoverage]
public class TableControllerOptions_Tests
{
    [Fact]
    public void Ctor_DefaultsDontChange()
    {
        TableControllerOptions sut = new();

        sut.EnableSoftDelete.Should().BeFalse();
        sut.MaxTop.Should().Be(128000);
        sut.PageSize.Should().Be(100);
        sut.UnauthorizedStatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Theory]
    [InlineData(0, 10000)]
    [InlineData(-1, 10000)]
    [InlineData(128001, 10000)]
    [InlineData(100, -1)]
    [InlineData(100, 0)]
    [InlineData(100, 128001)]
    public void Ctor_NoNegativeNumbers(int pageSize, int maxTop)
    {
        Action act = () => _ = new TableControllerOptions { PageSize = pageSize, MaxTop = maxTop };
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_Roundtrips()
    {
        TableControllerOptions sut = new() { EnableSoftDelete = true, MaxTop = 100, PageSize = 50, UnauthorizedStatusCode = 510 };

        sut.EnableSoftDelete.Should().BeTrue();
        sut.MaxTop.Should().Be(100);
        sut.PageSize.Should().Be(50);
        sut.UnauthorizedStatusCode.Should().Be(510);
    }
}
