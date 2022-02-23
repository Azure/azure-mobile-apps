// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Net.Http;
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
        /// Returns true if the response is compressed.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> for the response.</param>
        /// <returns><c>true</c> if the response headers indicates the response is compressed.</returns>
        internal static bool IsCompressed(this HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentEncoding?.Count > 0)
            {
                return response.Content.Headers.ContentEncoding.Any(m => ValidCompressionMethods.Contains(m));
            }
            else if (response.Headers.Vary?.Count > 0)
            {
                return response.Headers.Vary.Contains("Accept-Encoding");
            }
            return false;
        }

        /// <summary>
        /// Reads the conetnt of the <see cref="HttpContent"/> object as a string, with cancellation.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> object to process</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A task that returns the content as a string when complete.</returns>
        internal static Task<string> ReadAsStringAsync(this HttpContent content, CancellationToken cancellationToken)
            => Task.Run(() => content.ReadAsStringAsync(), cancellationToken);
    }
}
