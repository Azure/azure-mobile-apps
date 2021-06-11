// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Net.Http.Headers;
using System;

namespace Microsoft.AspNetCore.Datasync.Extensions
{
    internal static class EntityTagHeaderValueExtensions
    {
        /// <summary>
        /// Compares two entity-tags according to the rules within RFC 7232 section 2.3.2,
        /// using strong comparisons only.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if there is a match</returns>
        internal static bool Matches(this EntityTagHeaderValue a, EntityTagHeaderValue b)
            => a != null && b != null
            && !a.IsWeak && !b.IsWeak
            && (a.Tag == "*" || b.Tag == "*" || a.Tag == b.Tag);

        /// <summary>
        /// Converts the provided ETag into a byte array. If the etag is "*", then null is
        /// returned.
        /// </summary>
        /// <param name="etag">The ETag value</param>
        /// <returns></returns>
        internal static byte[] ToByteArray(this EntityTagHeaderValue etag)
        {
            if (etag.Tag == "*")
                return null;
            return Convert.FromBase64String(etag.Tag.ToString().Trim('"'));
        }
    }
}
