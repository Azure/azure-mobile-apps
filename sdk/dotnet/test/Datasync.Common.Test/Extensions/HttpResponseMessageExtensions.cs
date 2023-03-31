// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Datasync.Common.Test;

[ExcludeFromCodeCoverage]
public static class HttpResponseMessageExtensions
{
    /// <summary>
    /// The client uses System.Text.Json to deserialize content from the server.  This
    /// is the required <see cref="JsonSerializerOptions"/> for deserialization.
    /// </summary>
    private static JsonSerializerOptions SerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Deserialize the content asynchronously.
    /// </summary>
    /// <typeparam name="T">The expected type of the response</typeparam>
    /// <param name="response">The response object</param>
    /// <returns>The object</returns>
    public static async Task<T> DeserializeContentAsync<T>(this HttpResponseMessage response) where T : class
    {
        var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(payload, SerializerOptions);
    }

    /// <summary>
    /// Deserialize the content synchronously.
    /// </summary>
    /// <typeparam name="T">The expected type of the response</typeparam>
    /// <param name="response">The response object</param>
    /// <returns>The object</returns>
    public static T DeserializeContent<T>(this HttpResponseMessage response) where T : class
        => JsonSerializer.Deserialize<T>(response.Content.ReadAsStringAsync().Result, SerializerOptions);
}
