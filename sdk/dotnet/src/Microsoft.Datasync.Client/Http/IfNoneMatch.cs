// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Utils;
using System.Text.Json;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A precondition header that suggests only do the operation if the precondition does not match.
    /// </summary>
    public class IfNoneMatch : Precondition
    {
        private const string headerName = "If-None-Match";

        private IfNoneMatch(string value) : base(headerName)
        {
            Validate.IsNotNullOrWhitespace(value, nameof(value));
            HeaderValue = value == "*" ? value : $"\"{value.Trim('"')}\"";
        }

        private IfNoneMatch(byte[] value) : base(headerName)
        {
            Validate.IsNotNullOrEmpty(value, nameof(value));
            HeaderValue = JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Precondition for "does not exist"
        /// </summary>
        public static IfNoneMatch Any() => new("*");

        /// <summary>
        /// Precondition for "is not version"
        /// </summary>
        /// <param name="version">The version of the entity</param>
        public static IfNoneMatch Version(string version) => new(version);

        /// <summary>
        /// Precondition for "is not version"
        /// </summary>
        /// <param name="version">The version of the entity</param>
        public static IfNoneMatch Version(byte[] version) => new(version);

    }
}
