using Microsoft.Net.Http.Headers;
using System;

namespace Azure.Mobile.Server.Utils
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
    }
}
