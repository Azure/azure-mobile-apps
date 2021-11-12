// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// The <see cref="ITableData"/> interface provides an abstraction for the system properties
    /// for a given table data model.
    /// </summary>
    public interface ITableData : IEquatable<ITableData>
    {
        /// <summary>
        /// The globally unique ID for this entity.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The unique version identifier for this entity.  It must be updated on every write.
        /// </summary>
        byte[] Version { get; set; }

        /// <summary>
        /// The date and time that the entity was last updated.
        /// </summary>
        DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// An indicator that this entity has been marked for deletion.
        /// </summary>
        bool Deleted { get; set; }
    }
}
