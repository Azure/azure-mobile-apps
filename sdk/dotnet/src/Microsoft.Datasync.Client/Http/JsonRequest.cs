// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// Models a HTTP request with a JSON payload.
    /// </summary>
    public class JsonRequest<T>
    {
        private Uri _requestUri = new("/", UriKind.Relative);
        private IDictionary<string, string> _headers = new Dictionary<string, string>();
        private IDictionary<string, string> _queryParams = new Dictionary<string, string>();

        /// <summary>
        /// A list of the headers that go in the content section.
        /// </summary>
        private static readonly string[] _contentHeaders =
        {
            "Allow",
            "Content-Disposition",
            "Content-Encoding",
            "Content-Language",
            "Content-Length",
            "Content-Location",
            "Content-MD5",
            "Content-Range",
            "Content-Type",
            "Expires",
            "Last-Modified"
        };

        /// <summary>
        /// The <see cref="HttpMethod"/> to use for this request.
        /// </summary>
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        /// <summary>
        /// The <see cref="Uri"/> (which may be relative) for this request.  If relative, it
        /// is relative to the endpoint defined by the client.
        /// </summary>
        public Uri RequestUri
        {
            get => _requestUri;
            set => _requestUri = value ?? throw new ArgumentNullException(nameof(RequestUri));
        }

        /// <summary>
        /// A list of query parameters to be appended to the <c>RequestUri</c>.
        /// </summary>
        public IDictionary<string, string> QueryParameters
        {
            get => _queryParams;
            set => _queryParams = value ?? throw new ArgumentNullException(nameof(QueryParameters));
        }

        /// <summary>
        /// A list of headers to send along with the request.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get => _headers;
            set => _headers = value ?? throw new ArgumentNullException(nameof(Headers));
        }

        /// <summary>
        /// The payload to be sent in the request.
        /// </summary>
        public T? Payload { get; set; }

        /// <summary>
        /// Constructs the request Uri for this request based on the provided endpoint, the RequestUri
        /// (which may be absolute or relative) and the query parameters.
        /// </summary>
        /// <param name="endpoint">The endpoint that this request may be relative to</param>
        /// <returns>An absolute Uri for the request</returns>
        internal Uri ToAbsoluteUri(Uri endpoint)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }
            else if (!endpoint.IsAbsoluteUri)
            {
                throw new ArgumentException("Endpoint must be absolute", nameof(endpoint));
            }

            var builder = new UriBuilder(RequestUri.IsAbsoluteUri ? RequestUri : new Uri(endpoint, RequestUri));
            var queryParams = QueryParameters.ToList().OrderBy(qp => qp.Key).Select(qp => $"{HttpUtility.UrlEncode(qp.Key)}={HttpUtility.UrlEncode(qp.Value)}");
            builder.Query = string.Join("&", queryParams);
            return builder.Uri;
        }

        /// <summary>
        /// Returns the content type for the request.
        /// </summary>
        /// <param name="defaultContentType">The default content type, if one is not provided</param>
        /// <returns>The content type to use</returns>
        internal string GetContentType(string defaultContentType)
            => Headers.TryGetValue("Content-Type", out string contentType) ? contentType : defaultContentType;

        /// <summary>
        /// Returns true if the provided header name is a content header, and thus belongs on the <see cref="HttpContent.Headers"/> list.
        /// </summary>
        /// <param name="headerName">The header name to check</param>
        /// <returns>True if it is a content header</returns>
        internal bool IsContentHeader(string headerName)
            => _contentHeaders.Any(v => v.Equals(headerName, StringComparison.InvariantCultureIgnoreCase));

        /// <summary>
        /// Obtains a list of the headers that were provided that are content headers.
        /// </summary>
        /// <returns>A filtered list of headers</returns>
        internal List<KeyValuePair<string, string>> GetContentHeaders()
            => Headers.Where(hdr => IsContentHeader(hdr.Key)).ToList();

        /// <summary>
        /// Obtains a list of the headers that were provided that are not content headers.
        /// </summary>
        /// <returns>A filtered list of headers</returns>
        internal List<KeyValuePair<string, string>> GetNormalHeaders()
            => Headers.Where(hdr => !IsContentHeader(hdr.Key)).ToList();

        /// <summary>
        /// Converts this <see cref="JsonRequest"/> to a <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="endpoint">The absolute endpoint for this client</param>
        /// <param name="serializerOptions">The serializer options for the request</param>
        /// <returns>The equivalent <see cref="HttpRequestMessage"/></returns>
        internal HttpRequestMessage ToHttpRequestMessage(Uri endpoint, JsonSerializerOptions serializerOptions)
        {
            var request = new HttpRequestMessage() { Method = Method, RequestUri = ToAbsoluteUri(endpoint) };
            if (Payload != null)
            {
                request.Content = new StringContent(JsonSerializer.Serialize(Payload, serializerOptions), Encoding.UTF8, GetContentType("application/json"));
                GetContentHeaders().ForEach(hdr =>
                {
                    request.Content.Headers.Remove(hdr.Key);
                    request.Content.Headers.Add(hdr.Key, hdr.Value);
                });
            }
            GetNormalHeaders().ForEach(hdr => request.Headers.Add(hdr.Key, hdr.Value));
            return request;
        }
    }

    /// <summary>
    /// A <see cref="JsonRequest{T}"/> for non-payloads where there is no type.
    /// </summary>
    public class JsonRequest : JsonRequest<string>
    {
        /// <summary>
        /// An alternate <see cref="ToHttpRequestMessage(Uri,JsonSerializerOptions)"/> implementation that doesn't
        /// require serialization.
        /// </summary>
        /// <param name="endpoint">The absolute endpoint for this client</param>
        /// <returns>The equivalent <see cref="HttpRequestMessage"/></returns>
        internal HttpRequestMessage ToHttpRequestMessage(Uri endpoint)
        {
            var request = new HttpRequestMessage() { Method = Method, RequestUri = ToAbsoluteUri(endpoint) };
            if (Payload != null)
            {
                var contentTypeHeader = Headers.FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.InvariantCultureIgnoreCase)).Value ?? "text/plain";
                request.Content = new StringContent(Payload, Encoding.UTF8, GetContentType("text/plain"));
                GetContentHeaders().ForEach(hdr =>
                {
                    request.Content.Headers.Remove(hdr.Key);
                    request.Content.Headers.Add(hdr.Key, hdr.Value);
                });
            }
            GetNormalHeaders().ForEach(hdr => request.Headers.Add(hdr.Key, hdr.Value));
            return request;
        }
    }
}
