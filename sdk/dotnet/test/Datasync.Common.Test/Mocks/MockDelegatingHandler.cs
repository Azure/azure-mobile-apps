﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datasync.Common.Test.Mocks;

/// <summary>
/// A delegating handler for mocking responses.
/// </summary>
[ExcludeFromCodeCoverage]
public class MockDelegatingHandler : DelegatingHandler
{
    // For manipulating the request/response link - we need to surround it with a lock
    private SemaphoreSlim requestLock = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Used for serializing objects to be returned as responses.
    /// </summary>
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// List of requests that have been received.
    /// </summary>
    public List<HttpRequestMessage> Requests { get; } = new();

    /// <summary>
    /// List of responses that will be sent.
    /// </summary>
    public List<HttpResponseMessage> Responses { get; } = new();

    /// <summary>
    /// Handler for the request/response
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
    {
        await requestLock.WaitAsync(token).ConfigureAwait(false);
        Requests.Add(await CloneRequest(request).ConfigureAwait(false));
        requestLock.Release();
        return Responses[Requests.Count - 1];
    }

    /// <summary>
    /// Clone the <see cref="HttpRequestMessage"/>.
    /// </summary>
    public static async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
        request.Headers.ToList().ForEach(header => clone.Headers.TryAddWithoutValidation(header.Key, header.Value));

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            request.Content.Headers?.ToList().ForEach(header => clone.Content.Headers.Add(header.Key, header.Value));
        }

        return clone;
    }

    /// <summary>
    /// Adds a response with no payload to the list of responses.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="headers"></param>
    public void AddResponse(HttpStatusCode statusCode, IDictionary<string, string> headers = null)
        => Responses.Add(CreateResponse(statusCode, headers));

    /// <summary>
    /// Adds a response with a string payload.
    /// </summary>
    /// <param name="content">The JSON content</param>
    public void AddResponseContent(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var response = CreateResponse(statusCode);
        response.Content = new StringContent(content, Encoding.UTF8, "application/json");
        Responses.Add(response);
    }

    /// <summary>
    /// Adds a response with a payload to the list of responses.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="statusCode"></param>
    /// <param name="payload"></param>
    /// <param name="headers"></param>
    public void AddResponse<T>(HttpStatusCode statusCode, T payload, IDictionary<string, string> headers = null)
    {
        var response = CreateResponse(statusCode, headers);
        response.Content = new StringContent(JsonSerializer.Serialize(payload, serializerOptions), Encoding.UTF8, "application/json");
        Responses.Add(response);
    }

    /// <summary>
    /// Creates a <see cref="HttpResponseMessage"/> with no payload
    /// </summary>
    /// <param name="statusCode">The status code</param>
    /// <param name="headers">The headers (if any) to add</param>
    /// <returns>The <see cref="HttpResponseMessage"/></returns>
    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, IDictionary<string, string> headers = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (headers != null)
        {
            foreach (var kv in headers)
            {
                // Try to add to content first
                if (!response.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                {
                    response.Headers.Add(kv.Key, kv.Value);
                }
            }
        }
        return response;
    }
}