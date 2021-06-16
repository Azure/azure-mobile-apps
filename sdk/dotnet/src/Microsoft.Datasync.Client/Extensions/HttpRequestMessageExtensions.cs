// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Internal;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Microsoft.Datasync.Client.Extensions
{
    /// <summary>
    /// A set of methods for emulating a fluent API for <see cref="HttpRequestMessage"/>.
    /// </summary>
    internal static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Sets the body of the request to the serialized content.
        /// </summary>
        /// <typeparam name="T">The type of the content</typeparam>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to modify</param>
        /// <param name="content">The content to use for the payload of the request</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for serializing the payload</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithJsonPayload<T>(this HttpRequestMessage request, T content, JsonSerializerOptions options) where T : notnull
            => WithJsonPayload(request, content, "application/json", options);

        /// <summary>
        /// Sets the body of the request to the serialized content.
        /// </summary>
        /// <typeparam name="T">The type of the content</typeparam>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to modify</param>
        /// <param name="content">The content to use for the payload of the request</param>
        /// <param name="contentType">The MIME type for the request</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use for serializing the payload</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithJsonPayload<T>(this HttpRequestMessage request, T content, string contentType, JsonSerializerOptions options) where T : notnull
        {
            Validate.IsNotNull(content, nameof(content));
            Validate.IsNotNullOrWhitespace(contentType, nameof(contentType));
            Validate.IsNotNull(options, nameof(options));

            request.Content = new StringContent(JsonSerializer.Serialize(content, options), Encoding.UTF8, contentType.Trim());
            return request;
        }

        /// <summary>
        /// Adds a precondition to the HTTP request headers.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to modify.</param>
        /// <param name="condition">An optional <see cref="HttpCondition"/> holding the precondition.</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithPrecondition(this HttpRequestMessage request, HttpCondition? condition = null)
        {
            condition?.AddToHeaders(request.Headers);
            return request;
        }

        /// <summary>
        /// Sets the query string for the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to modify</param>
        /// <param name="queryString">The new query string</param>
        /// <returns>The modified request</returns>
        internal static HttpRequestMessage WithQueryString(this HttpRequestMessage request, string queryString)
        {
            request.RequestUri = new UriBuilder(request.RequestUri).WithQuery(queryString).Uri;
            return request;
        }
    }
}
