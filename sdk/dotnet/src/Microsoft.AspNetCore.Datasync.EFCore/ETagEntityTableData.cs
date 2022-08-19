// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Microsoft.AspNetCore.Datasync.EFCore
{
    /// <summary>
    /// An implementation of <see cref="ITableData"/> that is appropriate
    /// for EF Core databases that have an ETag (string-based) versioning
    /// concurrency check instead of a byte[] based versioning concurrency
    /// check
    /// </summary>
    public class ETagEntityTableData : ITableData
    {
        /// <summary>
        /// The globally unique ID for this entity.
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// The date/time that the entity was updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The row version for the entity.
        /// </summary>
        [NotMapped]
        public byte[] Version
        {
            get => Encoding.UTF8.GetBytes(this.EntityTag ?? string.Empty);
            set { this.EntityTag = Encoding.UTF8.GetString(value); }
        }

        /// <summary>
        /// The ETag for the entity.
        /// </summary>
        [Timestamp]
        [JsonIgnore]
        public string EntityTag
        {
            get; set;
        }

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
