﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Microsoft.Datasync.Integration.Test.KitchenSink;

[ExcludeFromCodeCoverage]
public abstract class BaseOperationTest : BaseTest, IDisposable
{
    protected readonly ITestOutputHelper Logger;
    protected readonly string? filename;
    protected readonly string connectionString;
    protected readonly OfflineSQLiteStore store;
    protected readonly DatasyncClient client;
    protected IOfflineTable<KitchenSinkDto>? offlineTable;
    protected IOfflineTable<ClientMovie>? offlineMovieTable;
    protected IRemoteTable<KitchenSinkDto> remoteTable;
    protected IRemoteTable<ClientMovie>? remoteMovieTable;
    protected ILogger<OfflineSQLiteStore> storeLoggerMock;

    protected BaseOperationTest(ITestOutputHelper logger, bool useFile = true)
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
        store.DefineTable<ClientMovie>("movies");
        client = GetMovieClient(store: store);
        remoteTable = client.GetRemoteTable<KitchenSinkDto>("kitchensink");
        remoteMovieTable = client.GetRemoteTable<ClientMovie>("movies");
    }

    protected async Task InitializeAsync(bool pullItems = true)
    {
        await client.InitializeOfflineStoreAsync();

        offlineTable = client.GetOfflineTable<KitchenSinkDto>("kitchensink");
        offlineMovieTable = client.GetOfflineTable<ClientMovie>("movies");

        if (pullItems)
        {
            await offlineTable.PullItemsAsync();
            await offlineMovieTable.PullItemsAsync();
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
