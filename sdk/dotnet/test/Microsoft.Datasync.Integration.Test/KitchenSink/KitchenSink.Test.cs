using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.SQLiteStore;
using Microsoft.Datasync.Integration.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using Movies = Datasync.Common.Test.TestData.Movies;

namespace Microsoft.Datasync.Integration.Test.KitchenSink
{
    /// <summary>
    /// A set of tests against the kitchen sink table - these are used for
    /// specific client/server interactions where we can set up the entire
    /// table alone.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class KitchenSink_Tests : BaseOperationTest
    {
        public KitchenSink_Tests(ITestOutputHelper logger) : base(logger) 
        { 
        }

        [Fact]
        public async Task KS1_NullRoundtrips()
        {
            // On client 1
            KitchenSinkDto client1dto = new() { StringValue = "This is a string" };
            await remoteTable.InsertItemAsync(client1dto);
            var remoteId = client1dto.Id;
            Assert.NotEmpty(remoteId);

            // On client 2
            await InitializeAsync();
            var pullQuery = offlineTable!.CreateQuery();
            await offlineTable!.PullItemsAsync(pullQuery, new PullOptions());
            var client2dto = await offlineTable.GetItemAsync(remoteId);
            Assert.NotNull(client2dto);
            Assert.Equal("This is a string", client2dto.StringValue);

            // Now update client 1
            client1dto = await remoteTable.GetItemAsync(remoteId);
            Assert.NotNull(client1dto);
            client1dto.StringValue = null;
            await remoteTable.ReplaceItemAsync(client1dto);

            // Finally, download the value on client 2
            await offlineTable!.PullItemsAsync();
            client2dto = await offlineTable.GetItemAsync(remoteId);
            Assert.NotNull(client2dto);
            // Issue 408 - cannot replace a string with a null.
            Assert.Null(client2dto.StringValue);
        }

        [Fact]
        public async Task KS2_DeferredTableDefinition()
        {
            var filename = Path.GetTempFileName();
            var connectionString = new UriBuilder(filename) { Query = "?mode=rwc" }.Uri.ToString();
            var store = new OfflineSQLiteStore(connectionString);
            var client = GetMovieClient(store: store);

            var table = client.GetOfflineTable<ClientMovie>("movies");

            var itemCount = await table.GetAsyncItems().CountAsync();
            Assert.Equal(0, itemCount);

            await table.PullItemsAsync();

            itemCount = await table.GetAsyncItems().CountAsync();
            Assert.Equal(Movies.Count, itemCount);
        }
    }
}
