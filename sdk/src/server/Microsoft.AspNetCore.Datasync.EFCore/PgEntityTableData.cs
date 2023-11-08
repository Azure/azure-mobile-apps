// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// A version of the <see cref="BaseEntityTableData"/> that is compatible with PostgreSQL.
/// </summary>
public class PgEntityTableData : BaseEntityTableData
{
    /// <inheritdoc />
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public override DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    [NotMapped]
    public override byte[] Version
    {
        get => BitConverter.GetBytes(RowVersion ?? 0);
        set => RowVersion = value.Length > 0 ? BitConverter.ToUInt32(value) : 0;
    }

    /// <summary>
    /// The value of the PostgreSQL "xmin" column.
    /// </summary>
    /// <see href="https://www.postgresql.org/docs/current/ddl-system-columns.html"/>
    [Timestamp, JsonIgnore]
    public uint? RowVersion { get; set; }
}
