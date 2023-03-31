// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Datasync.Common.Test.Models;

/// <summary>
/// The deserialized content from a paging operation.
/// </summary>
/// <typeparam name="T"></typeparam>
[ExcludeFromCodeCoverage]
public class PageOfItems<T> where T : class
{
    public T[] Items { get; set; }
    public long? Count { get; set; }
    public Uri NextLink { get; set; }
}

/// <summary>
/// The same thing as above, but with a string NextLink instead of a Uri
/// </summary>
/// <typeparam name="T"></typeparam>
[ExcludeFromCodeCoverage]
public class StringNextLinkPage<T> where T : class
{
    public T[] Items { get; set; }
    public long? Count { get; set; }
    public string NextLink { get; set; }
}

/// <summary>
/// The V2 version of the page of items.
/// </summary>
/// <typeparam name="T"></typeparam>
[ExcludeFromCodeCoverage]
public class V2PageOfItems<T> where T : class
{
    public T[] Results { get; set; }
    public long? Count { get; set; }
}
