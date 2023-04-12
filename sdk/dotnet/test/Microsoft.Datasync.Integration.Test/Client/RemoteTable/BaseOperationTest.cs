// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable;

[ExcludeFromCodeCoverage]
public abstract class BaseOperationTest : BaseTest
{
    protected readonly DatasyncClient client;
    protected readonly IRemoteTable soft, table;
    protected readonly DateTimeOffset startTime = DateTimeOffset.UtcNow;

    protected BaseOperationTest()
    {
        client = GetMovieClient();
        table = client.GetRemoteTable("movies");
        soft = client.GetRemoteTable("soft");
    }

    protected static void AssertJsonDocumentMatches(EFMovie entity, JToken actual)
    {
        var serializer = new ServiceSerializer();
        var expected = (JObject)serializer.Serialize(entity);
        Assert.IsAssignableFrom<JObject>(actual);
        Assert.Equal(expected, (JObject)actual);
    }

    protected static void AssertVersionMatches(byte[] expected, string actual)
    {
        string expstr = Convert.ToBase64String(expected);
        Assert.Equal(expstr, actual);
    }
}
