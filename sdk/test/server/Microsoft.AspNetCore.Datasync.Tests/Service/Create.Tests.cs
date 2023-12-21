// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using System.Net;
using System.Text;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Create_Tests : ServiceTest, IClassFixture<ServiceApplicationFactory>
{
    public Create_Tests(ServiceApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData(null)]
    [InlineData("de76a422-7fb0-4f1f-9bb4-12b3c7882541")]
    public async Task Create_WithValidInput_Returns201(string id)
    {
        ClientMovie source = new(Movies.BlackPanther) { Id = id };

        HttpResponseMessage response = await client.PostAsJsonAsync(factory.MovieEndpoint, source, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.Created);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>(serializerOptions);
        clientMovie.Should().NotBeNull().And.HaveChangedMetadata(id, StartTime).And.BeEquivalentTo<IMovie>(source);

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().HaveEquivalentMetadataTo(inMemoryMovie).And.BeEquivalentTo<IMovie>(inMemoryMovie);

        response.Headers.Location.Should().Be($"{client.BaseAddress}{factory.MovieEndpoint}/{clientMovie.Id}");
        response.Headers.ETag.Should().BeETag($"\"{clientMovie.Version}\"");
    }

    [Fact]
    public async Task Create_ExistingId_Returns409()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        ClientMovie source = new(Movies.BlackPanther) { Id = existingMovie.Id };

        HttpResponseMessage response = await client.PostAsJsonAsync(factory.MovieEndpoint, source, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.Conflict);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>(serializerOptions);
        clientMovie.Should().NotBeNull().And.HaveEquivalentMetadataTo(existingMovie).And.BeEquivalentTo<IMovie>(existingMovie);

        response.Headers.ETag.Should().BeETag($"\"{clientMovie.Version}\"");
    }

    [Fact]
    public async Task Create_SoftDeleted_Returns409()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        ClientMovie source = new(Movies.BlackPanther) { Id = existingMovie.Id };

        HttpResponseMessage response = await client.PostAsJsonAsync(factory.MovieEndpoint, source, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.Conflict);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>(serializerOptions);
        clientMovie.Should().NotBeNull().And.HaveEquivalentMetadataTo(existingMovie).And.BeEquivalentTo<IMovie>(existingMovie);

        response.Headers.ETag.Should().BeETag($"\"{clientMovie.Version}\"");
    }

    [Fact]
    public async Task Create_CanRoundtrip_Types()
    {
        const string id = "ks01";
        ClientKitchenSink source = new()
        {
            Id = id,
            StringValue = "state=none",
            EnumValue = KitchenSinkState.None,
            DateOnlyValue = new DateOnly(2023, 12, 15),
            TimeOnlyValue = new TimeOnly(9, 52, 35)
        };

        HttpResponseMessage response = await client.PostAsJsonAsync(factory.KitchenSinkEndpoint, source, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.Created);

        ClientKitchenSink clientKitchenSink = await response.Content.ReadFromJsonAsync<ClientKitchenSink>(serializerOptions);
        clientKitchenSink.Should().NotBeNull().And.HaveChangedMetadata(id, StartTime).And.BeEquivalentTo<IKitchenSink>(source);

        InMemoryKitchenSink serverEntity = factory.GetServerEntityById<InMemoryKitchenSink>(id);
        serverEntity.Should().BeEquivalentTo<IKitchenSink>(source);
    }

    [Fact]
    public async Task Create_NonJsonData_Returns415()
    {
        const string content = "<html><body><h1>Not JSON</h1></body></html>";
        HttpResponseMessage response = await client.PostAsync(factory.MovieEndpoint, new StringContent(content, Encoding.UTF8, "text/html"));
        response.Should().HaveStatusCode(HttpStatusCode.UnsupportedMediaType);
    }
}
