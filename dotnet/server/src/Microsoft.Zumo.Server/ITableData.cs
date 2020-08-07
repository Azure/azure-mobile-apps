// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// The <see cref="ITableData"/> provides an abstraction indicating how the system properties
    /// for a given table data model are to be serialized when communicating with the clients.  The
    /// uniform serialization of system properties ensures that the clients can process the system
    /// properties uniformly across platforms.
    /// </summary>
    public interface ITableData
    {
        /// <summary>
        /// The stable globally unique ID for this entity.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// The unique version identifier (which is updated every time the entity is updated).
        /// </summary>
        byte[] Version { get; set; }

        /// <summary>
        /// The timestamp for when the entity was last modified.
        /// </summary>
        DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// True if the item has been deleted.  This is only used when soft-delete is set in
        /// <see cref="TableControllerOptions{T}"/>
        /// </summary>
        bool Deleted { get; set; }
    }
}
