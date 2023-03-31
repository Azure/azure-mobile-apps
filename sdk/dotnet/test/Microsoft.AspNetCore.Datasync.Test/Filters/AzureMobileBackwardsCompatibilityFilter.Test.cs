// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Filters;

namespace Microsoft.AspNetCore.Datasync.Test.Filters;

[ExcludeFromCodeCoverage]
public class AzureMobileBackwardsCompatibilityFilter_Tests
{
    [Theory]
    [InlineData("releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'", "releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)")]
    [InlineData("releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z' and releaseDate le datetimeoffset'1999-12-31T00:00:00.000Z'", "releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset) and releaseDate le cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)")]
    [InlineData("releaseDate eq datetime'1994-10-14T00:00:00.000Z'", "releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)")]
    [InlineData("releaseDate eq datetime'1994-10-14T00:00:00.000Z' and releaseDate eq datetime'1994-10-14T00:00:00.000Z'", "releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset) and releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)")]
    [InlineData("releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z' and releaseDate eq datetime'1994-10-14T00:00:00.000Z'", "releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset) and releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)")]
    [InlineData("releaseDate eq datetime'1994-10-14T00:00:00.000Z' and releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'", "releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset) and releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)")]
    public void BasicTranslateTests(string filter, string expected)
    {
        // Act
        var actual = AzureMobileBackwardsCompatibilityFilter.TranslateV2Filter(filter);

        // Assert
        Assert.Equal(expected, actual);
    }
}
