// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// The <see cref="ITableData"/> provides an abstraction indicating how the system
    /// properties for a given table data model are to be serialized when communicating with
    /// the clients. The uniform serialization of system properties ensures that the clients 
    /// can process the system properties uniformly across platforms.
    /// </summary>
    public interface ITableData
    {
        /// <summary>
        /// Gets or sets the unique ID for this entity.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the unique version identifier which is updated every time the entity is updated.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is part of the data model.")]
        byte[] Version { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity was created.
        /// </summary>
        DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time the entity was last modified.
        /// </summary>
        DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted.
        /// </summary>
        bool Deleted { get; set; }
    }
}
