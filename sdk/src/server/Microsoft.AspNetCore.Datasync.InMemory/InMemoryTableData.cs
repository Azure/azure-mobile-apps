// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.InMemory;

/// <summary>
/// The base class for in-memory table data.
/// </summary>
public class InMemoryTableData : ITableData
{
    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool Deleted { get; set; } = false;

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    public byte[] Version { get; set; } = Array.Empty<byte>();

    /// <inheritdoc />
    public bool Equals(ITableData? other)
        => other != null && Id == other.Id && Version.SequenceEqual(other.Version);
}