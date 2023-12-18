using Datasync.Common;
using Datasync.Common.Models;
using System.Net;
using System.Text;

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Read_Tests : IClassFixture<ServiceApplicationFactory>
{
    private readonly HttpClient client;
    private readonly ServiceApplicationFactory factory;
    private readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    public Read_Tests(ServiceApplicationFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
    }

    [Fact]
    public async Task Read_Returns200()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        HttpResponseMessage response = await client.GetAsync($"{factory.MovieEndpoint}/{existingMovie.Id}");
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().Be(existingMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(existingMovie.Version));
        clientMovie.Deleted.Should().BeFalse();
        clientMovie.Should().BeEquivalentTo<IMovie>(existingMovie);

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Theory]
    [InlineData("If-Match", null, HttpStatusCode.OK)]
    [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("If-None-Match", null, HttpStatusCode.NotModified)]
    [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
    public async Task Read_WithVersioning_Works(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        HttpRequestMessage request = new(HttpMethod.Get, $"{factory.MovieEndpoint}/{existingMovie.Id}");
        request.Headers.Add(headerName, headerValue ?? $"\"{Convert.ToBase64String(existingMovie.Version)}\"");
        HttpResponseMessage response = await client.SendAsync(request);
        response.Should().HaveStatusCode(expectedStatusCode);

        if (expectedStatusCode == HttpStatusCode.OK)
        {
            ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
            clientMovie.Should().NotBeNull();
            clientMovie.Id.Should().Be(existingMovie.Id);
            clientMovie.UpdatedAt.Should().Be(existingMovie.UpdatedAt);
            clientMovie.Version.Should().Be(Convert.ToBase64String(existingMovie.Version));
            clientMovie.Deleted.Should().BeFalse();
            clientMovie.Should().BeEquivalentTo<IMovie>(existingMovie);

            response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
            response.Headers.ETag?.IsWeak.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Read_CanRoundtripTypes()
    {
        InMemoryKitchenSink storedEntity = new()
        {
            Id = Guid.NewGuid().ToString("N"),
            UpdatedAt = DateTimeOffset.Now.AddDays(-1),
            Version = Guid.NewGuid().ToByteArray(),
            Deleted = false,
            StringValue = "state=none",
            EnumValue = KitchenSinkState.Completed,
            DateOnlyValue = new DateOnly(2023, 12, 15),
            TimeOnlyValue = new TimeOnly(9, 52, 35)
        };
        factory.Store(storedEntity);

        HttpResponseMessage response = await client.GetAsync($"{factory.KitchenSinkEndpoint}/{storedEntity.Id}");
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientKitchenSink clientEntity = await response.Content.ReadFromJsonAsync<ClientKitchenSink>();
        clientEntity.Should().NotBeNull();
        clientEntity.Id.Should().Be(storedEntity.Id);
        clientEntity.UpdatedAt.Should().Be(storedEntity.UpdatedAt);
        clientEntity.Version.Should().Be(Convert.ToBase64String(storedEntity.Version));
        clientEntity.Deleted.Should().Be(storedEntity.Deleted);
        clientEntity.StringValue.Should().Be(storedEntity.StringValue);
        clientEntity.EnumValue.Should().Be(storedEntity.EnumValue);
        clientEntity.DateOnlyValue.Should().Be(storedEntity.DateOnlyValue);
        clientEntity.TimeOnlyValue.Should().Be(storedEntity.TimeOnlyValue);

        response.Headers.ETag?.Tag.Should().Be($"\"{clientEntity.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Fact]
    public async Task Read_MissingId_Returns404()
    {
        HttpResponseMessage response = await client.GetAsync($"{factory.MovieEndpoint}/missing");
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Read_SoftDeleted_NotDeleted_Returns200()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        HttpResponseMessage response = await client.GetAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}");
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().Be(existingMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(existingMovie.Version));
        clientMovie.Deleted.Should().BeFalse();
        clientMovie.Should().BeEquivalentTo<IMovie>(existingMovie);

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }

    [Fact]
    public async Task Read_SoftDeleted_Deleted_Returns410()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        HttpResponseMessage response = await client.GetAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}");
        response.Should().HaveStatusCode(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task Read_SoftDeleted_Deleted_WithIncludeDeleted_Returns200()
    {
        InMemoryMovie existingMovie = factory.GetRandomMovie();
        factory.SoftDelete(existingMovie);
        HttpResponseMessage response = await client.GetAsync($"{factory.SoftDeletedMovieEndpoint}/{existingMovie.Id}?__includedeleted=true");
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        ClientMovie clientMovie = await response.Content.ReadFromJsonAsync<ClientMovie>();
        clientMovie.Should().NotBeNull();
        clientMovie.Id.Should().Be(existingMovie.Id);
        clientMovie.UpdatedAt.Should().Be(existingMovie.UpdatedAt);
        clientMovie.Version.Should().Be(Convert.ToBase64String(existingMovie.Version));
        clientMovie.Deleted.Should().BeTrue();
        clientMovie.Should().BeEquivalentTo<IMovie>(existingMovie);

        response.Headers.ETag?.Tag.Should().Be($"\"{clientMovie.Version}\"");
        response.Headers.ETag?.IsWeak.Should().BeFalse();
    }
}
