// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client;

/// <summary>
/// Definition for the response from the datasync service.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being returned.</typeparam>
public class Page<TEntity>
{
    /// <summary>
    /// The list of entities to include in the response.
    /// </summary>
    [JsonPropertyName("items")]
    public IEnumerable<TEntity> Items { get; set; } = Enumerable.Empty<TEntity>();

    /// <summary>
    /// The count of all the entities to be returned by the search (without paging).
    /// </summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    /// <summary>
    /// The arguments to retrieve the next page of items.  The client needs to prepend
    /// the URI of the table to this.
    /// </summary>
    [JsonPropertyName("nextLink")]
    public string? NextLink { get; set; }
}
