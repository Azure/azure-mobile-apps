// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A set of extension methods to more easily deal with HTTP messages.
    /// </summary>
    internal static class HttpMessageExtensions
    {
        private static readonly string[] ValidCompressionMethods = new string[] { "br", "compress", "deflate", "gzip" };

        /// <summary>
        /// Gets the version string from the provided <c>ETag</c> header value.
        /// </summary>
        /// <param name="value">The <c>ETag</c> value.</param>
        /// <returns>The version string.</returns>
        internal static string GetVersion(this EntityTagHeaderValue value)
        {
            if (string.IsNullOrEmpty(value?.Tag))
            {
                return null;
            }
            return value.Tag.Substring(1, value.Tag.Length - 2).Replace("\\\"", "\"");
        }

        /// <summary>
        /// Converts a string to a quoted ETag value.
        /// </summary>
        /// <param name="value">The version string to convert.</param>
        /// <returns>The converted value.</returns>
        internal static string ToETagValue(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '"' && (i == 0 || value[i - 1] != '\\'))
                {
                    value = value.Insert(i, "\\");
                }
            }
            return $"\"{value}\"";
        }

        /// <summary>
        /// Returns true if the content is filled.
        /// </summary>
        /// <param name="content">The content to check..</param>
        /// <returns><c>true</c> if there is content; <c>false</c> otherwise.</returns>
        internal static bool HasContent(this HttpContent content)
            => content != null && content.GetType().Name != "EmptyContent";

        /// <summary>
        /// Returns true if the response has some content.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <returns><c>true</c> if the response has content; <c>false</c> otherwise.</returns>
        internal static bool HasContent(this HttpResponseMessage response)
            => HasContent(response.Content);

        /// <summary>
        /// Returns <c>true</c> if the response is compressed.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> for the response.</param>
        /// <returns><c>true</c> if the response headers indicates the response is compressed.</returns>
        internal static bool IsCompressed(this HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentEncoding.Count > 0)
            {
                return response.Content.Headers.ContentEncoding.Any(m => ValidCompressionMethods.Contains(m));
            }
            else if (response.Headers.Vary.Count > 0)
            {
                return response.Headers.Vary.Contains("Accept-Encoding");
            }
            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if the response indicates a conflict.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> for the response.</param>
        /// <returns><c>true</c> if the response indicates the response is a conflict.</returns>
        internal static bool IsConflictStatusCode(this HttpResponseMessage response)
            => response.StatusCode == HttpStatusCode.PreconditionFailed || response.StatusCode == HttpStatusCode.Conflict;

        /// <summary>
        /// Reads the content of the <see cref="HttpContent"/> object as a string, with cancellation.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> object to process</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A task that returns the content as a string when complete.</returns>
        internal static Task<string> ReadAsStringAsync(this HttpContent content, CancellationToken cancellationToken)
            => Task.Run(() => content.ReadAsStringAsync(), cancellationToken);
    }
}
