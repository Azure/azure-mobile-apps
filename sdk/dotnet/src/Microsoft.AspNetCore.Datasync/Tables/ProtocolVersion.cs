// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Datasync.Tables
{
    /// <summary>
    /// List of known versions that are valid in this edition of Azure Mobile Apps
    /// </summary>
    public enum ProtocolVersion
    {
        Invalid,
        V2,
        V3
    }

    internal static class ProtocolVersionExtensions
    {
        /// <summary>
        /// Parses an <c>ZUMO-API-VERSION</c> value to ensure it is valid.
        /// </summary>
        /// <param name="input">The version header or query parameter</param>
        /// <param name="version">The parsed version</param>
        /// <returns>true if the parsed version is valid</returns>
        internal static bool TryParseProtocolVersion(this string input, out ProtocolVersion version)
        {
            Regex pattern = new(@"^[0-9]\.[0-9](\.[0-9])?$");
            if (pattern.Match(input).Success)
            {
                if (input.StartsWith("2.0"))
                {
                    version = ProtocolVersion.V2;
                    return true;
                }
                else if (input.StartsWith("3.0"))
                {
                    version = ProtocolVersion.V3;
                    return true;
                }
            }
            version = ProtocolVersion.Invalid;
            return false;
        }

        /// <summary>
        /// Determines if the ZUMO-API-VERSION matches the provided version.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A <see cref="ZumoVersion"/> value for the current request</returns>
        internal static bool IsProtocolVersion(this HttpRequest request, ProtocolVersion expected)
            => request.HttpContext.Items["ProtocolVersion"] is ProtocolVersion version && version == expected;
    }
}
