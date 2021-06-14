// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// Models a HTTP response with a JSON payload.
    /// </summary>
    public class JsonResponse
    {
        private readonly Dictionary<string, string[]> _headers = new();

        /// <summary>
        /// Creates a new <see cref="JsonResponse"/> from a <see cref="HttpResponseMessage"/>,
        /// allowing the response message to be disposed.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/></param>
        internal JsonResponse(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            ReasonPhrase = response.ReasonPhrase;
            StatusCode = (int)response.StatusCode;
            response.Headers.ToList().ForEach(hdr => _headers.Add(hdr.Key, hdr.Value.ToArray()));
        }

        /// <summary>
        /// Reads the content asynchronously and then stores it for later use.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/></param>
        /// <returns>The <see cref="JsonResponse{T}"/> for chaining</returns>
        internal async Task<JsonResponse> ReadContentAsync(HttpContent? content)
        {
            if (content != null)
            {
                content.Headers.ToList().ForEach(hdr => _headers.Add(hdr.Key, hdr.Value.ToArray()));
                Content = await content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            else
            {
                Content = Array.Empty<byte>();
            }
            return this;
        }

        /// <summary>
        /// The content that was sent along with the response, in it's raw form.
        /// </summary>
        public byte[] Content { get; private set; } = Array.Empty<byte>();

        public bool HasContent { get => Content.Length > 0; }

        /// <summary>
        /// The list of headers from the response.
        /// </summary>
        public IReadOnlyDictionary<string, string[]> Headers { get => _headers; }

        /// <summary>
        /// True if the response indicates success.
        /// </summary>
        public bool IsSuccessStatusCode { get => StatusCode >= 200 && StatusCode <= 299; }

        /// <summary>
        /// The reason phrase provided by the service.
        /// </summary>
        public string? ReasonPhrase { get; }

        /// <summary>
        /// The numeric status code from the service.
        /// </summary>
        public int StatusCode { get; }
    }

    /// <summary>
    /// Models a HTTP response with a JSON payload of a specific type.
    /// </summary>
    public class JsonResponse<T> : JsonResponse
    {
        /// <inheritdoc />
        internal JsonResponse(HttpResponseMessage response) : base(response)
        {
        }

        /// <summary>
        /// Deserializes the content from the service into the type of the <see cref="JsonResponse{T}"/>.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> from the response.</param>
        /// <param name="deserializerOptions">Any <see cref="JsonSerializerOptions"/> to use for deserializing content</param>
        /// <returns>The <see cref="JsonResponse{T}"/> for chaining</returns>
        internal async Task<JsonResponse<T>> DeserializeContentAsync(HttpContent? content, JsonSerializerOptions deserializerOptions)
        {
            await ReadContentAsync(content).ConfigureAwait(false);
            if (HasContent && !HasValue)
            {
                Value = JsonSerializer.Deserialize<T>(Content, deserializerOptions);
            }
            return this;
        }

        /// <summary>
        /// True if the <see cref="Value"/> has been set.
        /// </summary>
        public bool HasValue { get => Value != null; }

        /// <summary>
        /// The value of the deserialized content
        /// </summary>
        public T? Value { get; private set; }

        /// <summary>
        /// Returns the value of this <see cref="JsonResponse{T}"/>.  Implicit operator allows you
        /// to assign the value directly from any method returning a <see cref="JsonResponse{T}"/>.
        /// </summary>
        /// <param name="response"></param>
        public static implicit operator T?(JsonResponse<T> response) => response.Value;
    }
}
