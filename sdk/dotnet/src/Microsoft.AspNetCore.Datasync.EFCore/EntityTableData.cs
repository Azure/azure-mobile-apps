// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Microsoft.AspNetCore.Datasync.EFCore
{
    /// <summary>
    /// An "Entity Framework ready" version of the <see cref="ITableData"/>.
    /// </summary>
    public abstract class EntityTableData : ITableData
    {
        [Key]
        public virtual string Id { get; set; }

        /// <summary>
        /// The date/time that the entity was updated.
        /// </summary>
        public virtual DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The row version for the entity.
        /// </summary>
        [Timestamp]
        public virtual byte[] Version { get; set; }

        /// <summary>
        /// True if the entity is marked as deleted.
        /// </summary>
        public virtual bool Deleted { get; set; }

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
