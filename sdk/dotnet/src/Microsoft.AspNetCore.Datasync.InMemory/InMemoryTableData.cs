// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace Microsoft.AspNetCore.Datasync.InMemory
{
    /// <summary>
    /// An implementation of the <see cref="ITableData"/> interface for handling
    /// in-memory test repositories.
    /// </summary>
    public class InMemoryTableData : ITableData
    {
        /// <summary>
        /// The globally unique ID for this entity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The unique version identifing this entity; updated every time the entity is updated.
        /// </summary>
        public byte[] Version { get; set; }

        /// <summary>
        /// The date and time that the entity was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// True if the entity is marked as deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Implements the <see cref="IEquatable{T}"/> interface to determine
        /// if the system properties match the system properties of the other
        /// entity.
        /// </summary>
        /// <param name="other">The other entity</param>
        /// <returns>true if the entity matches and the system properties are set.</returns>
        public bool Equals(ITableData other)
            => other != null
            && Id == other.Id
            && UpdatedAt == other.UpdatedAt
            && Deleted == other.Deleted
            && Version.SequenceEqual(other.Version);
    }
}
