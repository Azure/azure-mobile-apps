using Datasync.Common.Test.Models;
using Datasync.Common.Test;
using Microsoft.Datasync.Client.SQLiteStore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.Datasync.Client;
using System.IO;
using Microsoft.Datasync.Integration.Test.Helpers;

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
            store = new OfflineSQLiteStore(connectionString);
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
