// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Replace_Tests : ServiceTest, IClassFixture<ServiceApplicationFactory>
{
    public Replace_Tests(ServiceApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Replace_Returns200()
    {
        ClientMovie existingMovie = new(factory.GetRandomMovie()) { Title = "New Title" };
        HttpResponseMessage response = await client.PutAsJsonAsync($"{factory.MovieEndpoint}/{existingMovie.Id}", existingMovie, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>(serializerOptions);
        clientMovie.Should().NotBeNull().And.HaveChangedMetadata(existingMovie, StartTime).And.BeEquivalentTo<IMovie>(existingMovie);

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().HaveEquivalentMetadataTo(inMemoryMovie).And.BeEquivalentTo<IMovie>(inMemoryMovie);
        response.Headers.ETag.Should().BeETag($"\"{clientMovie.Version}\"");
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
            ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>(serializerOptions);
            clientMovie.Should().NotBeNull().And.HaveChangedMetadata(existingMovie, StartTime).And.BeEquivalentTo<IMovie>(existingMovie);

            InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
            clientMovie.Should().HaveEquivalentMetadataTo(inMemoryMovie).And.BeEquivalentTo<IMovie>(inMemoryMovie);
            response.Headers.ETag.Should().BeETag($"\"{clientMovie.Version}\"");
        }
    }

    [Fact]
    public async Task Replace_MissingId_Returns404()
    {
        ClientMovie existingMovie = new(factory.GetRandomMovie()) { Id = "missing" };
        HttpResponseMessage response = await client.PutAsJsonAsync($"{factory.MovieEndpoint}/missing", existingMovie, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Replace_NotSoftDeleted_Works()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        HttpResponseMessage response = await client.PutAsJsonAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", existingMovie, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>(serializerOptions);
        clientMovie.Should().NotBeNull().And.HaveChangedMetadata(existingMovie, StartTime).And.BeEquivalentTo<IMovie>(existingMovie);

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().HaveEquivalentMetadataTo(inMemoryMovie).And.BeEquivalentTo<IMovie>(inMemoryMovie);
        response.Headers.ETag.Should().BeETag($"\"{clientMovie.Version}\"");
    }

    [Fact]
    public async Task Replace_SoftDeleted_NotUndeleting_Returns410()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        HttpResponseMessage response = await client.PutAsJsonAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", existingMovie, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Replace_SoftDeleted_Undeleting_Returns410()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        HttpResponseMessage response = await client.PutAsJsonAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}", existingMovie, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Replace_SoftDeleted_Undeleting_WithShowDeleted_Returns200()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        existingMovie.Deleted = false;
        HttpResponseMessage response = await client.PutAsJsonAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}?__includedeleted=true", existingMovie, serializerOptions);
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>(serializerOptions);
        clientMovie.Should().NotBeNull().And.HaveChangedMetadata(existingMovie, StartTime).And.BeEquivalentTo<IMovie>(existingMovie);
        clientMovie.Deleted.Should().BeFalse();

        InMemoryMovie inMemoryMovie = factory.GetServerEntityById<InMemoryMovie>(clientMovie.Id);
        clientMovie.Should().HaveEquivalentMetadataTo(inMemoryMovie).And.BeEquivalentTo<IMovie>(inMemoryMovie);
        response.Headers.ETag.Should().BeETag($"\"{clientMovie.Version}\"");
    }
}
