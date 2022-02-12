// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A set of <see cref="HttpRequestMessage"/> extensions for handling requests with a fluent interface
    /// </summary>
    internal static class HttpMessageExtensions
    {
        /// <summary>
        /// List of all the telemetry features in the DatasyncFeatures set.
        /// </summary>
        private static readonly List<Tuple<DatasyncFeatures, string>> AllTelemetryFeatures
            = ((DatasyncFeatures[])Enum.GetValues(typeof(DatasyncFeatures))).Select(v => new Tuple<DatasyncFeatures, string>(v, EnumValueAttribute.GetValue(v))).ToList();


        /// <summary>
        /// Serialize the content for a request.
        /// </summary>
        /// <typeparam name="T">The type of the content</typeparam>
        /// <param name="request">The request to modify</param>
        /// <param name="content">The content</param>
        /// <param name="serializerOptions">The serializer options</param>
        /// <param name="mediaType">The media type</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithContent<T>(this HttpRequestMessage request, T content, JsonSerializerOptions serializerOptions, string mediaType = "application/json")
        {
            Validate.IsNotNull(content, nameof(content));
            Validate.IsNotNull(serializerOptions, nameof(serializerOptions));

            request.Content = new StringContent(JsonSerializer.Serialize(content, serializerOptions), Encoding.UTF8, mediaType);
            return request;
        }

        /// <summary>
        /// Adds a set of headers to the request.
        /// </summary>
        /// <param name="request">The request to modify</param>
        /// <param name="headers">the HTTP headers to add</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithHeaders(this HttpRequestMessage request, IDictionary<string, string> headers = null)
        {
            if (headers != null)
            {
                headers.ToList().ForEach(header => request.WithHeader(header.Key, header.Value));
            }
            return request;
        }

        /// <summary>
        /// Adds a single header to the correct place in the request.
        /// </summary>
        /// <param name="request">The request to modify</param>
        /// <param name="headerName">The headerName</param>
        /// <param name="headerValue">The headerValue</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithHeader(this HttpRequestMessage request, string headerName, string headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue))
            {
                // No-ope
                return request;
            }

            // There are a set of headers that apply to the content, and a set that apply to the body of the request.
            // We need to put the headers in the right place.
            if (headerName.StartsWith("Content-", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"Adjusting content headers is unsupported ('{headerName}')");
            }
            else
            {
                request.Headers.TryAddWithoutValidation(headerName, headerValue);
            }
            return request;
        }

        /// <summary>
        /// Adds the features header to the request.
        /// </summary>
        /// <param name="request">The request to modify</param>
        /// <param name="features">The features for this request</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithFeatureHeader(this HttpRequestMessage request, DatasyncFeatures features = DatasyncFeatures.None)
        {
            if (features != DatasyncFeatures.None)
            {
                var featureHeader = string.Join(",", AllTelemetryFeatures.Where(t => (features & t.Item1) == t.Item1).Select(t => t.Item2)).Trim(',');
                return request.WithHeader(ServiceHeaders.Features, featureHeader);
            }
            return request;
        }


        /// <summary>
        /// Returns true if the status code is a conflict.
        /// </summary>
        /// <param name="response">The response message</param>
        /// <returns>True if indicating a conflict</returns>
        internal static bool IsConflictStatusCode(this HttpResponseMessage response)
            => response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.PreconditionFailed;

        /// <summary>
        /// Reads the content of the <see cref="HttpContent"/> object as a byte array, with cancellation.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> object to process</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The content as a byte array, asynchronously</returns>
        internal static Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, CancellationToken token)
            => Task.Run(() => content.ReadAsByteArrayAsync(), token);
    }
}
