// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// The system properties that we can expect on an instance.
    /// </summary>
    public static class SystemProperties
    {
        /// <summary>
        /// The name of the "deleted" property in a JSON document.
        /// </summary>
        public const string JsonDeletedProperty = "deleted";

        /// <summary>
        /// The name of the "id" property in a JSON document.
        /// </summary>
        public const string JsonIdProperty = "id";

        /// <summary>
        /// The name of the "version" property in a JSON document.
        /// </summary>
        public const string JsonVersionProperty = "version";

        /// <summary>
        /// The name of the "updatedAt" property in a JSON document.
        /// </summary>
        public const string JsonUpdatedAtProperty = "updatedAt";

        /// <summary>
        /// The list of system properties, not including ID.
        /// </summary>
        internal static readonly string[] AllSystemProperties = new[]
        {
            JsonDeletedProperty,
            JsonVersionProperty,
            JsonUpdatedAtProperty
        };
    }
}
