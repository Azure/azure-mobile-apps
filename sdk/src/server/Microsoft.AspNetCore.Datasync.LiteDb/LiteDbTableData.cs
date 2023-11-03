// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using LiteDB;

namespace Microsoft.AspNetCore.Datasync.LiteDb;

/// <summary>
/// An implementation of the <see cref="ITableData"/> interface for handling
/// entities stored in a LiteDB database.
/// </summary>
public class LiteDbTableData : ITableData
{
    /// <inheritdoc />
    [BsonId]
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool Deleted { get; set; } = false;

    /// <inheritdoc />
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    public byte[] Version { get; set; } = Array.Empty<byte>();

    /// <inheritdoc />
    public bool Equals(ITableData? other)
        => other != null && Id == other.Id && Version.SequenceEqual(other.Version);
}