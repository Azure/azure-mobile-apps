// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync.Extensions
{
    internal static class HttpRequestExtensions
    {
        /// <summary>
        /// Creates the NextLink Uri for the next request in a paging request.
        /// </summary>
        /// <param name="skip">The skip value</param>
        /// <param name="top">The top value</param>
        /// <returns>A URI representing the next page</returns>
        internal static Uri CreateNextLink(this HttpRequest request, int skip = 0, int top = 0)
        {
            var builder = new UriBuilder(request.GetDisplayUrl());
            List<string> query = string.IsNullOrEmpty(builder.Query) ? new() : builder.Query.TrimStart('?').Split('&').Where(q => !q.StartsWith("$skip=") && !q.StartsWith("$top=")).ToList();

            if (skip > 0)
            {
                query.Add($"$skip={skip}");
            }
            if (top > 0)
            {
                query.Add($"$top={top}");
            }

            builder.Query = $"?{string.Join('&', query).TrimStart('&')}";

            // Issue #2: If the request is for the default port (http=80/https=443), then mark the port as default
            if (builder.IsDefaultPort())
            {
                builder.Port = -1;
            }

            return builder.Uri;
        }

        /// <summary>
        /// Returns true if the port specified by the UriBuilder is default for the protocol.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/> to check</param>
        /// <returns>true if the current port is the default for the current scheme.</returns>
        internal static bool IsDefaultPort(this UriBuilder builder)
            => (builder.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) && builder.Port == 80) || (builder.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase) && builder.Port == 443);

        /// <summary>
        /// Determines if the request has met the preconditions within the conditional headers,
        /// according to RFC 7232 section 5 and 6.
        /// </summary>
        /// <param name="entity">The entity to use as comparison</param>
        /// <param name="version">The version that was requested</param>
        internal static void ParseConditionalRequest<TEntity>(this HttpRequest request, TEntity entity, out byte[] version) where TEntity : ITableData
        {
            var headers = request.GetTypedHeaders();
            var isFetch = request.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase);
            var etag = entity.ToEntityTagHeaderValue();

            if (headers.IfMatch.Count > 0)
            {
                if (!headers.IfMatch.Any(e => e.Matches(etag)))
                {
                    throw new PreconditionFailedException(entity);
                }
            }

            if (headers.IfMatch.Count == 0 && headers.IfUnmodifiedSince.HasValue)
            {
                if (entity == null || entity.UpdatedAt > headers.IfUnmodifiedSince.Value)
                {
                    throw new PreconditionFailedException(entity);
                }
            }

            if (headers.IfNoneMatch.Count > 0)
            {
                if (headers.IfNoneMatch.Any(e => e.Matches(etag)))
                {
                    throw isFetch ? new NotModifiedException() : new PreconditionFailedException(entity);
                }
            }

            if (headers.IfNoneMatch.Count == 0 && headers.IfModifiedSince.HasValue)
            {
                if (entity == null || entity.UpdatedAt <= headers.IfModifiedSince.Value)
                {
                    throw isFetch && entity != null ? new NotModifiedException() : new PreconditionFailedException(entity);
                }
            }

            version = headers.IfMatch.Count > 0 ? headers.IfMatch.Single().ToByteArray() : null;
        }

        /// <summary>
        /// Reads the body of the request as a string.
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>The body of the request</returns>
        internal static async Task<string> GetBodyAsStringAsync(this HttpRequest request)
        {
            using var streamReader = new StreamReader(request.Body, Encoding.UTF8);
            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}
