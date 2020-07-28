using Azure.Core;
using System;
using System.Globalization;

namespace Azure.Mobile.Client.Utils
{
    internal static class RequestExtensions
    {
        /// <summary>
        /// Build a Uri for accessing the table endpoint
        /// </summary>
        /// <param name="request">The request to modify</param>
        /// <param name="endpoint">The table endpoint</param>
        /// <param name="relativePath">An optional relative path to the real endpoint</param>
        internal static void BuildUri(this Request request, Uri endpoint, string relativePath = null)
        {
            var builder = request.Uri;
            builder.Reset(endpoint);
            if (relativePath != null)
            {
                builder.AppendPath($"/{relativePath}", escape: false);
            }
        }

        /// <summary>
        /// Applies the provided <see cref="MatchConditions"/> to the request as headers.
        /// </summary>
        /// <param name="request">The request to modify</param>
        /// <param name="requestOptions">The <see cref="MatchConditions"/> to apply.</param>
        internal static void ApplyConditionalHeaders(this Request request, MatchConditions options)
        {
            if (options == null)
            {
                return;
            }

            if (options.IfMatch.HasValue)
            {
                string value = options.IfMatch.Value == ETag.All ?
                    options.IfMatch.Value.ToString() : $"\"{options.IfMatch.Value.ToString()}\"";
                request.Headers.Add(HttpHeader.Names.IfMatch, value);
            }

            if (options.IfNoneMatch.HasValue)
            {
                string value = options.IfNoneMatch.Value == ETag.All ?
                    options.IfNoneMatch.Value.ToString() : $"\"{options.IfNoneMatch.Value.ToString()}\"";
                request.Headers.Add(HttpHeader.Names.IfNoneMatch, value);
            }

            if (options is RequestConditions dateOptions)
            {
                if (dateOptions.IfModifiedSince.HasValue)
                {
                    request.Headers.Add(HttpHeader.Names.IfModifiedSince, dateOptions.IfModifiedSince.Value.UtcDateTime.ToString("R", CultureInfo.InvariantCulture));
                }

                if (dateOptions.IfUnmodifiedSince.HasValue)
                {
                    request.Headers.Add(HttpHeader.Names.IfUnmodifiedSince, dateOptions.IfUnmodifiedSince.Value.UtcDateTime.ToString("R", CultureInfo.InvariantCulture));
                }
            }
        }
    }
}
