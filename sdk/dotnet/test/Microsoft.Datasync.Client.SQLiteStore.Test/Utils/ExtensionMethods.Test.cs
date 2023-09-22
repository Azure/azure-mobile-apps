// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.SQLiteStore.Utils;

namespace Microsoft.Datasync.Client.SQLiteStore.Test.Utils;

[ExcludeFromCodeCoverage]
public class ExtensionMethods_Tests
{
    [Fact]
    public void Split_Works()
    {
        List<string> items = new();
        for (int i = 0; i < 25; i++)
        {
            items.Add(i.ToString());
        }

        var actual = items.Split(10).ToList();

        Assert.Equal(items.Take(10).ToList(), actual[0].ToList());
        Assert.Equal(items.Skip(10).Take(10).ToList(), actual[1].ToList());
        Assert.Equal(items.Skip(20).ToList(), actual[2].ToList());
    }
}
