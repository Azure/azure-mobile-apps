// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;

namespace Datasync.Common.Models;

/// <summary>
/// The client side of the table data.
/// </summary>
public class ClientTableData
{
    public ClientTableData()
    {
    }

    public ClientTableData(object source)
    {
        if (source is ITableData metadata)
        {
            Id = metadata.Id;
            UpdatedAt = metadata.UpdatedAt;
            Version = Convert.ToBase64String(metadata.Version);
            Deleted = metadata.Deleted;
        }
    }

    public string Id { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string Version { get; set; }
    public bool Deleted { get; set; }
}
