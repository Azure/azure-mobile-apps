// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// A version of the <see cref="BaseEntityTableData"/> that is compatible with most Entity Framework Core drivers.
/// </summary>
public class EntityTableData : BaseEntityTableData
{
    /// <inheritdoc />
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public override DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    [Timestamp]
    public override byte[] Version { get; set; } = Array.Empty<byte>();
}
