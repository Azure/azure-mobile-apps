// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// The base class for entity framework core-based table data.
/// </summary>
public abstract class BaseEntityTableData : ITableData
{
    /// <inheritdoc />
    [Key]
    public virtual string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public virtual bool Deleted { get; set; } = false;

    /// <inheritdoc />
    public virtual DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    public virtual byte[] Version { get; set; } = Array.Empty<byte>();

    /// <inheritdoc />
    public bool Equals(ITableData? other)
        => other != null && Id == other.Id && Version.SequenceEqual(other.Version);
}