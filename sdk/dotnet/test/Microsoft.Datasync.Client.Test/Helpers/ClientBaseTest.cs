// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Table;

namespace Microsoft.Datasync.Client.Test.Helpers;

[ExcludeFromCodeCoverage]
public class ClientBaseTest : BaseTest
{
    /// <summary>
    /// Creates a paging response.
    /// </summary>
    /// <param name="count">The count of elements to return</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="nextLink">The next link</param>
    /// <returns></returns>
    protected Page<IdEntity> CreatePageOfItems(int count, long? totalCount = null, Uri nextLink = null)
    {
        List<IdEntity> items = new();

        for (int i = 0; i < count; i++)
        {
            items.Add(new IdEntity { Id = Guid.NewGuid().ToString("N") });
        }
        var page = new Page<IdEntity> { Items = items, Count = totalCount, NextLink = nextLink };
        MockHandler.AddResponse(HttpStatusCode.OK, page);
        return page;
    }
}
