// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync.Test.Extensions;

[ExcludeFromCodeCoverage]
public class EntityTagHeaderValue_Tests
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
        // Arrange
        var a = new EntityTagHeaderValue(taga, aIsWeak);
        var b = new EntityTagHeaderValue(tagb, bIsWeak);

        // Act
        var actual = a.Matches(b);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToByteArray_ReturnsNull_WhenStar()
    {
        // Arrange
        var a = new EntityTagHeaderValue("*");

        // Act
        byte[] actual = a.ToByteArray();

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ToByteArray_ReturnsValid_ForGuid()
    {
        // Arrange
        var guid = Guid.NewGuid().ToByteArray();
        var etag = new EntityTagHeaderValue($"\"{Convert.ToBase64String(guid)}\"");

        // Act
        byte[] actual = etag.ToByteArray();

        // Assert
        Assert.Equal(guid, actual);
    }
}
