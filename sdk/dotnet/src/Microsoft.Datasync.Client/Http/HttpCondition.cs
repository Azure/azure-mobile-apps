// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Internal;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Represents a HTTP precondition, according to RFC 7232.  For more information,
    /// see <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Conditional_requests"/>
    /// </summary>
    /// <remarks>
    /// The datasync service only support version checks via ETag matching.
    /// </remarks>
    /// <seealso href="https://datatracker.ietf.org/doc/html/rfc7232"/>
    public sealed class HttpCondition
    {
        private const string IfMatchHeader = "If-Match";
        private const string IfNoneMatchHeader = "If-None-Match";
        private const string Any = "*";

        /// <summary>
        /// Creates a new <see cref="HttpCondition"/> based on a byte array version.
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <param name="value">The value of the header</param>
        private HttpCondition(string name, byte[] value)
        {
            Validate.IsPreconditionHeader(name, nameof(name));
            Validate.IsNotNullOrEmpty(value, nameof(value));

            HeaderName = name;
            HeaderValue = JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Creates a new <see cref="HttpCondition"/> based on a string value.
        /// </summary>
        /// <param name="name">The name of the header</param>
        /// <param name="value">The value of the header</param>
        private HttpCondition(string name, string value)
        {
            Validate.IsPreconditionHeader(name, nameof(name));
            Validate.IsNotNullOrWhitespace(value, nameof(value));

            HeaderName = name;
            HeaderValue = value == "*" ? value : $"\"{value.Trim('"')}\"";
        }

        /// <summary>
        /// The header name to use for this precondition.
        /// </summary>
        private string HeaderName { get; }

        /// <summary>
        /// The value of the precondition header.
        /// </summary>
        private string HeaderValue { get; }

        /// <summary>
        /// Add the pre-condition to the provided <see cref="HttpHeaders"/> object.
        /// </summary>
        /// <param name="headers">The <see cref="HttpHeaders"/> object to modify.</param>
        public void AddToHeaders(HttpHeaders headers)
        {
            Validate.IsNotNull(headers, nameof(headers));
            if (headers.Contains(HeaderName))
            {
                headers.Remove(HeaderName);
            }
            headers.Add(HeaderName, HeaderValue);
        }

        /// <summary>
        /// A printable / loggable version of the object.
        /// </summary>
        public override string ToString()
            => $"PRECONDITION({HeaderName}={HeaderValue})";

        /// <summary>
        /// Requires that the request only succeed if the entity exists.
        /// </summary>
        public static HttpCondition IfExists() => new(IfMatchHeader, Any);

        /// <summary>
        /// Requires that the request only succeed if the entity does not exist.
        /// </summary>
        public static HttpCondition IfNotExists() => new(IfNoneMatchHeader, Any);

        /// <summary>
        /// Requires that the request succeed if the version matches.
        /// </summary>
        /// <param name="version"></param>
        public static HttpCondition IfMatch(byte[] version) => new(IfMatchHeader, version);

        /// <summary>
        /// Requires that the request succeed if the version matches
        /// </summary>
        /// <param name="version"></param>
        public static HttpCondition IfMatch(string version) => new(IfMatchHeader, version);

        /// <summary>
        /// Requires that the request succeed if the version does not match.
        /// </summary>
        /// <param name="version"></param>
        public static HttpCondition IfNotMatch(byte[] version) => new(IfNoneMatchHeader, version);

        /// <summary>
        /// Requires that the request succeed if the version does not match.
        /// </summary>
        /// <param name="version"></param>
        public static HttpCondition IfNotMatch(string version) => new(IfNoneMatchHeader, version);
    }
}
