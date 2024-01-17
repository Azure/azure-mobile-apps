// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client;

/// <summary>
/// Each entity supported by Azure Mobile Apps data synchronization must
/// implement <see cref="ITableData"/> to provide the proper metadata for
/// data synchronization.  This interface is the client-side version of the
/// server side <see cref="ITableData"/> interface.
/// </summary>
public interface IClientTableData
{
    /// <summary>
    /// A globally unique identifier for the entity.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// If <c>true</c>, the entity is deleted.  Required for soft-delete support.
    /// </summary>
    bool Deleted { get; set; }

    /// <summary>
    /// The date/time of the last update to the entity.  This must support msec resolution.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// An opaque blob that changes to indicate that the entity has been changed.
    /// </summary>
    byte[]? Version { get; set; }
}
