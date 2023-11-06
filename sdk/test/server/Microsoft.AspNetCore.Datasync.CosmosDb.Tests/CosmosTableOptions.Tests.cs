// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Cosmos;
using NSubstitute;
using System.Drawing;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync.CosmosDb.Tests;

[ExcludeFromCodeCoverage]
public class CosmosTableOptions_Tests
{
    private readonly CosmosRepositoryOptions<CosmosMovie> sut = new();

    [Fact]
    public void ItemRequestOptions_CanRoundtrip()
    {
        ItemRequestOptions options = new() { EnableContentResponseOnWrite = true };
        sut.ItemRequestOptions = options;
        sut.ItemRequestOptions.Should().BeSameAs(options);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateEntityIdAsync_NullId_Works(string id)
    {
        CosmosMovie entity = new() { Id = id };
        string actual = await sut.CreateEntityIdAsync(entity);
        actual.Should().NotBeEmpty().And.EndWith(":default").And.NotStartWith(":default");
    }

    [Theory]
    [InlineData("123:abc")]
    public async Task CreateEntityIdAsync_NonNullId_Works(string id)
    {
        CosmosMovie entity = new() { Id = id };
        string actual = await sut.CreateEntityIdAsync(entity);
        actual.Should().Be(id);
    }

    [Theory]
    [InlineData("default")]
    public async Task CreateEntityIdAsync_BadId_Throws(string id)
    {
        CosmosMovie entity = new() { Id = id };
        Func<Task> act = async () => _ = await sut.CreateEntityIdAsync(entity);
        (await act.Should().ThrowAsync<HttpException>()).WithStatusCode(400);
    }

    // Remember: the partition key is JSON!
    [Theory]
    [InlineData("123:abc", "123", "[\"abc\"]")]
    public void TryGetParitionKey_Valid_Works(string id, string expectedId, string expectedPartitionKey)
    {
        string actualId = sut.TryGetPartitionKey(id, out PartitionKey partitionKey);
        actualId.Should().Be(expectedId);
        partitionKey.ToString().Should().Be(expectedPartitionKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("foo")]
    [InlineData(":foo")]
    [InlineData("foo:")]
    [InlineData("foo::")]
    [InlineData("foo:bar:baz")]
    public void TryGetPartitionKey_Invalid_Throws(string id)
    {
        Action act = () => _ = sut.TryGetPartitionKey(id, out PartitionKey partitionKey);
        act.Should().Throw<HttpException>().WithStatusCode(400);
    }
}
