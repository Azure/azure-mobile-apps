// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Castle.Core.Logging;
using Datasync.Common.Test;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.SQLiteStore;
using Microsoft.Datasync.Integration.Test.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.Datasync.Integration.Test.KitchenSink
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseOperationTest : BaseTest, IDisposable
    {
        protected readonly ITestOutputHelper Logger;
        protected readonly string? filename;
        protected readonly string connectionString;
        protected readonly OfflineSQLiteStore store;
        protected readonly DatasyncClient client;
        protected IOfflineTable<KitchenSinkDto>? offlineTable;
        protected IRemoteTable<KitchenSinkDto> remoteTable;
        protected Mock<ILogger<OfflineSQLiteStore>> storeLoggerMock;

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

            storeLoggerMock = new Mock<ILogger<OfflineSQLiteStore>>();
            storeLoggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception?, string>>()));
            store = new OfflineSQLiteStore(connectionString, storeLoggerMock.Object);
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
}
