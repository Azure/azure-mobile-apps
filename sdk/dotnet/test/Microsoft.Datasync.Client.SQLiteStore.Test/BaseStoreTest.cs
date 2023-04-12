// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.SQLiteStore.Test;

[ExcludeFromCodeCoverage]
public class BaseStoreTest : BaseTest
{
    protected BaseStoreTest()
    {
        List<JObject> items = new();
        for (int i = 0; i < 10; i++)
        {
            var item = new JObject()
            {
                { "id", Guid.NewGuid().ToString() },
                { "stringValue", $"item#{i}" }
            };
            items.Add(item);
        }
        IdEntityValues = items.ToArray();
    }

    /// <summary>
    /// Connection string to use.
    /// </summary>
    protected readonly string ConnectionString = "file:memory?mode=memory";

    /// <summary>
    /// Name of a test table to use.
    /// </summary>
    protected readonly string TestTable = "test";

    protected JObject JObjectWithAllTypes = new()
    {
        { "Object", new JObject() },
        { "Array", new JArray() },
        { "Integer", 0L },
        { "Float", 0f },
        { "String", String.Empty },
        { "Boolean", false },
        { "Date", DateTime.MinValue },
        { "Bytes", Array.Empty<byte>() },
        { "Guid", Guid.Empty },
        { "TimeSpan", TimeSpan.Zero },
        { "Uri", new Uri("http://localhost") }
    };

    protected JObject IdEntityDefinition = new()
    {
        { "id", string.Empty },
        { "stringValue", string.Empty }
    };

    protected JObject[] IdEntityValues { get; }
}
