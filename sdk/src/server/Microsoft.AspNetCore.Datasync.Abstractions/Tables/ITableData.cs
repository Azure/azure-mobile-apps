// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// Each entity supported by Azure Mobile Apps data synchronization must
/// implement <see cref="ITableData"/> to provide the proper metadata for
/// data synchronization.
/// </summary>
public interface ITableData : IEquatable<ITableData>
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
    DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// An opaque blob that changes to indicate that the entity has been changed.
    /// </summary>
    byte[] Version { get; set; }
}
