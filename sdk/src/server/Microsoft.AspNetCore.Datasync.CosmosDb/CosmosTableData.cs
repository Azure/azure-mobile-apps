// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.CosmosDb;

public class CosmosTableData : ITableData
{
    #region ITableData
    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool Deleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UnixEpoch;

    /// <inheritdoc />
    [JsonIgnore]
    public byte[] Version
    {
        get { return Encoding.UTF8.GetBytes(EntityTag); }
        set { EntityTag = Encoding.UTF8.GetString(value); }
    }
    #endregion

    /// <summary>
    /// The Cosmos DB entity tag value.
    /// </summary>
    [JsonPropertyName("_etag")]
    public string EntityTag { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool Equals(ITableData? other)
        => other != null && Id == other.Id && Version.SequenceEqual(other.Version);
}