// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Datasync.Test.Extensions;

[ExcludeFromCodeCoverage]
public class IHeaderDictionary_Tests
{
    private readonly byte[] testVersion = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F };
    private readonly string testETag = "\"AQBCIkeP\"";

    [Fact]
    public void AddFromEntity_NoAdds_WhenNullEntity()
    {
        // Arrange
        var headers = new HeaderDictionary();

        // Act
        headers.AddFromEntity(null);

        // Assert
        Assert.False(headers.ContainsKey("ETag"));
        Assert.False(headers.ContainsKey("Last-Modified"));
    }

    [Fact]
    public void AddFromEntity_NoETag_WhenNullVersion()
    {
        // Arrange
        var headers = new HeaderDictionary();
        var entity = new InMemoryEntity { Version = null, UpdatedAt = default };

        // Act
        headers.AddFromEntity(entity);

        // Assert
        Assert.False(headers.ContainsKey("ETag"));
        Assert.False(headers.ContainsKey("Last-Modified"));
    }

    [Fact]
    public void AddFromEntity_NoETag_WhenEmptyVersion()
    {
        // Arrange
        var headers = new HeaderDictionary();
        var entity = new InMemoryEntity { Version = Array.Empty<byte>(), UpdatedAt = default };

        // Act
        headers.AddFromEntity(entity);

        // Assert
        Assert.False(headers.ContainsKey("ETag"));
        Assert.False(headers.ContainsKey("Last-Modified"));
    }

    [Fact]
    public void AddFromEntity_ETag_WhenValidVersion()
    {
        // Arrange
        var headers = new HeaderDictionary();
        var entity = new InMemoryEntity { Version = testVersion, UpdatedAt = default };

        // Act
        headers.AddFromEntity(entity);

        // Assert
        Assert.Single(headers["ETag"]);
        Assert.Equal(testETag, headers["ETag"][0]);
        Assert.False(headers.ContainsKey("Last-Modified"));
    }

    [Fact]
    public void AddFromEntity_LastModified_WhenUpdatedAt()
    {
        // Arrange
        var headers = new HeaderDictionary();
        var entity = new InMemoryEntity { Version = null, UpdatedAt = DateTimeOffset.Parse("2019-01-30T13:30:15Z") };

        // Act
        headers.AddFromEntity(entity);

        // Assert
        Assert.False(headers.ContainsKey("ETag"));
        Assert.Single(headers["Last-Modified"]);
        Assert.Equal("Wed, 30 Jan 2019 13:30:15 GMT", headers["Last-Modified"][0]);
    }

    [Fact]
    public void AddFromEntity_BothHeaders_WhenBothSet()
    {
        // Arrange
        var headers = new HeaderDictionary();
        var entity = new InMemoryEntity { Version = testVersion, UpdatedAt = DateTimeOffset.Parse("2019-01-30T13:30:15Z") };

        // Act
        headers.AddFromEntity(entity);

        // Assert
        Assert.Single(headers["ETag"]);
        Assert.Equal(testETag, headers["ETag"][0]);
        Assert.Single(headers["Last-Modified"]);
        Assert.Equal("Wed, 30 Jan 2019 13:30:15 GMT", headers["Last-Modified"][0]);
    }

    [Fact]
    public void AddFromEntity_IsAdditive()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "Host", "Localhost" }
        };
        var entity = new InMemoryEntity { Version = testVersion, UpdatedAt = DateTimeOffset.Parse("2019-01-30T13:30:15Z") };

        // Act
        headers.AddFromEntity(entity);

        // Assert
        Assert.Single(headers["Host"]);
        Assert.Equal("Localhost", headers["Host"][0]);
        Assert.Single(headers["ETag"]);
        Assert.Equal(testETag, headers["ETag"][0]);
        Assert.Single(headers["Last-Modified"]);
        Assert.Equal("Wed, 30 Jan 2019 13:30:15 GMT", headers["Last-Modified"][0]);
    }

    [Fact]
    public void AddFromEntity_ReplacesHeaders()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "ETag", "Foo" },
            { "Last-Modified", "Yes" }
        };
        var entity = new InMemoryEntity { Version = testVersion, UpdatedAt = DateTimeOffset.Parse("2019-01-30T13:30:15Z") };

        // Act
        headers.AddFromEntity(entity);

        // Assert
        Assert.Single(headers["ETag"]);
        Assert.Equal(testETag, headers["ETag"][0]);
        Assert.Single(headers["Last-Modified"]);
        Assert.Equal("Wed, 30 Jan 2019 13:30:15 GMT", headers["Last-Modified"][0]);
    }
}
