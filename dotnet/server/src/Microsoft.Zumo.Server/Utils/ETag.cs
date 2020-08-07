// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;

namespace Microsoft.Zumo.Server.Utils
{
    /// <summary>
    /// Methods that assist in evaluating the pre-conditions, per RFC 7232
    /// </summary>
    /// <see cref="https://tools.ietf.org/html/rfc7232"/>
    internal static class ETag
    {
        /// <summary>
        /// Creates an ETag Header Value base donn a byte array version.
        /// </summary>
        /// <param name="version">The value to check</param>
        /// <returns>The ETag header value.</returns>
        internal static string FromByteArray(byte[] version) => $"\"{Convert.ToBase64String(version)}\"";

        /// <summary>
        /// Compares two <see cref="EntityTagHeaderValue"/> values to see if they match.  The
        /// global match (*) in a header is taken into account.
        /// </summary>
        /// <param name="a">An <see cref="EntityTagHeaderValue"/> value.</param>
        /// <param name="b">An <see cref="EntityTagHeaderValue"/> value.</param>
        /// <returns>true if the two values match</returns>
        internal static bool Matches(EntityTagHeaderValue a, EntityTagHeaderValue b)
            => a != null && b != null && !a.IsWeak && !b.IsWeak && (a.Tag == "*" || b.Tag == "*" || a.Tag == b.Tag);

        /// <summary>
        /// Evaluate the pre-condition headers (If-Match, If-None-Match, etc.) according
        /// to RFC7232 section 6.
        /// </summary>
        /// <seealso cref="https://tools.ietf.org/html/rfc7232"/>
        /// <param name="item">The item within the check</param>
        /// <param name="headers">The typed headers for the request</param>
        /// <param name="isFetch">True if the request is a fetch request</param>
        /// <returns>A HTTP Status Code indicating success or expected response.</returns>
        internal static int EvaluatePreconditions<T>(T item, RequestHeaders headers, bool isFetch = false) where T : class, ITableData
        {
            var eTagVersion = item == null ? null : new EntityTagHeaderValue(ETag.FromByteArray(item.Version));

            // Step 1: If If-Match is present, evaluate it
            if (headers.IfMatch.Count > 0)
            {
                if (!headers.IfMatch.Any(e => ETag.Matches(e, eTagVersion)))
                {
                    return StatusCodes.Status412PreconditionFailed;
                }
            }
            // Step 2: If If-Match is not present, and If-Modified-Since is present, 
            //  evaluate the If-Modified-Since precondition
            if (headers.IfMatch.Count == 0 && headers.IfUnmodifiedSince.HasValue)
            {
                if (!(item != null && item.UpdatedAt <= headers.IfUnmodifiedSince.Value))
                {
                    return StatusCodes.Status412PreconditionFailed;
                }
            }

            // Step 3: If If-None-Match is present, evaluate it.
            if (headers.IfNoneMatch.Count > 0)
            {
                if (!headers.IfNoneMatch.All(e => !ETag.Matches(e, eTagVersion)))
                {
                    return isFetch ? StatusCodes.Status304NotModified : StatusCodes.Status412PreconditionFailed;
                }
            }

            // Step 4: If isFetchOperation, and If-None-Match is not present, and If-Modified-Since is present,
            //  evaluate the If-Modified-Since precondition
            if (isFetch && headers.IfNoneMatch.Count == 0 && headers.IfModifiedSince.HasValue)
            {
                if (!(item != null && item.UpdatedAt > headers.IfModifiedSince.Value))
                {
                    return StatusCodes.Status304NotModified;
                }
            }

            // Step 5 is about Range and If-Range requests, which we don't support.

            // Otherwise, do the operation as normal.
            return StatusCodes.Status200OK;
        }
    }
}
