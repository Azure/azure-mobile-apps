// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;

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
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// If set to true, the item is deleted.
        /// </summary>
        [JsonProperty("deleted")]
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// The last time that the record was updated.
        /// </summary>
        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.MinValue;

        /// <summary>
        /// The opaque version string.  This changes when the
        /// item is updated.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
