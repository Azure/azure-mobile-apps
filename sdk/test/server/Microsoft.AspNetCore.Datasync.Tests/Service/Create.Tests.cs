// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Create_Tests : IClassFixture<ServiceApplicationFactory>
{
    private readonly HttpClient client;
    private readonly ServiceApplicationFactory factory;
    private readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    public Create_Tests(ServiceApplicationFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("de76a422-7fb0-4f1f-9bb4-12b3c7882541")]
    public async Task Create_WithValidInput_Returns201(string id)
    {
        ClientMovie source = new(Movies.BlackPanther) { Id = id };
        string content = JsonSerializer.Serialize(source);
        HttpResponseMessage response = await client.PostAsync(factory.MovieEndpoint, new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Created);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().NotBeNullOrEmpty();
        if (!string.IsNullOrEmpty(id))
        {
            clientMovie.Id.Should().Be(id);
        }
        clientMovie.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        clientMovie.Version.Should().NotBeNullOrEmpty();

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().BeEquivalentTo<IMovie>(inMemoryMovie);
        clientMovie.UpdatedAt.Should().Be(inMemoryMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(inMemoryMovie.Version));

        response.Headers.Location.Should().Be($"{client.BaseAddress}{factory.MovieEndpoint}/{clientMovie.Id}");
        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Fact]
    public async Task Create_ExistingId_Returns409()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        ClientMovie source = new(Movies.BlackPanther) { Id = existingMovie.Id };
        string content = JsonSerializer.Serialize(source);
        HttpResponseMessage response = await client.PostAsync(factory.MovieEndpoint, new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Conflict);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull().And.BeEquivalentTo<IMovie>(existingMovie);
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().Be(existingMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(existingMovie.Version));

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Fact]
    public async Task Create_SoftDeleted_Returns409()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);

        ClientMovie source = new(Movies.BlackPanther) { Id = existingMovie.Id };
        string content = JsonSerializer.Serialize(source);
        HttpResponseMessage response = await client.PostAsync(factory.SoftDeletedMovieEndpoint, new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Conflict);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull().And.BeEquivalentTo<IMovie>(existingMovie);
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().Be(existingMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(existingMovie.Version));

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
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
        string content = JsonSerializer.Serialize(source);
        HttpResponseMessage response = await client.PostAsync(factory.KitchenSinkEndpoint, new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Created);

        ClientKitchenSink clientKitchenSink = await response.Content.ReadFromJsonAsync<ClientKitchenSink>();
        clientKitchenSink.Should().NotBeNull();
        clientKitchenSink.Id.Should().Be(id);
        clientKitchenSink.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        clientKitchenSink.Version.Should().NotBeNullOrEmpty();

        clientKitchenSink.StringValue.Should().Be(source.StringValue);
        clientKitchenSink.EnumValue.Should().Be(source.EnumValue);
        clientKitchenSink.DateOnlyValue.Should().Be(source.DateOnlyValue);
        clientKitchenSink.TimeOnlyValue.Should().Be(source.TimeOnlyValue);

        InMemoryKitchenSink serverEntity = factory.GetServerEntityById<InMemoryKitchenSink>(id);

        serverEntity.StringValue.Should().Be(source.StringValue);
        serverEntity.EnumValue.Should().Be(source.EnumValue);
        serverEntity.DateOnlyValue.Should().Be(source.DateOnlyValue);
        serverEntity.TimeOnlyValue.Should().Be(source.TimeOnlyValue);
    }
}
