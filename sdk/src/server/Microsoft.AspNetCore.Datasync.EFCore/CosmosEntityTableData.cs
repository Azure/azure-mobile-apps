// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.EFCore;

/// <summary>
/// A version of the <see cref="BaseEntityTableData"/> that is compatible with Cosmos DB.
/// </summary>
public class CosmosEntityTableData : BaseEntityTableData
{
    /// <inheritdoc />
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public override DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    [NotMapped]
    public override byte[] Version
    {
        get { return Encoding.UTF8.GetBytes(this.EntityTag); }
        set { this.EntityTag = Encoding.UTF8.GetString(value); }
    }

    /// <summary>
    /// The Cosmos DB entity tag.
    /// </summary>
    [Timestamp, JsonIgnore]
    public string EntityTag { get; set; } = string.Empty;
}
