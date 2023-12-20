// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.Models;

/// <summary>
/// The model class for the response to a query request.
/// </summary>
public class PagedResult
{
    /// <summary>
    /// Creates a new <see cref="PagedResult"/> with some optional entities.
    /// </summary>
    /// <param name="items">The list of entities to include in the response.</param>
    public PagedResult(IEnumerable<object>? items = null)
    {
        Items = items ?? new List<object>();
    }

    /// <summary>
    /// The list of entities to include in the response.
    /// </summary>
    public IEnumerable<object> Items { get; set; }

    /// <summary>
    /// The count of all the entities to be returned by the search (without paging).
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// The arguments to retrieve the next page of items.  The client needs to prepend
    /// the URI of the table to this.
    /// </summary>
    public string? NextLink { get; set; }
}
