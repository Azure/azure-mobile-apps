// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Tests;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using System.Reflection;
using Xunit.Abstractions;
using CosmosContainer = Microsoft.Azure.Cosmos.Container;

namespace Microsoft.AspNetCore.Datasync.CosmosDb.Tests;

[ExcludeFromCodeCoverage]
public class CosmosRepository_Tests : RepositoryTests<CosmosMovie>, IAsyncDisposable
{
    #region Setup
    private readonly ITestOutputHelper output;
    private readonly string connectionString;
    private CosmosClient cosmosClient;
    private CosmosContainer cosmosContainer;

    public CosmosRepository_Tests(ITestOutputHelper output) : base()
    {
        connectionString = Environment.GetEnvironmentVariable("ZUMO_COSMOSDB_CONNECTIONSTRING");
        this.output = output;
    }

    protected override bool CanRunLiveTests() => !string.IsNullOrWhiteSpace(connectionString);

    protected override Task<CosmosMovie> GetEntityAsync(string id)
    {
        throw new NotImplementedException();
    }

    protected override Task<int> GetEntityCountAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task<IRepository<CosmosMovie>> GetPopulatedRepositoryAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task<string> GetRandomEntityIdAsync(bool exists)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        throw new NotImplementedException();
    }
    #endregion

    [Fact]
    public void Ctor_UserEntity_Throws()
    {
        CosmosContainer container = Substitute.For<CosmosContainer>();
        Action act = () => _ = new CosmosRepository<User>(container);
        act.Should().Throw<InvalidCastException>();
    }

    [Fact]
    public void ItemRequestOptionsWithVersion_NullVersion_Copies()
    {
        CosmosContainer container = Substitute.For<CosmosContainer>();
        CosmosRepositoryOptions<CosmosMovie> options = new() { ItemRequestOptions = new() { EnableContentResponseOnWrite = true } };
        CosmosRepository<CosmosMovie> sut = new(container, options);

        MethodInfo sutMethod = typeof(CosmosRepository<CosmosMovie>)
            .GetMethod("ItemRequestOptionsWithVersion", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] { typeof(byte[]) }, null);

        ItemRequestOptions actual = (ItemRequestOptions)sutMethod.Invoke(sut, new object[] { (byte[])null });
        actual.Should().NotBeNull().And.BeEquivalentTo(options.ItemRequestOptions);
    }

    [Fact]
    public void ItemRequestOptionsWithVersion_WithVersion_Copies()
    {
        CosmosContainer container = Substitute.For<CosmosContainer>();
        CosmosRepositoryOptions<CosmosMovie> options = new() { ItemRequestOptions = new() { EnableContentResponseOnWrite = true } };
        CosmosRepository<CosmosMovie> sut = new(container, options);
        byte[] version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 };

        MethodInfo sutMethod = typeof(CosmosRepository<CosmosMovie>)
            .GetMethod("ItemRequestOptionsWithVersion", BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] { typeof(byte[]) }, null);

        ItemRequestOptions actual = (ItemRequestOptions)sutMethod.Invoke(sut, new object[] { version });
        actual.Should().NotBeNull().And.BeEquivalentTo(options.ItemRequestOptions, x => x.Excluding(m => m.IfMatchEtag));
        actual.IfMatchEtag.Should().BeEquivalentTo("abcde");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParseEntityId_NullId_Throws(string id)
    {
        CosmosContainer container = Substitute.For<CosmosContainer>();
        CosmosRepository<CosmosMovie> repository = new(container);

        Action act = () => _ = repository.ParseEntityId(id, out PartitionKey _);
        act.Should().Throw<HttpException>().WithStatusCode(400);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParseEntityId_NullResponse_Throws(string id)
    {
        CosmosContainer container = Substitute.For<CosmosContainer>();
        CosmosRepositoryOptions<CosmosMovie> options = Substitute.For<CosmosRepositoryOptions<CosmosMovie>>();
        options.TryGetPartitionKey(id, out PartitionKey _).ReturnsForAnyArgs((string)null);
        CosmosRepository<CosmosMovie> repository = new(container, options);

        Action act = () => _ = repository.ParseEntityId("123:abc", out PartitionKey _);
        act.Should().Throw<HttpException>().WithStatusCode(400);
    }

    [Theory]
    [InlineData("123:abc", "123", "[\"abc\"]")]
    public void ParseEntityId_WorksProperty(string id, string expectedId, string expectedPartitionKey)
    {
        CosmosContainer container = Substitute.For<CosmosContainer>();
        CosmosRepository<CosmosMovie> repository = new(container);

        string actualId = repository.ParseEntityId(id, out PartitionKey partitionKey);
        actualId.Should().Be(expectedId);
        partitionKey.ToString().Should().Be(expectedPartitionKey);
    }

    #region Artifacts
    class User : CosmosTableData { }
    #endregion
}
