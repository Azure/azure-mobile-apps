// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class ITableData_Tests
{
    [Fact]
    public void HasValidVersion_Works()
    {
        EntityTableData empty = new() { Version = Array.Empty<byte>() };
        empty.HasValidVersion().Should().BeFalse();

        EntityTableData full = new() { Version = Guid.NewGuid().ToByteArray() };
        full.HasValidVersion().Should().BeTrue();
    }

    [Fact]
    public void ToEntityTagHeaderValue_Works()
    {
        EntityTableData empty = new() { Version = Array.Empty<byte>() };
        empty.ToEntityTagHeaderValue().Should().BeNull();

        EntityTableData full = new() { Version = Guid.Parse("cd331898-18aa-4664-ba6a-40c48de0546f").ToByteArray() };
        EntityTagHeaderValue actual = full.ToEntityTagHeaderValue();
        actual.Should().NotBeNull();
        actual.IsWeak.Should().BeFalse();
        actual.Tag.ToString().Should().Be("\"mBgzzaoYZEa6akDEjeBUbw==\"");
    }
}