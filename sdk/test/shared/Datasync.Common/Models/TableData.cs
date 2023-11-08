// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Common;

/// <summary>
/// A concrete implementation of ITableData with no data.
/// </summary>
[ExcludeFromCodeCoverage]
public class TableData : ITableData
{
    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool Deleted { get; set; } = false;

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    public byte[] Version { get; set; } = Array.Empty<byte>();

    public bool Equals(ITableData other)
        => other is not null && Id == other.Id && Version.SequenceEqual(other.Version);
}
