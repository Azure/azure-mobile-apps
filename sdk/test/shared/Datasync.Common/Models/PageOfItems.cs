// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Datasync.Common.Models;

/// <summary>
/// The deserialized content from a paging operation.
/// </summary>
/// <typeparam name="T">The type of entity being transmitted</typeparam>
[ExcludeFromCodeCoverage]
public class PageOfItems<T> where T : class
{
    public T[] Items { get; set; }
    public long? Count { get; set; }
    public string NextLink { get; set; }
}
