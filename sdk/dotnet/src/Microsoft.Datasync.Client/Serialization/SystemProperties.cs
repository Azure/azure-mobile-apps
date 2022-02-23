// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// The system properties that we can expect on an instance.
    /// </summary>
    internal class SystemProperties
    {
        /// <summary>
        /// The name of the "id" property in a JSON document.
        /// </summary>
        internal const string JsonIdProperty = "id";

        /// <summary>
        /// The name of the "version" property in a JSON document.
        /// </summary>
        internal const string JsonVersionProperty = "version";

        /// <summary>
        /// The name of the "updatedAt" property in a JSON document.
        /// </summary>
        internal const string JsonUpdatedAtProperty = "updatedAt";

        /// <summary>
        /// The globally unique ID for the instance.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The last date that the instance was updated on the service.
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// The version set on the service.
        /// </summary>
        public string Version { get; set; }
    }
}
