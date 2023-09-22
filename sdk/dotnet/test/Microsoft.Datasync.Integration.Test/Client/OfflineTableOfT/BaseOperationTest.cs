// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTableOfT;

[ExcludeFromCodeCoverage]
public abstract class BaseOperationTest : BaseTest, IDisposable
{
    internal readonly ITestOutputHelper? Logger;
    internal readonly string? filename;
    internal readonly string connectionString;
    internal readonly OfflineSQLiteStore store;
    internal readonly DatasyncClient client;
    internal IOfflineTable<ClientMovie>? soft, table;

    internal JObject MovieDefinition = new()
    {
        { "id", string.Empty },
        { "deleted", false },
        { "updatedAt", DateTimeOffset.UtcNow },
        { "version", string.Empty },
        { "bestPictureWinner", false },
        { "duration", 0 },
        { "rating", string.Empty },
        { "releaseDate", DateTimeOffset.UtcNow },
        { "title", string.Empty },
        { "year", 0 }
    };

    internal BaseOperationTest(ITestOutputHelper logger, bool useFile = true)
    {
        Logger = logger;
        if (useFile)
        {
            filename = Path.GetTempFileName();
            connectionString = new UriBuilder(filename) { Query = "?mode=rwc" }.Uri.ToString();
        }
        else
        {
            connectionString = "file:in-memory.db?mode=memory";
        }
        store = new OfflineSQLiteStore(connectionString);
        store.DefineTable<ClientMovie>("movies");
        store.DefineTable<ClientMovie>("soft");
        client = GetMovieClient(store: store);
    }

    internal BaseOperationTest(bool useFile = true)
    {
        if (useFile)
        {
            filename = Path.GetTempFileName();
            connectionString = new UriBuilder(filename) { Query = "?mode=rwc" }.Uri.ToString();
        }
        else
        {
            connectionString = "file:in-memory.db?mode=memory";
        }
        store = new OfflineSQLiteStore(connectionString);
        store.DefineTable<ClientMovie>("movies");
        store.DefineTable<ClientMovie>("soft");
        client = GetMovieClient(store: store);
    }

    internal async Task InitializeAsync(bool pullItems = true)
    {
        await client.InitializeOfflineStoreAsync();

        table = client.GetOfflineTable<ClientMovie>("movies");
        soft = client.GetOfflineTable<ClientMovie>("soft");

        if (pullItems)
        {
            await table.PullItemsAsync();
            await soft.PullItemsAsync();
        }
    }

    internal static void AssertSystemPropertiesMatch(EFMovie expected, ClientMovie actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Deleted, actual.Deleted);
        AssertVersionMatches(expected.Version, actual.Version);
        Assert.Equal(expected.UpdatedAt.ToUnixTimeMilliseconds(), actual.UpdatedAt.ToUnixTimeMilliseconds());
    }

    internal static void AssertSystemPropertiesMatch(EFMovie expected, JObject actual)
    {
        Assert.Equal(expected.Id, actual.Value<string>("id"));
        Assert.Equal(expected.Deleted, actual.Value<bool>("deleted"));
        Assert.Equal(Convert.ToBase64String(expected.Version), actual.Value<string>("version"));
    }

    internal async Task ModifyServerVersionAsync(string id)
    {
        var remoteTable = client.GetRemoteTable<ClientMovie>("movies");
        var item = await remoteTable!.GetItemAsync(id);
        var temp = item.Title;
        item.Title = "Foo";
        await remoteTable!.ReplaceItemAsync(item);
        item.Title = temp;
        await remoteTable!.ReplaceItemAsync(item);
    }

    internal static void AssertVersionMatches(byte[] expected, string actual)
    {
        string expstr = Convert.ToBase64String(expected);
        Assert.Equal(expstr, actual);
    }

    public void Dispose()
    {
        store.DbConnection.connection.Close();
        if (filename != null)
        {
            File.Delete(filename);
        }
        GC.SuppressFinalize(this);
    }
}
