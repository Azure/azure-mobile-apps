// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// The operation options for a query.
/// </summary>
public struct QueryOperationOptions
{
    /// <summary>
    /// If <c>true</c>, the query will include deleted items.
    /// </summary>
    public bool IncludeDeletedItems { get; set; }

    /// <summary>
    /// If <c>true</c>, the <see cref="Page{TEntity}.Count"/> will be retrieved from the server
    /// during this query.
    /// </summary>
    public bool IncludeTotalCount { get; set; }

    /// <summary>
    /// If set, an OData filter to use to restrict the dataset being returned.
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// If set, the entities will be ordered according to the provided list of orderings.
    /// </summary>
    public IEnumerable<Ordering>? OrderBy { get; set; }

    /// <summary>
    /// If set, the list of properties (camel-cased) will be requested from the servier.
    /// </summary>
    public IEnumerable<string>? Select { get; set; }

    /// <summary>
    /// Definition of an ordering clause for the <see cref="QueryOperationOptions.OrderBy"/> property.
    /// </summary>
    public struct Ordering
    {
        /// <summary>
        /// The property to sort by.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The direction of the ordering.
        /// </summary>
        public SortOrder SortOrder { get; set; }
    }

    /// <summary>
    /// The sort order for an ordering clause.
    /// </summary>
    public enum SortOrder
    {
        Ascending,
        Descending
    }
}
