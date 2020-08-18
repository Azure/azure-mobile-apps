// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Zumo.MobileData
{
    /// <summary>
    /// Base class for all data transfer objects.
    /// </summary>
    public abstract class TableData
    {
        /// <summary>
        /// The globally unique ID for the entity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The ETag version for the entity
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The last moment the entity was updated
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// True if the entity has been soft-deleted
        /// </summary>
        public bool Deleted { get; set; }
    }
}
