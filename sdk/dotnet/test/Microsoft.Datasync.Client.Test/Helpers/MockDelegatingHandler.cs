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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// A DelegatingHandler for mocking responses.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MockDelegatingHandler : DelegatingHandler
    {
        private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // Need this because of encoding the NextLink
        };

        /// <summary>
        /// The list of requests that have been received.
        /// </summary>
        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

        /// <summary>
        /// The list of responses that will be sent.
        /// </summary>
        public List<HttpResponseMessage> Responses { get; } = new List<HttpResponseMessage>();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
        {
            Requests.Add(await Clone(request));
            return Responses[Requests.Count - 1];
        }

        /// <summary>
        /// Creates a clone of the provided request
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        public async Task<HttpRequestMessage> Clone(HttpRequestMessage request)
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
        /// Adds a response with a payload to the list of responses.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statusCode"></param>
        /// <param name="payload"></param>
        /// <param name="headers"></param>
        public void AddResponse<T>(HttpStatusCode statusCode, T payload, IDictionary<string, string> headers = null)
            => Responses.Add(CreateResponse(statusCode, payload, headers));

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

        /// <summary>
        /// Creates a <see cref="HttpResponseMessage"/> with the provided payload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statusCode"></param>
        /// <param name="payload"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, T payload, IDictionary<string, string> headers = null)
        {
            var response = CreateResponse(statusCode, headers);
            response.Content = new StringContent(JsonSerializer.Serialize(payload, serializerOptions), Encoding.UTF8, "application/json");
            return response;
        }
    }
}
