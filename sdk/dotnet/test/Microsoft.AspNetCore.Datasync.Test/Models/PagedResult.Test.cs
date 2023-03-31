// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Microsoft.AspNetCore.Datasync.Models;

namespace Microsoft.AspNetCore.Datasync.Test.Models;

[ExcludeFromCodeCoverage]
public class PagedResult_Tests
{
    [Fact]
    public void Ctor_CanCreateEmptyModel()
    {
        // Act
        var actual = new PagedResult();

        // Assert
        Assert.NotNull(actual);
        Assert.NotNull(actual.Items);
        Assert.Empty(actual.Items);
        Assert.Null(actual.Count);
        Assert.Null(actual.NextLink);
    }

    [Fact]
    public void Ctor_CanCreateFilledModel()
    {
        // Arrange
        var objects = Movies.OfType<InMemoryMovie>();

        // Act
        var actual = new PagedResult(objects);

        // Assert
        Assert.NotNull(actual);
        Assert.NotNull(actual.Items);
        Assert.Equal(248, actual.Items.Count());
        Assert.Null(actual.Count);
        Assert.Null(actual.NextLink);
    }
}
