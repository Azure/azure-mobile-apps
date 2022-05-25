// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// An abstract implementation of the <see cref="ITableData"/> interface indicating how the
    /// system properties for a given table data model are to be serialized when communicating
    /// with clients when using Entity Framework for accessing the backend store.
    /// The uniform serialization of system properties ensures that the clients 
    /// can process the system properties uniformly across platforms. Concrete entity framework
    /// models can derive from this base class in order to support the system properties.
    /// </summary>
    public abstract class EntityData : ITableData
    {
        protected EntityData()
        {
        }

        [Key]
        [TableColumnAttribute(TableColumnType.Id)]
        public string Id { get; set; }

        [Timestamp]
        [TableColumnAttribute(TableColumnType.Version)]
        public byte[] Version { get; set; }

        [Index(IsClustered = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [TableColumnAttribute(TableColumnType.CreatedAt)]
        public DateTimeOffset? CreatedAt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [TableColumnAttribute(TableColumnType.UpdatedAt)]
        public DateTimeOffset? UpdatedAt { get; set; }

        [TableColumnAttribute(TableColumnType.Deleted)]
        public bool Deleted { get; set; }
    }
}
