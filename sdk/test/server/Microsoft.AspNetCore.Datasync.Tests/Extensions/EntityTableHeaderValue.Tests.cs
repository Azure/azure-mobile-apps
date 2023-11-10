// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class EntityTableHeaderValue_Tests
{
    [Theory]
    [InlineData("\"taga\"", true, "\"taga\"", false, false)]
    [InlineData("\"taga\"", false, "\"taga\"", true, false)]
    [InlineData("*", false, "\"taga\"", false, true)]
    [InlineData("\"taga\"", false, "*", false, true)]
    [InlineData("\"taga\"", false, "\"tagb\"", false, false)]
    [InlineData("\"taga\"", false, "\"taga\"", false, true)]
    public void MatchesTests(string taga, bool aIsWeak, string tagb, bool bIsWeak, bool expected)
    {
        var a = new EntityTagHeaderValue(taga, aIsWeak);
        var b = new EntityTagHeaderValue(tagb, bIsWeak);
        a.Matches(b).Should().Be(expected);
    }

    [Fact]
    public void ToByteArray_ReturnsEmptyArray_WhenStar()
    {
        var a = new EntityTagHeaderValue("*");
        byte[] actual = a.ToByteArray();
        actual.Should().BeEmpty();
    }

    [Fact]
    public void ToByteArray_ReturnsValid_ForGuid()
    {
        var guid = Guid.NewGuid().ToByteArray();
        var etag = new EntityTagHeaderValue($"\"{Convert.ToBase64String(guid)}\"");
        byte[] actual = etag.ToByteArray();
        actual.Should().BeEquivalentTo(guid);
    }
}
