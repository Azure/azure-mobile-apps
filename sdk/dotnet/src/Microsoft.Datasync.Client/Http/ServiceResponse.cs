// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A representation of the HTTP response, with an in-memory copy of the payload.
    /// </summary>
    public class ServiceResponse
    {
        protected ServiceResponse(HttpResponseMessage response)
        {
            Validate.IsNotNull(response, nameof(response));

            var headers = new Dictionary<string, IEnumerable<string>>();
            response.Headers.ToList().ForEach(hdr => headers.Add(hdr.Key, hdr.Value.ToArray()));
            response.Content.Headers.ToList().ForEach(hdr => headers.Add(hdr.Key, hdr.Value.ToArray()));

            Content = Array.Empty<byte>();
            Headers = headers;
            ReasonPhrase = response.ReasonPhrase;
            StatusCode = (int)response.StatusCode;
            Version = new Version(response.Version.ToString());
        }

        /// <summary>
        /// Creates a new <see cref="HttpResponse"/> from an existing <see cref="HttpResponseMessage"/>.  After execution,
        /// the <see cref="HttpResponseMessage"/> can be disposed.
        /// </summary>
        /// <param name="message">The source message</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="HttpMessage"/></returns>
        internal static async Task<ServiceResponse> FromResponseAsync(HttpResponseMessage message, CancellationToken token = default)
        {
            Validate.IsNotNull(message, nameof(message));

            var response = new ServiceResponse(message)
            {
                Content = await message.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false)
            };
            return response;
        }

        /// <summary>
        /// Creates a new <see cref="HttpResponse{T}"/> from an existing <see cref="HttpResponseMessage"/>.  After execution,
        /// the <see cref="HttpResponseMessage"/> can be disposed.
        /// </summary>
        /// <typeparam name="T">The expected type of the payload</typeparam>
        /// <param name="message">The source message</param>
        /// <param name="deserializerOptions">The <see cref="JsonSerializerOptions"/> to use during deserialization</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="HttpMessage{T}"/></returns>
        internal static Task<ServiceResponse<T>> FromResponseAsync<T>(HttpResponseMessage message, JsonSerializerOptions deserializerOptions, CancellationToken token = default)
            => ServiceResponse<T>.FromResponseAsync(message, deserializerOptions, token);

        /// <summary>
        /// The payload of the response.
        /// </summary>
        public byte[] Content { get; protected set; }

        /// <summary>
        /// True if a payload was sent from the datasync service.
        /// </summary>
        public bool HasContent { get => Content.Length > 0; }

        /// <summary>
        /// True if the HTTP response indicates a conflict.
        /// </summary>
        public bool IsConflictStatusCode { get => StatusCode == 409 || StatusCode == 412; }
        /// <summary>
        /// True if the HTTP response was successful.
        /// </summary>
        public bool IsSuccessStatusCode { get => StatusCode >= 200 && StatusCode <= 299; }

        /// <summary>
        /// The consolidated list of all headers returned in the response.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

        /// <summary>
        /// The reason phrase which typically is sent by servers together with the status code.
        /// </summary>
        public string ReasonPhrase { get; }

        /// <summary>
        /// The HTTP StatusCode for the response.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// The HTTP message version.
        /// </summary>
        public Version Version { get; }
    }

    /// <summary>
    /// A typed version of the <see cref="ServiceResponse"/> object that deserializes expected JSON content
    /// into the expected type.
    /// </summary>
    /// <typeparam name="T">The expected type of the deserialized JSON payload</typeparam>
    public class ServiceResponse<T> : ServiceResponse
    {
        /// <summary>
        /// Creates a new <see cref="HttpResponse{T}"/> object based on a <see cref="HttpResponseMessage"/>
        /// </summary>
        /// <param name="message">The source message</param>
        protected ServiceResponse(HttpResponseMessage message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpResponse{T}"/> from an existing <see cref="HttpResponseMessage"/>.  After execution,
        /// the <see cref="HttpResponseMessage"/> can be disposed.
        /// </summary>
        /// <param name="message">The source message</param>
        /// <param name="deserializerOptions">The <see cref="JsonSerializerOptions"/> to use during deserialization</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The <see cref="HttpMessage{T}"/></returns>
        internal static async Task<ServiceResponse<T>> FromResponseAsync(HttpResponseMessage message, JsonSerializerOptions deserializerOptions, CancellationToken token = default)
        {
            Validate.IsNotNull(message, nameof(message));
            Validate.IsNotNull(deserializerOptions, nameof(deserializerOptions));

            var response = new ServiceResponse<T>(message)
            {
                Content = await message.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false)
            };
            if (response.HasContent)
            {
                response.Value = JsonSerializer.Deserialize<T>(response.Content, deserializerOptions);
            }
            return response;
        }

        /// <summary>
        /// True if the payload contained JSON content with the provided type and it could be deserialized.
        /// </summary>
        public bool HasValue { get => Value != null; }

        /// <summary>
        /// The deserialized content of the payload.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// An implicit operator, allowing you to assign a result directly to T without further processing.
        /// </summary>
        /// <param name="response"></param>
        public static implicit operator T(ServiceResponse<T> response) => response.Value;
    }
}
