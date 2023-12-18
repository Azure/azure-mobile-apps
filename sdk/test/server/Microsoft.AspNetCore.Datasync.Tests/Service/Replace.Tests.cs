// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Replace_Tests : IClassFixture<ServiceApplicationFactory>
{
    private readonly HttpClient client;
    private readonly ServiceApplicationFactory factory;
    private readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    public Replace_Tests(ServiceApplicationFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    [Fact]
    public async Task Replace_Returns200()
    {
        ClientMovie existingMovie = new(factory.GetRandomMovie()) { Title = "New Title" };
        string content = JsonSerializer.Serialize(existingMovie);
        HttpResponseMessage response = await client.PutAsync($"{factory.MovieEndpoint}/{existingMovie.Id}", new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        clientMovie.Version.Should().NotBeNullOrEmpty().And.NotBe(existingMovie.Version);

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().BeEquivalentTo<IMovie>(inMemoryMovie);
        clientMovie.UpdatedAt.Should().Be(inMemoryMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(inMemoryMovie.Version));

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Theory]
    [InlineData("If-Match", null, HttpStatusCode.OK)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
    public async Task Replace_WithVersioning_Works(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
    {
        ClientMovie existingMovie = new(factory.GetRandomMovie());
        string etag = headerValue ?? $"\"{existingMovie.Version}\"";
        string content = JsonSerializer.Serialize(existingMovie);

        HttpRequestMessage request = new(HttpMethod.Put, $"{factory.MovieEndpoint}/{existingMovie.Id}") { Content = new StringContent(content, Encoding.UTF8, "application/json") };
        request.Headers.Add(headerName, etag);

        HttpResponseMessage response = await client.SendAsync(request);
        response.Should().HaveStatusCode(expectedStatusCode);

        if (expectedStatusCode == HttpStatusCode.OK)
        {
            ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
            clientMovie.Should().NotBeNull();
            clientMovie.Id.Should().Be(existingMovie.Id);
            clientMovie.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
            clientMovie.Version.Should().NotBeNullOrEmpty().And.NotBe(existingMovie.Version);

            InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
            clientMovie.Should().BeEquivalentTo<IMovie>(inMemoryMovie);
            clientMovie.UpdatedAt.Should().Be(inMemoryMovie.UpdatedAt);
            clientMovie.Version.Should().Be(Convert.ToBase64String(inMemoryMovie.Version));

            response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
            response.Headers.ETag?.IsWeak.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Replace_MissingId_Returns404()
    {
        ClientMovie existingMovie = new(factory.GetRandomMovie()) { Id = "missing" };
        string content = JsonSerializer.Serialize(existingMovie);
        HttpResponseMessage response = await client.PutAsync($"{factory.MovieEndpoint}/missing", new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Replace_NotSoftDeleted_Works()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        string content = JsonSerializer.Serialize(new ClientMovie(existingMovie) { Title = "New Title" });
        HttpResponseMessage response = await client.PutAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Replace_SoftDeleted_NotUndeleting_Returns410()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        string content = JsonSerializer.Serialize(new ClientMovie(existingMovie) { Title = "New Title" });
        HttpResponseMessage response = await client.PutAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Replace_SoftDeleted_Undeleting_Returns410()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        string content = JsonSerializer.Serialize(new ClientMovie(existingMovie) { Deleted = false });
        HttpResponseMessage response = await client.PutAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Replace_SoftDeleted_Undeleting_WithShowDeleted_Returns200()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        string content = JsonSerializer.Serialize(new ClientMovie(existingMovie) { Deleted = false });
        HttpResponseMessage response = await client.PutAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}?__includedeleted=true", new StringContent(content, Encoding.UTF8, "application/json"));
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().BeAfter(StartTime).And.BeBefore(DateTimeOffset.UtcNow);
        clientMovie.Version.Should().NotBeNullOrEmpty().And.NotBe(Convert.ToBase64String(existingMovie.Version));
        clientMovie.Deleted.Should().BeFalse();

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().BeEquivalentTo<IMovie>(inMemoryMovie);
        clientMovie.UpdatedAt.Should().Be(inMemoryMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(inMemoryMovie.Version));
        clientMovie.Deleted.Should().BeFalse();

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }
}
