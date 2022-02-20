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
