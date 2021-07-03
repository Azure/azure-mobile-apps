// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Utils;
using System.Text.Json;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A precondition header that suggests only do the operation if the precondition matches.
    /// </summary>
    public class IfMatch : Precondition
    {
        private const string headerName = "If-Match";

        private IfMatch(string value) : base(headerName)
        {
            Validate.IsNotNullOrWhitespace(value, nameof(value));
            HeaderValue = value == "*" ? value : $"\"{value.Trim('"')}\"";
        }

        private IfMatch(byte[] value) : base(headerName)
        {
            Validate.IsNotNullOrEmpty(value, nameof(value));
            HeaderValue = JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Precondition for "exists"
        /// </summary>
        public static IfMatch Any() => new("*");

        /// <summary>
        /// Precondition for "matches version"
        /// </summary>
        /// <param name="version">The version</param>
        public static IfMatch Version(string version) => new(version);

        /// <summary>
        /// Precondition for "matches version"
        /// </summary>
        /// <param name="version">The version</param>
        public static IfMatch Version(byte[] version) => new(version);

    }
}
