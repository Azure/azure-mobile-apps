// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Table.Operations.OfflineTable;

[ExcludeFromCodeCoverage]
public class BaseOperationTest : ClientBaseTest
{
    protected readonly MockOfflineStore store;
    protected readonly IOfflineTable table, authTable;

    public BaseOperationTest() : base()
    {
        store = new MockOfflineStore();
        table = GetMockClient(null, store).GetOfflineTable("movies");
        authTable = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken), store).GetOfflineTable("movies");
    }

    /// <summary>
    /// A set of invalid IDs for testing
    /// </summary>
    public static IEnumerable<object[]> GetInvalidIds() => new List<object[]>
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "\t" },
        new object[] { "abcdef gh" },
        new object[] { "!!!" },
        new object[] { "?" },
        new object[] { ";" },
        new object[] { "{EA235ADF-9F38-44EA-8DA4-EF3D24755767}" },
        new object[] { "###" }
    };

    /// <summary>
    /// Stores the item provided in the table.
    /// </summary>
    /// <typeparam name="T">The type of the item.</typeparam>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="item">The item to store.</param>
    public JObject StoreInTable<T>(string tableName, T item)
    {
        var instance = (JObject)GetMockClient().Serializer.Serialize(item);
        store.Upsert(tableName, new[] { instance });
        return instance;
    }
}
