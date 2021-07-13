// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A base class for defining data transfer objects.
    /// </summary>
    public abstract class DatasyncClientData
    {
        /// <summary>
        /// The item globally unique ID.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// If set to true, the item is deleted.
        /// </summary>
        [JsonPropertyName("deleted")]
        public bool? Deleted { get; set; }

        /// <summary>
        /// The last time that the record was updated.
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// The opaque version string.  This changes when the
        /// item is updated.
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
