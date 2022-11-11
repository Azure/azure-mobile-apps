// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.SQLiteStore.Test
{
    [ExcludeFromCodeCoverage]
    public class OfflineSQLiteStore_Tests : BaseStoreTest
    {
        [Fact]
        public void Ctor_Works_WithValidConnectionString()
        {
            var store = new OfflineSQLiteStore(ConnectionString);

            Assert.NotNull(store);
            Assert.NotNull(store.DbConnection);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("http://localhost")]
        [InlineData("/c:/foo/bar")]

        public void Ctor_Throws_WithInvalidConnectionString(string sut)
        {
            Assert.ThrowsAny<ArgumentException>(() => new OfflineSQLiteStore(sut));
        }

        [Fact]
        public async Task InitializeAsync_InitializesDatabase()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            Assert.NotNull(store.DbConnection.connection);
        }

        [Fact]
        public async Task DeleteAsyncByQuery_Throws_WhenStoreIsNotInitialized()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.DeleteAsync(new QueryDescription(TestTable)));
        }

        [Fact]
        public async Task DeleteAsyncByQuery_Throws_OnMissingTable()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.DeleteAsync(new QueryDescription("movies")));
        }

        [Fact]
        public async Task DeleteAsyncByQuery_CanDeleteEntities()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            await store.UpsertAsync(TestTable, IdEntityValues, false);

            // Page before the deletion.
            var page = await store.GetPageAsync(new QueryDescription(TestTable) { IncludeTotalCount = true });
            Assert.Equal(IdEntityValues.Length, page.Count);
            Assert.Contains(page.Items, o => o.Value<string>("stringValue") == "item#1");

            QueryDescription query = QueryDescription.Parse(TestTable, "$filter=(stringValue eq 'item#1')");
            await store.DeleteAsync(query);

            // Page after the deletion.
            var page2 = await store.GetPageAsync(new QueryDescription(TestTable) { IncludeTotalCount = true });
            Assert.Equal(IdEntityValues.Length - 1, page2.Count);
            Assert.DoesNotContain(page2.Items, o => o.Value<string>("stringValue") == "item#1");
        }

        [Fact]
        public async Task DeleteAsyncById_EmptyListOfIds()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            await store.UpsertAsync(TestTable, IdEntityValues, false);

            // Page before the deletion.
            var page = await store.GetPageAsync(new QueryDescription(TestTable) { IncludeTotalCount = true });
            Assert.Equal(IdEntityValues.Length, page.Count);

            var ids = Array.Empty<string>();
            await store.DeleteAsync(TestTable, ids);

            // Page after the deletion.
            var page2 = await store.GetPageAsync(new QueryDescription(TestTable) { IncludeTotalCount = true });
            Assert.Equal(IdEntityValues.Length, page2.Count);
        }

        [Fact]
        public async Task DeleteAsyncById_CanDeleteEntities()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            await store.UpsertAsync(TestTable, IdEntityValues, false);

            // Page before the deletion.
            var page = await store.GetPageAsync(new QueryDescription(TestTable) { IncludeTotalCount = true });
            Assert.Equal(IdEntityValues.Length, page.Count);
            Assert.Contains(page.Items, o => o.Value<string>("stringValue") == "item#1");

            var ids = IdEntityValues.Skip(2).Take(2).Select(o => o.Value<string>("id")).ToArray();
            await store.DeleteAsync(TestTable, ids);

            // Page after the deletion.
            var page2 = await store.GetPageAsync(new QueryDescription(TestTable) { IncludeTotalCount = true });
            Assert.Equal(IdEntityValues.Length - 2, page2.Count);
            Assert.DoesNotContain(page2.Items, o => o.Value<string>("id") == ids[0]);
            Assert.DoesNotContain(page2.Items, o => o.Value<string>("id") == ids[1]);
        }

        [Fact]
        public async Task GetItemAsync_Throws_WhenStoreIsNotInitialized()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.GetItemAsync(TestTable, "id"));
        }

        [Fact]
        public async Task GetItemAsync_ReturnsNull_WhenMissing()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            await store.UpsertAsync(TestTable, IdEntityValues, false);

            // Page before the deletion.
            var result = await store.GetItemAsync(TestTable, Guid.NewGuid().ToString());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetItemAsync_ReturnsItem_WhenPresent()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            await store.UpsertAsync(TestTable, IdEntityValues, false);

            var expected = IdEntityValues.Skip(2).First();
            var result = await store.GetItemAsync(TestTable, expected.Value<string>("id"));
            Assert.Equal(expected.ToString(Formatting.None), result.ToString(Formatting.None));
        }

        [Fact]
        public async Task GetPageAsync_Throws_WhenStoreIsNotInitialized()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.GetPageAsync(new QueryDescription(TestTable)));
        }

        [Fact]
        public async Task UpsertAsync_ThenGetPageAsync_AllTypes()
        {
            using var store = new OfflineSQLiteStore(ConnectionString);

            store.DefineTable(TestTable, JObjectWithAllTypes);
            await store.InitializeAsync();

            var upserted = new JObject()
            {
                { "id", "xyz" },
                { "Object", new JObject() { {"id", "abc"} }},
                { "Array", new JArray() { new JObject(){{"id", 3}} } },
                { "Integer", 123L },
                { "Float", 12.5m },
                { "String", "def" },
                { "Boolean", true },
                { "Date", new DateTime(2003, 5, 6, 4, 5, 1, DateTimeKind.Utc) },
                { "Bytes", new byte[] { 1, 2, 3} },
                { "Guid", new Guid("AB3EB1AB-53CD-4780-928B-A7E1CB7A927C") },
                { "TimeSpan", new TimeSpan(1234) },
                { "Uri", new Uri("http://localhost/") }
            };
            await store.UpsertAsync(TestTable, new[] { upserted }, false);

            var query = new QueryDescription(TestTable);
            var page = await store.GetPageAsync(query);

            Assert.Single(page.Items);
            Assert.Equal(upserted.ToString(Formatting.None), page.Items.First().ToString(Formatting.None));
        }

        [Fact]
        public async Task UpsertAsync_NotDefinedTable_Throws()
        {
            using var store = new OfflineSQLiteStore(ConnectionString);

            store.DefineTable(TestTable, JObjectWithAllTypes);
            await store.InitializeAsync();

            var upserted = new JObject()
            {
                { "id", "xyz" },
                { "Object", new JObject() { {"id", "abc"} }},
                { "Array", new JArray() { new JObject(){{"id", 3}} } },
                { "Integer", 123L },
                { "Float", 12.5m },
                { "String", "def" },
                { "Boolean", true },
                { "Date", new DateTime(2003, 5, 6, 4, 5, 1, DateTimeKind.Utc) },
                { "Bytes", new byte[] { 1, 2, 3} },
                { "Guid", new Guid("AB3EB1AB-53CD-4780-928B-A7E1CB7A927C") },
                { "TimeSpan", new TimeSpan(1234) }
            };
            await Assert.ThrowsAsync<InvalidOperationException>(() => store.UpsertAsync("movies", new[] { upserted }, false));
        }

        [Fact]
        public async Task UpsertAsync_MissingColumn_Throws()
        {
            using var store = new OfflineSQLiteStore(ConnectionString);

            store.DefineTable(TestTable, JObjectWithAllTypes);
            await store.InitializeAsync();

            var upserted = new JObject()
            {
                { "id", "xyz" },
                { "Object", new JObject() { {"id", "abc"} }},
                { "Array", new JArray() { new JObject(){{"id", 3}} } },
                { "Integer", 123L },
                { "Float", 12.5m },
                { "String", "def" },
                { "Boolean", true },
                { "SomeExtraField", new DateTime(2003, 5, 6, 4, 5, 1, DateTimeKind.Utc) },
                { "Bytes", new byte[] { 1, 2, 3} },
                { "Guid", new Guid("AB3EB1AB-53CD-4780-928B-A7E1CB7A927C") },
                { "TimeSpan", new TimeSpan(1234) }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => store.UpsertAsync(TestTable, new[] { upserted }, false));
        }

        [Fact]
        public async Task UpsertAsync_Returns_OnEmptyItems()
        {
            using var store = new OfflineSQLiteStore(ConnectionString);

            store.DefineTable(TestTable, JObjectWithAllTypes);
            await store.InitializeAsync();
            var upserted = Array.Empty<JObject>();

            await store.UpsertAsync(TestTable, upserted, false);

            var query = new QueryDescription(TestTable);
            var page = await store.GetPageAsync(query);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task UpsertAsync_Returns_OnNoColumns()
        {
            using var store = new OfflineSQLiteStore(ConnectionString);

            store.DefineTable(TestTable, JObjectWithAllTypes);
            await store.InitializeAsync();
            var upserted = new JObject[] { new JObject() };
            await store.UpsertAsync(TestTable, upserted, false);

            var query = new QueryDescription(TestTable);
            var page = await store.GetPageAsync(query);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task TableModificationsWork()
        {
            var filename = Path.GetTempFileName();
            var uri = new UriBuilder(filename) { Query = "?mode=rwc" }.Uri.ToString();

            Assert.StartsWith("file://", uri);
            try
            {
                JObject table = new()
                {
                    { "id", string.Empty },
                    { "stringValue", string.Empty }
                };

                // Create the first store
                var store1 = new OfflineSQLiteStore(uri);
                store1.DefineTable(TestTable, table);
                await store1.InitializeAsync();

                var results1 = await store1.ExecuteQueryAsync(new JObject(), $"PRAGMA table_info({TestTable})");

                // Do Assertions here
                Assert.Equal(table.Properties().Count(), results1.Count);
                foreach (var prop in table.Properties())
                {
                    Assert.Contains(prop.Name, results1.Select(o => o.Value<string>("name")));
                }

                // Clean up
                store1.DbConnection.connection.Close();

                // Create the second store.
                table["addedColumn"] = string.Empty;
                var store2 = new OfflineSQLiteStore(uri);
                store2.DefineTable(TestTable, table);
                await store2.InitializeAsync();

                var results2 = await store2.ExecuteQueryAsync(new JObject(), $"PRAGMA table_info({TestTable})");

                // Do Assertions here
                Assert.Equal(table.Properties().Count(), results2.Count);
                foreach (var prop in table.Properties())
                {
                    Assert.Contains(prop.Name, results2.Select(o => o.Value<string>("name")));
                }

                // Clean up
                store2.DbConnection.connection.Close();

                // Create the first store
                table.Remove("stringValue");
                var store3 = new OfflineSQLiteStore(uri);
                store3.DefineTable(TestTable, table);
                await store3.InitializeAsync();

                var results3 = await store3.ExecuteQueryAsync(new JObject(), $"PRAGMA table_info({TestTable})");

                // Do Assertions here
                Assert.Equal(table.Properties().Count(), results3.Count);
                foreach (var prop in table.Properties())
                {
                    Assert.Contains(prop.Name, results3.Select(o => o.Value<string>("name")));
                }

                // Clean up
                store3.DbConnection.connection.Close();
            }
            finally
            {
                File.Delete(filename);
            }
        }

        [Fact]
        public async Task ExecuteQueryAsync_CanQueryTable()
        {
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();
            await store.UpsertAsync(TestTable, IdEntityValues, false);

            // Page before the deletion.
            var parameters = new Dictionary<string, object>() { { "@p", "item#1" } };
            var items = await store.ExecuteQueryAsync(TestTable, $"SELECT [id] FROM [{TestTable}] WHERE [stringValue] = @p", parameters);
            Assert.Single(items);
            var id = IdEntityValues.Single(o => o.Value<string>("stringValue") == "item#1").Value<string>("id");
            Assert.Contains(id, items.Select(o => o.Value<string>("id")));
        }

        [Fact]
        public async Task DeltaTokenStore_StoresWithMSAccuracy()
        {
            // Set up the default store and client.
            var store = new OfflineSQLiteStore(ConnectionString);
            var client = new DatasyncClient("https://localhost/", new DatasyncClientOptions { OfflineStore = store });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            var deltaTokenStore = context.DeltaTokenStore;
            var deltaToken = DateTimeOffset.Parse("2022-08-01T13:48:23.123Z");
            Assert.Equal(123, deltaToken.Millisecond);      // Just a double-check for us.

            // Store the deltaToken
            await deltaTokenStore.SetDeltaTokenAsync("testtable", "testquery", deltaToken);

            // Now we will deliberately invalidate the cache so that we aren't fooled by the cache.
            await deltaTokenStore.InvalidateCacheAsync("testtable", "testquery");

            // Get the deltaToken back
            var storedToken = await deltaTokenStore.GetDeltaTokenAsync("testtable", "testquery");
            Assert.Equal(storedToken.Millisecond, deltaToken.Millisecond);
        }

        [Fact]
        public async Task GetTablesAsync_ReturnsListOfTables()
        {
            // Set up the default store and client.
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable(TestTable, IdEntityDefinition);
            var client = new DatasyncClient("https://localhost/", new DatasyncClientOptions { OfflineStore = store });
            var context = new SyncContext(client, store);
            await context.InitializeAsync();

            var tables = await context.OfflineStore.GetTablesAsync();
            Assert.Equal(1, tables.Count);  // If it's 4, then the system tables are being returned as well.
            Assert.Equal(TestTable, tables[0]);
        }

        [Fact]
        public async Task Dispose_ReleasesFileHandle()
        {
            // Set up store as a file.
            var dbFile = Path.Join(Path.GetTempPath(), "test-release.db");
            var store = new OfflineSQLiteStore($"file:///{dbFile}");
            store.DefineTable(TestTable, IdEntityDefinition);
            await store.InitializeAsync();

            // Act - dispose the store
            store.Dispose();

            // Assert - Should be able to File.Delete the store file.
            File.Delete(dbFile);   // This should not throw.
            Assert.False(File.Exists(dbFile), $"{dbFile} still exists but was deleted.");
        }

        /// <summary>
        /// Issue 499 - using ExecuteQueryAsync on an offline database will return IList{JObject}.
        /// Deserializing the returned JObjects generates a JsonSerializationException for items
        /// that were created with InsertItemAsync(T item) that have not yet been pushed.  This is
        /// because the three columns (deleted, updatedAt, and version) are all null.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Issue499()
        {
            // Set up a client and a store.
            // Set up the default store and client.
            var store = new OfflineSQLiteStore(ConnectionString);
            store.DefineTable<ClientMovie>("movies");
            var client = new DatasyncClient("https://localhost/", new DatasyncClientOptions { OfflineStore = store });
            await client.InitializeOfflineStoreAsync();
            var offlineTable = client.GetOfflineTable<ClientMovie>("movies");

            // Insert a new item in the database
            var testItem = GetSampleMovie<ClientMovie>();
            await offlineTable.InsertItemAsync(testItem);

            // Execute executeQueryAsync on the offline table.
            var sqlStatement = $"SELECT * FROM movies WHERE id = @id";
            var queryParams = new Dictionary<string, object>
            {
                { "@id", testItem.Id }
            };
            var result = await store.ExecuteQueryAsync("movies", sqlStatement, queryParams);

            // Step 3 - deserialize the content
            var movies = result.Select(obj => client.Serializer.Deserialize<ClientMovie>(obj)).ToArray();

            // This should be the movie setting
            Assert.Single(movies);

            var movieResult = movies[0]!;
            Assert.Equal<IMovie>(testItem, movieResult);
            Assert.Equal(testItem.Id, movieResult.Id);
        }
    }
}