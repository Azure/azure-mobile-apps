// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Microsoft.Datasync.Integration.Test.KitchenSink;

[ExcludeFromCodeCoverage]
public abstract class BaseOperationContractResolverTest : BaseTestContractResolver, IDisposable
{
    protected readonly ITestOutputHelper Logger;
    protected readonly string? filename;
    protected readonly string connectionString;
    protected readonly OfflineSQLiteStore store;
    protected readonly DatasyncClient client;
    protected IOfflineTable<KitchenSinkDto>? offlineTable;
    protected IRemoteTable<KitchenSinkDto> remoteTable;
    protected ILogger<OfflineSQLiteStore> storeLoggerMock;

    protected BaseOperationContractResolverTest(ITestOutputHelper logger, bool useFile = true)
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

        storeLoggerMock = Substitute.For<ILogger<OfflineSQLiteStore>>();
        storeLoggerMock.Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
        store = new OfflineSQLiteStore(connectionString, storeLoggerMock);
        store.DefineTable<KitchenSinkDto>("kitchensink");
        client = GetMovieClient(store: store);
        remoteTable = client.GetRemoteTable<KitchenSinkDto>("kitchensink");
    }

    protected async Task InitializeAsync(bool pullItems = true)
    {
        await client.InitializeOfflineStoreAsync();

        offlineTable = client.GetOfflineTable<KitchenSinkDto>("kitchensink");

        if (pullItems)
        {
            await offlineTable.PullItemsAsync();
        }
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
