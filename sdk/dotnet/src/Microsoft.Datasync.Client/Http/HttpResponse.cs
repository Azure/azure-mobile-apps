// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A read-only encapsulation of the <see cref="HttpResponseMessage"/> such that you
    /// can dispose the message, leaving just the content.
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// For unit-testing and to prevent instantiation.
        /// </summary>
        protected HttpResponse()
        {
            Content = Array.Empty<byte>();
            StatusCode = HttpStatusCode.NonAuthoritativeInformation;
            Headers = new Dictionary<string, IEnumerable<string>>();
            Version = Version.Parse("0.0.0");
        }

        /// <summary>
        /// Creates a new <see cref="HttpResponse"/> with the main fields filled in from
        /// a <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="message">The source message</param>
        protected HttpResponse(HttpResponseMessage message)
        {
            ReasonPhrase = message.ReasonPhrase;
            StatusCode = message.StatusCode;
            Version = message.Version;
            Content = Array.Empty<byte>();

            // Construct the full headers list by copying the data - we don't want
            // the data to disappear once the response message is disposed.
            Dictionary<string, IEnumerable<string>> headers = new();
            message.Headers.ToList().ForEach(header => headers.Add(header.Key, header.Value.ToArray()));
            message.Content.Headers.ToList().ForEach(header => headers.Add(header.Key, header.Value.ToArray()));
            Headers = headers;
        }

        /// <summary>
        /// The UTF-8 content from the request.
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// Returns true if there is content in the response.
        /// </summary>
        public bool HasContent { get => Content.Length > 0; }

        /// <summary>
        /// The collection of HTTP response headers.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

        /// <summary>
        /// True if the request indicates a service conflict.
        /// </summary>
        public bool IsConflictStatusCode { get => StatusCode == HttpStatusCode.Conflict || StatusCode == HttpStatusCode.PreconditionFailed; }

        /// <summary>
        /// True if the request is a success.
        /// </summary>
        public bool IsSuccessStatusCode { get => (int)StatusCode >= 200 && (int)StatusCode <= 299; }

        /// <summary>
        /// The reason phrase which typically is sent by servers together with the status code.
        /// </summary>
        public string? ReasonPhrase { get; }

        /// <summary>
        /// The status code of the HTTP response
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The HTTP message version
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Read the content from the service response.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> object</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The current <see cref="HttpResponse"/> for chaining</returns>
        protected async Task<HttpResponse> ReadContentAsync(HttpContent content, CancellationToken token = default)
        {
            Content = await content.ReadAsByteArrayAsync(token).ConfigureAwait(false);
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="HttpResponse"/> and reads the content from the server.
        /// </summary>
        /// <param name="message">The <see cref="HttpResponseMessage"/> holding the response from the server.</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="HttpResponse"/> object</returns>
        internal static async Task<HttpResponse> FromResponseAsync(HttpResponseMessage message, CancellationToken token = default)
            => await new HttpResponse(message).ReadContentAsync(message.Content, token).ConfigureAwait(false);
    }

    /// <summary>
    /// A read-only encapsulation of the <see cref="HttpResponseMessage"/> such that you
    /// can dispose the message, leaving just the deserialized content.
    /// </summary>
    /// <typeparam name="T">The expected type of the payload</typeparam>
    public class HttpResponse<T> : HttpResponse
    {
        /// <summary>
        /// For unit-testing and to prevent instantiation.
        /// </summary>
        protected HttpResponse() : base()
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpResponse{Task}"/> with the main fields filled in from
        /// a <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="message">The source message</param>
        public HttpResponse(HttpResponseMessage message) : base(message)
        {
        }

        /// <summary>
        /// True if this response has a value.
        /// </summary>
        public bool HasValue { get => Value != null; }

        /// <summary>
        /// The value of the response (deserialized)
        /// </summary>
        public T? Value { get; private set; }

        /// <summary>
        /// Returns the value of this <see cref="HttpResponse{T}"/> object.  This is useful
        /// for assigning the result of an operation directly to a T variable.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse{T}"/> object</param>
        public static implicit operator T?(HttpResponse<T> response) => response.Value;

        /// <summary>
        /// Read and deserialize the content from the service.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> object</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for deserialization</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The current <see cref="HttpResponse{T}"/> for chaining</returns>
        internal async Task<HttpResponse<T>> DeserializeContentAsync(HttpContent content, JsonSerializerOptions options, CancellationToken token = default)
        {
            await ReadContentAsync(content, token).ConfigureAwait(false);
            if (HasContent)
            {
                Value = JsonSerializer.Deserialize<T>(Content, options);
            }
            return this;
        }

        /// <summary>
        /// Creates a new <see cref="HttpResponse{T}"/> and reads the content from the server, deserializing it into a value.
        /// </summary>
        /// <param name="message">The <see cref="HttpResponseMessage"/> holding the response from the server.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when deserializing content</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A new <see cref="HttpResponse{T}"/> object</returns>
        internal static async Task<HttpResponse<T>> FromResponseAsync(HttpResponseMessage message, JsonSerializerOptions options, CancellationToken token = default)
            => await new HttpResponse<T>(message).DeserializeContentAsync(message.Content, options, token).ConfigureAwait(false);
    }
}
