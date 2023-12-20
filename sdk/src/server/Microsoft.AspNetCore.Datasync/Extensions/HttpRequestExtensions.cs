// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync.Extensions;

internal static class HttpRequestExtensions
{
    /// <summary>
    /// Creates the NextLink Uri for the next request in a paging request.
    /// </summary>
    /// <param name="skip">The skip value</param>
    /// <param name="top">The top value</param>
    /// <returns>A URI representing the next page</returns>
    internal static string CreateNextLink(this HttpRequest request, int skip = 0, int top = 0)
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
        return string.Join('&', query).TrimStart('&');
    }

    /// <summary>
    /// Used in checking the If-Modified-Since and If-Unmodified-Since headers for date/time comparisons with nulls.
    /// </summary>
    internal static bool IsAfter(this DateTimeOffset left, DateTimeOffset? right)
        => !right.HasValue || left > right.Value;

    /// <summary>
    /// Used in checking the If-Modified-Since and If-Unmodified-Since headers for date/time comparisons with nulls.
    /// </summary>
    internal static bool IsBefore(this DateTimeOffset left, DateTimeOffset? right)
        => !right.HasValue || left <= right.Value;

    /// <summary>
    /// Determines if the entity tag in the If-Match or If-None-Match header matches the entity version.
    /// </summary>
    /// <param name="etag">The entity tag header value.</param>
    /// <param name="version">The version in the entity.</param>
    /// <returns><c>true</c> if the entity tag header value matches the version; <c>false</c> otherwise.</returns>
    internal static bool Matches(this EntityTagHeaderValue etag, byte[] version)
        => !etag.IsWeak && version.Length > 0 && (etag.Tag == "*" || etag.Tag.ToString().Trim('"').Equals(version.ToEntityTagValue()));

    /// <summary>
    /// Determines if the request has met the preconditions within the conditional headers, according to RFC 7232 section 5 and 6.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being checked.</typeparam>
    /// <param name="request">The current <see cref="HttpRequest"/> object that contains the request headers.</param>
    /// <param name="entity">The entity being checked.</param>
    /// <param name="version">On conclusion, the version that was requested.</param>
    /// <exception cref="HttpException">Thrown if the conditional request requirements are not met.</exception>
    internal static void ParseConditionalRequest<TEntity>(this HttpRequest request, TEntity entity, out byte[] version) where TEntity : ITableData
    {
        var headers = request.GetTypedHeaders();
        bool isFetch = request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase);

        if (headers.IfMatch.Count > 0 && !headers.IfMatch.Any(e => e.Matches(entity.Version)))
        {
            throw new HttpException(StatusCodes.Status412PreconditionFailed) { Payload = entity };
        }

        if (headers.IfMatch.Count == 0 && headers.IfUnmodifiedSince.HasValue && headers.IfUnmodifiedSince.Value.IsBefore(entity.UpdatedAt))
        {
            throw new HttpException(StatusCodes.Status412PreconditionFailed) { Payload = entity };
        }

        if (headers.IfNoneMatch.Count > 0 && headers.IfNoneMatch.Any(e => e.Matches(entity.Version)))
        {
            throw isFetch ? new HttpException(StatusCodes.Status304NotModified) : new HttpException(StatusCodes.Status412PreconditionFailed) { Payload = entity };
        }

        if (headers.IfNoneMatch.Count == 0 && headers.IfModifiedSince.HasValue && headers.IfModifiedSince.Value.IsAfter(entity.UpdatedAt))
        {
            throw isFetch ? new HttpException(StatusCodes.Status304NotModified) : new HttpException(StatusCodes.Status412PreconditionFailed) { Payload = entity };
        }

        version = headers.IfMatch.Count > 0 ? headers.IfMatch.Single().ToByteArray() : Array.Empty<byte>();
    }

    /// <summary>
    /// Determines if the client requested that the deleted items should be considered to
    /// be "in view".
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/> being processed.</param>
    /// <returns><c>true</c> if deleted items shoudl be considered "in view"; <c>false</c> otherwise.</returns>
    internal static bool ShouldIncludeDeletedItems(this HttpRequest request)
    {
        if (request.Query.TryGetValue("__includedeleted", out StringValues deletedQueryParameter))
        {
            return deletedQueryParameter.Any(x => x!.Equals("true", StringComparison.InvariantCultureIgnoreCase));
        }
        return false;
    }

    /// <summary>
    /// Convertes the provided ETag into a byte array.
    /// </summary>
    /// <param name="etag">The ETag to convert.</param>
    /// <returns>The ETag converted to a byte array.</returns>
    internal static byte[] ToByteArray(this EntityTagHeaderValue etag)
        => etag.Tag == "*" ? Array.Empty<byte>() : Convert.FromBase64String(etag.Tag.ToString().Trim('"'));
}
