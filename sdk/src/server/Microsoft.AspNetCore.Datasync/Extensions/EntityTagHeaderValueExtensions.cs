// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Datasync;

internal static class EntityTagHeaderValueExtensions
{
    /// <summary>
    /// Compares two <see cref="EntityTagHeaderValue"/> according to the rules within
    /// RFC 7232 section 2.3.2, using strong comparisons only.
    /// </summary>
    /// <param name="a">The left-hand <see cref="EntityTagHeaderValue"/>.</param>
    /// <param name="b">The right-hand <see cref="EntityTagHeaderValue"/>.</param>
    /// <returns><c>true</c> if the values match; <c>false</c> otherwise.</returns>
    internal static bool Matches(this EntityTagHeaderValue a, EntityTagHeaderValue? b)
        => b != null && !a.IsWeak && !b.IsWeak && (a.Tag == "*" || b.Tag == "*" || a.Tag == b.Tag);

    /// <summary>
    /// Converts the provided <see cref="EntityTagHeaderValue"/> to a byte array.  If the
    /// value is "*", then an empty array is returned.
    /// </summary>
    /// <remarks>
    /// The <c>ETag</c> is a Base64 encoded string in Azure Mobile Apps.
    /// </remarks>
    /// <param name="etag">The <see cref="EntityTagHeaderValue"/> to convert.</param>
    /// <returns>The version byte array.</returns>
    internal static byte[] ToByteArray(this EntityTagHeaderValue etag)
        => etag.Tag == "*" ? Array.Empty<byte>() : Convert.FromBase64String(etag.Tag.ToString().Trim('"'));
}
