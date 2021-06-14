// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// A terminating <see cref="HttpMessageHandler"/> that allows us to bypass the
    /// HTTP request/response and insert our own.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MockMessageHandler : DelegatingHandler
    {
        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// The list of requests that have been received.
        /// </summary>
        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        /// <summary>
        /// The list of responses that will be sent.
        /// </summary>
        public List<HttpResponseMessage> Responses { get; } = new List<HttpResponseMessage>();

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
        {
            Requests.Add(await Clone(request));
            return Responses[Requests.Count - 1];
        }

        /// <summary>
        /// Creates a cone of the provided request
        /// </summary>
        public async Task<HttpRequestMessage> Clone(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
            request.Headers.ToList().ForEach(header => clone.Headers.TryAddWithoutValidation(header.Key, header.Value));
            if (request.Content != null)
            {
                var memoryStream = new MemoryStream();
                await request.Content.CopyToAsync(memoryStream).ConfigureAwait(false);
                memoryStream.Position = 0;
                clone.Content = new StreamContent(memoryStream);
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
        {
            var response = new HttpResponseMessage(statusCode);
            headers?.ToList().ForEach(header => response.Headers.Add(header.Key, header.Value));
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
            var response = new HttpResponseMessage(statusCode);
            response.Content = new StringContent(JsonSerializer.Serialize(payload, serializerOptions), Encoding.UTF8, "application/json");
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    // This will overwrite the content-type if it is set in the headers.
                    if (!response.Content.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                    {
                        response.Headers.Add(kv.Key, kv.Value);
                    }
                }
            }
            Responses.Add(response);
        }
    }
}
