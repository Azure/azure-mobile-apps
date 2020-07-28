using Microsoft.AspNet.OData.Query;
using System;
using System.ComponentModel.DataAnnotations;

namespace Azure.Mobile.Server.Entity
{
    /// <summary>
    /// An "Entity Framework ready" version of the <see cref="ITableData"/> that
    /// can be used for augmenting the data transfer objects used
    /// with Azure.Mobile.Server.
    /// </summary>
    public abstract class EntityTableData : ITableData
    {
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// The date/time that the entity was updated.
        /// </summary>
        [OrderBy]
        [Filter]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The row version for the entity.
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; }

        /// <summary>
        /// True if the entity is marked as deleted.
        /// </summary>
        [Filter]
        public bool Deleted { get; set; }
    }
}
