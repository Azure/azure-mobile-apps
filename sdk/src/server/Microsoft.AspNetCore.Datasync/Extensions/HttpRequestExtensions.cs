// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Microsoft.AspNetCore.Datasync;

internal static class HttpRequestExtensions
{
    /// <summary>
    /// Creates the <c>nextLink</c> <see cref="Uri"/> for the next request in a paging set.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequest"/> object.</param>
    /// <param name="skip">The new value of the <c>$skip</c> query parameter.</param>
    /// <param name="top">The new value of the <c>$top</c> query parameter.</param>
    /// <returns>A <see cref="Uri"/> representing the next page of results.</returns>
    internal static Uri CreateNextLink(this HttpRequest request, int skip = 0, int top = 0)
    {
        UriBuilder builder = new(request.GetDisplayUrl());
        List<string> query = string.IsNullOrEmpty(builder.Query) ? new()
            : builder.Query.TrimStart('?').Split('&').Where(q => !q.StartsWith("$skip=") && !q.StartsWith("$top=")).ToList();
        query.AddIf(skip > 0, $"$skip={skip}");
        query.AddIf(top > 0, $"$top={top}");
        builder.Query = $"?{string.Join("&", query).TrimStart('&')}";
        if (builder.IsDefaultPort())
        {
            builder.Port = -1;
        }
        return builder.Uri;
    }

    /// <summary>
    /// Reads the body of the request as a string.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/> to process.</param>
    /// <returns>The body of the request.</returns>
    internal static async Task<string> GetBodyAsStringAsync(this HttpRequest request)
    {
        using StreamReader streamReader = new(request.Body, Encoding.UTF8);
        return await streamReader.ReadToEndAsync();
    }

    /// <summary>
    /// Returns <c>true</c> if the port specified in the <see cref="UriBuilder"/> object is the
    /// default port for the protocol.
    /// </summary>
    /// <param name="builder">The <see cref="UriBuilder"/> object to check.</param>
    /// <returns><c>true</c> if the current port is the default for the current scheme; <c>false</c> otherwise.</returns>
    internal static bool IsDefaultPort(this UriBuilder builder)
        => (builder.Port == 80 && builder.Scheme.EqualsIgnoreCase("http")) || (builder.Port == 443 && builder.Scheme.EqualsIgnoreCase("https"));

    /// <summary>
    /// Determines if the request has met the preconditions within the conditional headers according to RFC 7232 sections 5 and 6.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity being processed.</typeparam>
    /// <param name="request">The current <see cref="HttpRequest"/> object.</param>
    /// <param name="entity">The entity to use for comparisons.</param>
    /// <param name="version">On exit, the version that was requested, or an empty array.</param>
    internal static void ParseConditionalRequest<TEntity>(this HttpRequest request, TEntity entity, out byte[] version) where TEntity : ITableData
    {
        RequestHeaders headers = request.GetTypedHeaders();
        bool isFetchOperation = request.Method.EqualsIgnoreCase("GET");
        EntityTagHeaderValue? etag = entity.ToEntityTagHeaderValue();

        if (headers.IfMatch.Count > 0 && !headers.IfMatch.Any(e => e.Matches(etag)))
        {
            throw new HttpException(StatusCodes.Status412PreconditionFailed, "If-Match condition failed") { Payload = entity };
        }

        if (headers.IfMatch.Count == 0 && headers.IfUnmodifiedSince?.IsAfter(entity.UpdatedAt) == false)
        {
            throw new HttpException(StatusCodes.Status412PreconditionFailed, "If-Unmodified-Since condition failed") { Payload = entity };
        }

        if (headers.IfNoneMatch.Count > 0 && headers.IfNoneMatch.Any(e => e.Matches(etag)))
        {
            throw isFetchOperation
                ? new HttpException(StatusCodes.Status304NotModified)
                : new HttpException(StatusCodes.Status412PreconditionFailed, "If-None-Match condition failed") { Payload = entity };
        }

        if (headers.IfNoneMatch.Count == 0 && headers.IfModifiedSince?.IsBefore(entity.UpdatedAt) == false)
        {
            throw isFetchOperation
                ? new HttpException(StatusCodes.Status304NotModified)
                : new HttpException(StatusCodes.Status412PreconditionFailed, "If-Modified-Since condition failed") { Payload = entity };
        }

        version = headers.IfMatch.Count > 0 ? headers.IfMatch.Single().ToByteArray() : Array.Empty<byte>();
    }
}
