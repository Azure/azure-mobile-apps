// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Datasync.Common.Test.Service;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace Datasync.Common.Test;

/// <summary>
/// A set of extension methods that make it easier to send unit tests to the test server.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TestServerExtensions
{
    private const string ZumoVersionHeader = "ZUMO-API-VERSION";

    /// <summary>
    /// The URI to assume for the <see cref="TestServer"/> instance.
    /// </summary>
    private static Uri ServerUri { get; } = new Uri("https://localhost");

    /// <summary>
    /// The client uses System.Text.Json to deserialize content from the server.  This
    /// is the required <see cref="JsonSerializerOptions"/> for deserialization.
    /// </summary>
    private static JsonSerializerOptions SerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Returns the movie in the database.
    /// </summary>
    /// <param name="server">The server to retrieve the context from</param>
    /// <param name="id">The ID of the movie</param>
    /// <returns>The movie, or null if it doesn't exist.</returns>
    public static EFMovie GetMovieById(this TestServer server, string id)
    {
        using var scope = server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        return context.GetMovieById(id);
    }

    /// <summary>
    /// Soft-deletes a set of movies.
    /// </summary>
    /// <param name="server">The server to use for the action</param>
    /// <param name="predicate">A predicate to find the movies to soft-delete</param>
    /// <returns></returns>
    public static Task SoftDeleteMoviesAsync(this TestServer server, Expression<Func<EFMovie, bool>> predicate)
    {
        using var scope = server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        return context.SoftDeleteMoviesAsync(predicate);
    }

    /// <summary>
    /// Returns a current count of the movies.
    /// </summary>
    /// <param name="server">The server to query</param>
    /// <returns></returns>
    public static int GetMovieCount(this TestServer server)
    {
        using var scope = server.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        return context.Movies.Count();
    }

    /// <summary>
    /// Sends a request to the remote server with no body content.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="method">The <see cref="HttpMethod"/> to use</param>
    /// <param name="relativeUri">The relative Uri of the request</param>
    /// <param name="headers">Any additional headers to send</param>
    /// <returns>The response from the server</returns>
    public static Task<HttpResponseMessage> SendRequest(this TestServer server, HttpMethod method, string relativeUri, Dictionary<string, string> headers = null)
    {
        var client = server.CreateClient();
        var request = new HttpRequestMessage { Method = method, RequestUri = new Uri(ServerUri, relativeUri) };
        if (headers != null)
        {
            foreach (var kv in headers)
            {
                request.Headers.Add(kv.Key, kv.Value);
            }
        }

        // Auto-add the X-ZUMO-Version header if we don't already have one.
        if (!request.Headers.Contains(ZumoVersionHeader))
        {
            request.Headers.Add(ZumoVersionHeader, "3.0.0");
        }

        return client.SendAsync(request);
    }

    /// <summary>
    /// Sends a request to the remote server with no body content.
    /// </summary>
    /// <typeparam name="T">The type of the content</typeparam>
    /// <param name="server"></param>
    /// <param name="method">The <see cref="HttpMethod"/> to use</param>
    /// <param name="relativeUri">The relative Uri of the request</param>
    /// <param name="content">The payload of the request</param>
    /// <param name="headers">Any additional headers to send</param>
    /// <returns>The response from the server</returns>
    public static Task<HttpResponseMessage> SendRequest<T>(this TestServer server, HttpMethod method, string relativeUri, T content, Dictionary<string, string> headers = null) where T : class
        => SendRequest<T>(server, method, relativeUri, content, "application/json", headers);

    /// <summary>
    /// Sends a request to the remote server with no body content.
    /// </summary>
    /// <typeparam name="T">The type of the content</typeparam>
    /// <param name="server"></param>
    /// <param name="method">The <see cref="HttpMethod"/> to use</param>
    /// <param name="relativeUri">The relative Uri of the request</param>
    /// <param name="content">The payload of the request</param>
    /// <param name="contentType">The MIME content type of the request</param>
    /// <param name="headers">Any additional headers to send</param>
    /// <returns>The response from the server</returns>
    public static Task<HttpResponseMessage> SendRequest<T>(this TestServer server, HttpMethod method, string relativeUri, T content, string contentType, Dictionary<string, string> headers = null) where T : class
    {
        var client = server.CreateClient();
        var request = new HttpRequestMessage { Method = method, RequestUri = new Uri(ServerUri, relativeUri) };
        if (headers != null)
        {
            foreach (var kv in headers)
            {
                request.Headers.Add(kv.Key, kv.Value);
            }
        }

        // Auto-add the X-ZUMO-Version header if we don't already have one.
        if (!request.Headers.Contains(ZumoVersionHeader))
        {
            request.Headers.Add(ZumoVersionHeader, "3.0.0");
        }

        var payload = JsonSerializer.Serialize(content, SerializerOptions);
        request.Content = new StringContent(payload, Encoding.UTF8, contentType);
        return client.SendAsync(request);
    }

    /// <summary>
    /// Send a patch request to the remote server.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="relativeUri"></param>
    /// <param name="content"></param>
    /// <param name="contentType"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendPatch(this TestServer server, string relativeUri, IEnumerable<PatchOperation> content, string contentType = "application/json-patch+json", Dictionary<string, string> headers = null)
    {
        var client = server.CreateClient();
        var request = new HttpRequestMessage { Method = HttpMethod.Patch, RequestUri = new Uri(ServerUri, relativeUri) };
        if (headers != null)
        {
            foreach (var kv in headers)
            {
                request.Headers.Add(kv.Key, kv.Value);
            }
        }

        // Auto-add the X-ZUMO-Version header if we don't already have one.
        if (!request.Headers.Contains(ZumoVersionHeader))
        {
            request.Headers.Add(ZumoVersionHeader, "3.0.0");
        }

        var payload = JsonSerializer.Serialize(content, SerializerOptions);
        request.Content = new StringContent(payload, Encoding.UTF8, contentType);
        return client.SendAsync(request);
    }

    /// <summary>
    /// Alternate form of the patch request with headers and no content type.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="relativeUri"></param>
    /// <param name="content"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendPatch(this TestServer server, string relativeUri, IEnumerable<PatchOperation> content, Dictionary<string, string> headers = null)
        => SendPatch(server, relativeUri, content, "application/json-patch+json", headers);

    /// <summary>
    /// Alternate form of the patch request with no headers or content type.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="relativeUri"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static Task<HttpResponseMessage> SendPatch(this TestServer server, string relativeUri, IEnumerable<PatchOperation> content)
        => SendPatch(server, relativeUri, content, "application/json-patch+json", null);
}
