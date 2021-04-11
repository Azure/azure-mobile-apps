// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AzureMobile.Server.Exceptions;

namespace Microsoft.AzureMobile.Server.Extensions
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
            var builder = new UriBuilder(new Uri(request.GetDisplayUrl()));
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
            return builder.Uri;
        }

        /// <summary>
        /// Determines if the request has met the preconditions within the conditional headers,
        /// according to RFC 7232 sectuin 5 and 6.
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
    }
}
