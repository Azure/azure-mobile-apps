// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// Definition of a basic HTTP request.
    /// </summary>
    internal class ServiceRequest
    {
        /// <summary>
        /// The HTTP PATCH method.
        /// </summary>
        internal static readonly HttpMethod PATCH = new("PATCH");

        /// <summary>
        /// The HTTP method to use to request the resource.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// The JSON content to send as the payload.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// If <c>true</c>, ensure that the response has content.
        /// </summary>
        public bool EnsureResponseContent { get; set; } = true;

        /// <summary>
        /// A set of additional headers to send with the request.
        /// </summary>
        public IDictionary<string, string> RequestHeaders { get; set; }

        /// <summary>
        /// The URI path and query of the resource to request (relative to the base endpoint).
        /// </summary>
        public string UriPathAndQuery { get; set; }

        /// <summary>
        /// Converts the <see cref="ServiceRequest"/> to a <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="baseUri">If provided, generate an absolute URI for the request.</param>
        /// <returns>The equivalent <see cref="HttpRequestMessage"/> to this service request.</returns>
        internal HttpRequestMessage ToHttpRequestMessage(Uri baseUri = null)
        {
            Arguments.IsNotNull(Method, nameof(Method));
            Arguments.IsNotNullOrWhitespace(UriPathAndQuery, nameof(UriPathAndQuery));

            // If UriPathAndQuery starts with http:// or https://, then assume it is absolute,
            // and just use it.  Otherwise construct a new one.
            var uri = UriPathAndQuery.StartsWith("http://") || UriPathAndQuery.StartsWith("https://") ? new Uri(UriPathAndQuery)
                : baseUri != null ? new Uri(baseUri, UriPathAndQuery) : new Uri(UriPathAndQuery, UriKind.Relative);
            var request = new HttpRequestMessage(Method, uri);
            if (RequestHeaders?.Count > 0)
            {
                foreach (var header in RequestHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            if (!string.IsNullOrWhiteSpace(Content))
            {
                request.Content = new StringContent(Content, Encoding.UTF8, "application/json");
            }
            return request;
        }
    }
}
