// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// A model representing a page of entities returned from the service.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being retrieved.</typeparam>
public class Page<TEntity>
{
    /// <summary>
    /// The entities being returned in this page of results.
    /// </summary>
    public IEnumerable<TEntity> Items { get; set; } = Array.Empty<TEntity>();

    /// <summary>
    /// If set, the total count of entities to be returned by the query without paging.
    /// </summary>
    public int? Count { get; set; }

    /// <summary>
    /// If set, there is another page and this is the query to use to retrieve it.
    /// </summary>
    public QueryOperationOptions? NextPage { get; set; }
}
