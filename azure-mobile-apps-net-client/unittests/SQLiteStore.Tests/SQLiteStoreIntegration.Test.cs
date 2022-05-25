// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLiteStore.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SQLiteStore.Tests
{
    public class SQLiteStoreIntegration
    {
        private const string TestTable = "stringId_test_table";
        private static string TestDbName = "integration-test.db";

        [Fact]
        public async Task InsertAsync_Throws_IfItemAlreadyExistsInLocalStore()
        {
            ResetDatabase(TestTable);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(TestTable, new JObject()
            {
                { "id", String.Empty},
                { "String", String.Empty }
            });

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = await CreateClient(hijack, store);

            string pullResult = "[{\"id\":\"abc\",\"String\":\"Wow\"}]";
            hijack.AddResponseContent(pullResult);
            hijack.AddResponseContent("[]");

            IMobileServiceSyncTable table = service.GetSyncTable(TestTable);
            await table.PullAsync(null, null);

            var ex = await Assert.ThrowsAsync<MobileServiceLocalStoreException>(() => table.InsertAsync(new JObject() { { "id", "abc" } }));

            Assert.Equal("An insert operation on the item is already in the queue.", ex.Message);
        }

        [Fact]
        public async Task ReadAsync_RoundTripsDate()
        {
            string tableName = "itemWithDate";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(tableName, new JObject()
            {
                { "id", String.Empty},
                { "date", DateTime.Now }
            });

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable table = service.GetSyncTable(tableName);

            DateTime theDate = new DateTime(2014, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            JObject inserted = await table.InsertAsync(new JObject() { { "date", theDate } });

            Assert.Equal(inserted["date"].Value<DateTime>(), theDate);

            JObject rehydrated = await table.LookupAsync(inserted["id"].Value<string>());

            Assert.Equal(rehydrated["date"].Value<DateTime>(), theDate);
        }

        [Fact]
        public async Task ReadAsync_RoundTripsDate_Generic()
        {
            string tableName = "NotSystemPropertyCreatedAtType";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<NotSystemPropertyCreatedAtType>();

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<NotSystemPropertyCreatedAtType> table = service.GetSyncTable<NotSystemPropertyCreatedAtType>();

            DateTime theDate = new DateTime(2014, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            var inserted = new NotSystemPropertyCreatedAtType() { __CreatedAt = theDate };
            await table.InsertAsync(inserted);

            Assert.Equal(inserted.__CreatedAt.ToUniversalTime(), theDate);

            NotSystemPropertyCreatedAtType rehydrated = await table.LookupAsync(inserted.Id);

            Assert.Equal(rehydrated.__CreatedAt.ToUniversalTime(), theDate);
        }

        [Fact]
        public async Task ReadAsync_RoundTripsBytes()
        {
            const string tableName = "bytes_test_table";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable(tableName, new JObject {
                { "id", String.Empty },
                { "data", new byte[0] }
            });

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable table = service.GetSyncTable(tableName);

            byte[] theData = { 0, 128, 255 };

            JObject inserted = await table.InsertAsync(new JObject { { "data", theData } });

            Assert.Equal(theData, inserted["data"].Value<byte[]>());

            JObject rehydrated = await table.LookupAsync(inserted["id"].Value<string>());

            Assert.Equal(theData, rehydrated["data"].Value<byte[]>());
        }

        [Fact]
        public async Task ReadAsync_RoundTripsBytes_Generic()
        {
            const string tableName = "BytesType";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<BytesType>();

            var hijack = new TestHttpHandler();
            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<BytesType> table = service.GetSyncTable<BytesType>();

            byte[] theData = { 0, 128, 255 };

            BytesType inserted = new BytesType { Data = theData };

            await table.InsertAsync(inserted);

            Assert.Equal(inserted.Data, theData);

            BytesType rehydrated = await table.LookupAsync(inserted.Id);

            Assert.Equal(rehydrated.Data, theData);
        }

        [Fact]
        public async Task ReadAsync_WithSystemPropertyType_Generic()
        {
            string tableName = "stringId_test_table";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"{""id"": ""123"", ""version"": ""xyz""}");
            IMobileServiceClient service = await CreateClient(hijack, store);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            var inserted = new ToDoWithSystemPropertiesType()
            {
                Id = "123",
                Version = "abc",
                String = "def"
            };
            await table.InsertAsync(inserted);

            Assert.Equal("abc", inserted.Version);

            await service.SyncContext.PushAsync();

            ToDoWithSystemPropertiesType rehydrated = await table.LookupAsync(inserted.Id);

            Assert.Equal("xyz", rehydrated.Version);

            string expectedRequestContent = @"{""id"":""123"",""String"":""def""}";
            // version should not be sent with insert request
            Assert.Equal(hijack.RequestContents[0], expectedRequestContent);
        }

        [Fact]
        public async Task ReadAsync_StringCompare_WithSpecialChars()
        {
            ResetDatabase("stringId_test_table");

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            IMobileServiceClient service = await CreateClient(new TestHttpHandler(), store);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            var inserted = new ToDoWithSystemPropertiesType()
            {
                Id = "123",
                Version = "abc",
                String = "test@contoso.com"
            };
            await table.InsertAsync(inserted);

            ToDoWithSystemPropertiesType rehydrated = (await table.Where(t => t.String == "test@contoso.com").ToListAsync()).FirstOrDefault();

            Assert.Equal("test@contoso.com", rehydrated.String);
        }

        [Fact]
        public async Task DefineTable_IgnoresColumn_IfCaseIsDifferentButNameIsSame()
        {
            string tableName = "itemWithDate";

            ResetDatabase(tableName);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(tableName, new JObject()
                {
                    { "id", String.Empty},
                    { "date", DateTime.Now }
                });

                var hijack = new TestHttpHandler();
                await CreateClient(hijack, store);
            }

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(tableName, new JObject()
                {
                    { "id", String.Empty},
                    { "DaTE", DateTime.Now } // the casing of date is different here
                });

                var hijack = new TestHttpHandler();
                await CreateClient(hijack, store);
            }
        }

        [Fact]
        public async Task Upsert_Succeeds_IfCaseIsDifferentButNameIsSame()
        {
            string tableName = "itemWithDate";

            ResetDatabase(tableName);

            using (var store = new MobileServiceSQLiteStore(TestDbName))
            {
                store.DefineTable(tableName, new JObject()
                {
                    { "id", String.Empty},
                    { "date", DateTime.Now }
                });

                await store.InitializeAsync();

                await store.UpsertAsync("ITEMwithDATE", new[]
                {
                    new JObject()
                    {
                        { "ID", Guid.NewGuid() },
                        {"dATE", DateTime.UtcNow }
                    }
                }, ignoreMissingColumns: false);
            }
        }

        [Fact]
        public async Task PullAsync_DoesIncrementalSync_WhenQueryIdIsSpecified()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            IMobileServiceSyncTable<ToDoWithStringId> table = await GetSynctable<ToDoWithStringId>(hijack);

            string pullResult = "[{\"id\":\"b\",\"String\":\"Wow\",\"version\":\"def\", \"updatedAt\":\"2014-01-30T23:01:33.444Z\"}]";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(pullResult) }); // pull
            hijack.AddResponseContent("[]");

            await table.PullAsync(queryId: "todoItems", query: table.CreateQuery());
            QueryEquals(hijack.Requests[0].RequestUri.Query, "?$filter=(updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.0000000%2B00%3A00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true");


            pullResult = "[{\"id\":\"b\",\"String\":\"Updated\",\"version\":\"def\", \"updatedAt\":\"2014-02-27T23:01:33.444Z\"}]";
            hijack.AddResponseContent(pullResult); // pull
            hijack.AddResponseContent("[]");

            await table.PullAsync(queryId: "todoItems", query: table.CreateQuery());

            var item = await table.LookupAsync("b");
            Assert.Equal("Updated", item.String);
            QueryEquals(hijack.Requests[2].RequestUri.Query, "?$filter=(updatedAt%20ge%20datetimeoffset'2014-01-30T23%3A01%3A33.4440000%2B00%3A00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_DoesIncrementalSync_WhenQueryIdIsSpecified_WithoutCache()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            IMobileServiceSyncTable<ToDoWithStringId> table = await GetSynctable<ToDoWithStringId>(hijack);

            string pullResult = "[{\"id\":\"b\",\"String\":\"Wow\",\"version\":\"def\", \"updatedAt\":\"2014-01-30T23:01:33.444Z\"}]";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(pullResult) }); // pull
            hijack.AddResponseContent("[]");

            await table.PullAsync(queryId: "todoItems", query: table.CreateQuery());
            QueryEquals(hijack.Requests[0].RequestUri.Query, "?$filter=(updatedAt%20ge%20datetimeoffset'1970-01-01T00%3A00%3A00.0000000%2B00%3A00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true");

            table = await GetSynctable<ToDoWithStringId>(hijack);


            pullResult = "[{\"id\":\"b\",\"String\":\"Updated\",\"version\":\"def\", \"updatedAt\":\"2014-02-27T23:01:33.444Z\"}]";
            hijack.AddResponseContent(pullResult); // pull
            hijack.AddResponseContent("[]");

            await table.PullAsync(queryId: "todoItems", query: table.CreateQuery());

            var item = await table.LookupAsync("b");
            Assert.Equal("Updated", item.String);
            QueryEquals(hijack.Requests[2].RequestUri.Query, "?$filter=(updatedAt%20ge%20datetimeoffset'2014-01-30T23%3A01%3A33.4440000%2B00%3A00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_RequestsSystemProperties_WhenDefinedOnTableType()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            IMobileServiceSyncTable<ToDoWithSystemPropertiesType> table = await GetSynctable<ToDoWithSystemPropertiesType>(hijack);

            string pullResult = "[{\"id\":\"b\",\"String\":\"Wow\",\"version\":\"def\",\"createdAt\":\"2014-01-29T23:01:33.444Z\", \"updatedAt\":\"2014-01-30T23:01:33.444Z\"}]";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(pullResult) }); // pull
            hijack.AddResponseContent("[]");

            await table.PullAsync(null, null);

            var item = await table.LookupAsync("b");
            Assert.Equal("Wow", item.String);
            Assert.Equal("def", item.Version);
            // we preserved the system properties returned from server on update
            Assert.Equal(item.CreatedAt.ToUniversalTime(), new DateTime(2014, 01, 29, 23, 1, 33, 444, DateTimeKind.Utc));
            Assert.Equal(item.UpdatedAt.ToUniversalTime(), new DateTime(2014, 01, 30, 23, 1, 33, 444, DateTimeKind.Utc));

            // we request all the system properties present on DefineTable<> object
            QueryEquals(hijack.Requests[0].RequestUri.Query, "?$skip=0&$top=50&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_DoesNotTriggerPush_OnUnrelatedTables_WhenThereIsOperationTable()
        {
            ResetDatabase(TestTable);
            TestUtilities.DropTestTable(TestDbName, "unrelatedTable");
            TestUtilities.DropTestTable(TestDbName, "relatedTable");
            TestUtilities.DropTestTable(TestDbName, "StringIdType");

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable("unrelatedTable", new JObject() { { "id", String.Empty } });
            store.DefineTable<StringIdType>();

            IMobileServiceClient client = await CreateClient(hijack, store);

            // insert item in pull table
            IMobileServiceSyncTable unrelatedTable = client.GetSyncTable("unrelatedTable");
            await unrelatedTable.InsertAsync(new JObject() { { "id", "abc" } });

            // then insert item in other table
            MobileServiceSyncTable<StringIdType> mainTable = client.GetSyncTable<StringIdType>() as MobileServiceSyncTable<StringIdType>;
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await mainTable.InsertAsync(item);

            await mainTable.PullAsync(null, null, null, null, CancellationToken.None, null, "relatedTable");

            Assert.Equal(3, hijack.Requests.Count); // 1 for push and 2 for pull
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/StringIdType");
            Assert.Equal(1L, client.SyncContext.PendingOperations);
        }

        [Fact]
        public async Task PullAsync_DoesNotTriggerPush_WhenPushOtherTablesIsFalse()
        {
            ResetDatabase(TestTable);
            TestUtilities.DropTestTable(TestDbName, "unrelatedTable");
            TestUtilities.DropTestTable(TestDbName, "StringIdType");

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable("unrelatedTable", new JObject() { { "id", String.Empty } });
            store.DefineTable<StringIdType>();

            IMobileServiceClient client = await CreateClient(hijack, store);

            // insert item in pull table
            IMobileServiceSyncTable unrelatedTable = client.GetSyncTable("unrelatedTable");
            await unrelatedTable.InsertAsync(new JObject() { { "id", "abc" } });

            // then insert item in other table
            MobileServiceSyncTable<StringIdType> mainTable = client.GetSyncTable<StringIdType>() as MobileServiceSyncTable<StringIdType>;
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await mainTable.InsertAsync(item);

            await mainTable.PullAsync(null, null, null, false, CancellationToken.None);

            Assert.Equal(3, hijack.Requests.Count); // 1 for push and 2 for pull
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/StringIdType");
            Assert.Equal(1L, client.SyncContext.PendingOperations);
        }

        [Fact]
        public async Task PullAsync_TriggersPush_OnRelatedTables_WhenThereIsOperationTable()
        {
            ResetDatabase(TestTable);
            TestUtilities.DropTestTable(TestDbName, "relatedTable");
            TestUtilities.DropTestTable(TestDbName, "StringIdType");

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hi\"}");
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable("relatedTable", new JObject() { { "id", String.Empty } });
            store.DefineTable<StringIdType>();

            IMobileServiceClient client = await CreateClient(hijack, store);

            // insert item in pull table
            IMobileServiceSyncTable unrelatedTable = client.GetSyncTable("relatedTable");
            await unrelatedTable.InsertAsync(new JObject() { { "id", "abc" } });

            // then insert item in other table
            MobileServiceSyncTable<StringIdType> mainTable = client.GetSyncTable<StringIdType>() as MobileServiceSyncTable<StringIdType>;
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await mainTable.InsertAsync(item);

            await mainTable.PullAsync(null, null, null, null, CancellationToken.None, null, "relatedTable");

            Assert.Equal(4, hijack.Requests.Count); // 2 for push and 2 for pull
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/relatedTable");
            QueryEquals(hijack.Requests[1].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[1].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[1].RequestUri.AbsolutePath, "/tables/StringIdType");
            Assert.Equal(0L, client.SyncContext.PendingOperations);
        }

        [Fact]
        public async Task PullAsync_TriggersPush_WhenPushOtherTablesIsTrue_AndThereIsOperationTable()
        {
            ResetDatabase(TestTable);
            TestUtilities.DropTestTable(TestDbName, "relatedTable");
            TestUtilities.DropTestTable(TestDbName, "StringIdType");

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hi\"}");
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable("relatedTable", new JObject() { { "id", String.Empty } });
            store.DefineTable<StringIdType>();

            IMobileServiceClient client = await CreateClient(hijack, store);

            // insert item in pull table
            IMobileServiceSyncTable unrelatedTable = client.GetSyncTable("relatedTable");
            await unrelatedTable.InsertAsync(new JObject() { { "id", "abc" } });

            // then insert item in other table
            MobileServiceSyncTable<StringIdType> mainTable = client.GetSyncTable<StringIdType>() as MobileServiceSyncTable<StringIdType>;
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await mainTable.InsertAsync(item);

            await mainTable.PullAsync(null, null, null, true, CancellationToken.None);

            Assert.Equal(4, hijack.Requests.Count); // 2 for push and 2 for pull
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/relatedTable");
            QueryEquals(hijack.Requests[1].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[1].RequestUri.AbsolutePath, "/tables/StringIdType");
            QueryEquals(hijack.Requests[1].RequestUri.AbsolutePath, "/tables/StringIdType");
            Assert.Equal(0L, client.SyncContext.PendingOperations);
        }

        [Fact]
        public async Task PushAsync_PushesOnlySelectedTables_WhenSpecified()
        {
            ResetDatabase(TestTable);
            TestUtilities.DropTestTable(TestDbName, "someTable");
            TestUtilities.DropTestTable(TestDbName, "StringIdType");

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable("someTable", new JObject() { { "id", String.Empty } });
            store.DefineTable<StringIdType>();

            IMobileServiceClient client = await CreateClient(hijack, store);

            // insert item in pull table
            IMobileServiceSyncTable unrelatedTable = client.GetSyncTable("someTable");
            await unrelatedTable.InsertAsync(new JObject() { { "id", "abc" } });

            // then insert item in other table
            MobileServiceSyncTable<StringIdType> mainTable = client.GetSyncTable<StringIdType>() as MobileServiceSyncTable<StringIdType>;
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await mainTable.InsertAsync(item);

            await (client.SyncContext as MobileServiceSyncContext).PushAsync(CancellationToken.None, MobileServiceTableKind.Table, "someTable");

            Assert.Single(hijack.Requests);
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/someTable");
            Assert.Equal(1L, client.SyncContext.PendingOperations);
        }

        [Fact]
        public async Task PushAsync_PushesAllTables_WhenEmptyListIsGiven()
        {
            ResetDatabase(TestTable);
            TestUtilities.DropTestTable(TestDbName, "someTable");
            TestUtilities.DropTestTable(TestDbName, "StringIdType");

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable("someTable", new JObject() { { "id", String.Empty } });
            store.DefineTable<StringIdType>();

            IMobileServiceClient client = await CreateClient(hijack, store);

            // insert item in pull table
            IMobileServiceSyncTable table1 = client.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });

            // then insert item in other table
            MobileServiceSyncTable<StringIdType> table2 = client.GetSyncTable<StringIdType>() as MobileServiceSyncTable<StringIdType>;
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await table2.InsertAsync(item);

            await (client.SyncContext as MobileServiceSyncContext).PushAsync(CancellationToken.None);

            Assert.Equal(2, hijack.Requests.Count);
            QueryEquals(hijack.Requests[0].RequestUri.AbsolutePath, "/tables/someTable");
            QueryEquals(hijack.Requests[1].RequestUri.AbsolutePath, "/tables/StringIdType");
            Assert.Equal(0L, client.SyncContext.PendingOperations);
        }

        [Fact]
        public async Task SystemPropertiesArePreserved_OnlyWhenReturnedFromServer()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<ToDoWithSystemPropertiesType> table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            // first insert an item
            var updatedItem = new ToDoWithSystemPropertiesType()
            {
                Id = "b",
                String = "Hey",
                Version = "abc",
                CreatedAt = new DateTime(2013, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2013, 1, 1, 1, 1, 2, DateTimeKind.Utc)
            };
            await table.UpdateAsync(updatedItem);

            var lookedupItem = await table.LookupAsync("b");

            Assert.Equal("Hey", lookedupItem.String);
            Assert.Equal("abc", lookedupItem.Version);
            // we ignored the sys properties on the local object
            Assert.Equal(lookedupItem.CreatedAt, new DateTime(0, DateTimeKind.Utc));
            Assert.Equal(lookedupItem.UpdatedAt, new DateTime(0, DateTimeKind.Utc));

            Assert.Equal(1L, service.SyncContext.PendingOperations); // operation pending

            hijack.OnSendingRequest = async req =>
            {
                string content = await req.Content.ReadAsStringAsync();
                Assert.Equal(@"{""id"":""b"",""String"":""Hey""}", content); // the system properties are not sent to server
                return req;
            };
            string updateResult = "{\"id\":\"b\",\"String\":\"Wow\",\"version\":\"def\",\"createdAt\":\"2014-01-29T23:01:33.444Z\", \"updatedAt\":\"2014-01-30T23:01:33.444Z\"}";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(updateResult) }); // push
            await service.SyncContext.PushAsync();

            Assert.Equal(0L, service.SyncContext.PendingOperations); // operation removed

            lookedupItem = await table.LookupAsync("b");
            Assert.Equal("Wow", lookedupItem.String);
            Assert.Equal("def", lookedupItem.Version);
            // we preserved the system properties returned from server on update
            Assert.Equal(lookedupItem.CreatedAt.ToUniversalTime(), new DateTime(2014, 01, 29, 23, 1, 33, 444, DateTimeKind.Utc));
            Assert.Equal(lookedupItem.UpdatedAt.ToUniversalTime(), new DateTime(2014, 01, 30, 23, 1, 33, 444, DateTimeKind.Utc));
        }

        [Fact]
        public async Task TruncateAsync_DeletesAllTheRows()
        {
            string tableName = "stringId_test_table";

            ResetDatabase(tableName);

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"{""id"": ""123"", ""version"": ""xyz""}");
            hijack.AddResponseContent(@"{""id"": ""134"", ""version"": ""ghi""}");

            IMobileServiceClient service = await CreateClient(hijack, store);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            var items = new ToDoWithSystemPropertiesType[]
            {
                new ToDoWithSystemPropertiesType()
                {
                    Id = "123",
                    Version = "abc",
                    String = "def"
                },
                new ToDoWithSystemPropertiesType()
                {
                    Id = "134",
                    Version = "ghi",
                    String = "jkl"
                }
            };

            foreach (var inserted in items)
            {
                await table.InsertAsync(inserted);
            }

            var result = await table.IncludeTotalCount().Take(0).ToCollectionAsync();
            Assert.Equal(2L, result.TotalCount);

            await service.SyncContext.PushAsync();
            await table.PurgeAsync();

            result = await table.IncludeTotalCount().Take(0).ToCollectionAsync();
            Assert.Equal(0L, result.TotalCount);
        }

        [Fact]
        public async Task PushAsync_RetriesOperation_WhenConflictOccursInLastPush()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            string conflictResult = "{\"id\":\"b\",\"String\":\"Hey\",\"version\":\"def\"}";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.PreconditionFailed) { Content = new StringContent(conflictResult) }); // first push
            string successResult = "{\"id\":\"b\",\"String\":\"Wow\",\"version\":\"def\"}";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(successResult) }); // second push

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<ToDoWithSystemPropertiesType> table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            // first insert an item
            var updatedItem = new ToDoWithSystemPropertiesType() { Id = "b", String = "Hey", Version = "abc" };
            await table.UpdateAsync(updatedItem);

            // then push it to server
            var ex = await Assert.ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);

            Assert.NotNull(ex.PushResult);
            Assert.Equal(MobileServicePushStatus.Complete, ex.PushResult.Status);
            Assert.Single(ex.PushResult.Errors);
            MobileServiceTableOperationError error = ex.PushResult.Errors.FirstOrDefault();
            Assert.NotNull(error);
            Assert.False(error.Handled);
            Assert.Equal(MobileServiceTableOperationKind.Update, error.OperationKind);
            Assert.Equal(error.RawResult, conflictResult);
            Assert.Equal(error.TableName, TestTable);
            Assert.Equal(HttpStatusCode.PreconditionFailed, error.Status);

            var errorItem = error.Item.ToObject<ToDoWithSystemPropertiesType>(JsonSerializer.Create(service.SerializerSettings));
            Assert.Equal(errorItem.Id, updatedItem.Id);
            Assert.Equal(errorItem.String, updatedItem.String);
            Assert.Equal(errorItem.Version, updatedItem.Version);
            Assert.Equal(errorItem.CreatedAt, updatedItem.CreatedAt);
            Assert.Equal(errorItem.UpdatedAt, updatedItem.UpdatedAt);

            Assert.Equal(error.Result.ToString(Formatting.None), conflictResult);

            Assert.Equal(1L, service.SyncContext.PendingOperations); // operation not removed
            updatedItem = await table.LookupAsync("b");
            Assert.Equal("Hey", updatedItem.String); // item is not updated 

            await service.SyncContext.PushAsync();

            Assert.Equal(0L, service.SyncContext.PendingOperations); // operation now succeeds

            updatedItem = await table.LookupAsync("b");
            Assert.Equal("Wow", updatedItem.String); // item is updated
        }

        [Fact]
        public async Task PushAsync_DiscardsOperationAndUpdatesTheItem_WhenCancelAndUpdateItemAsync()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            string conflictResult = "{\"id\":\"b\",\"String\":\"Wow\",\"version\":\"def\"}";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.PreconditionFailed) { Content = new StringContent(conflictResult) }); // first push

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<ToDoWithSystemPropertiesType> table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            // first insert an item
            var updatedItem = new ToDoWithSystemPropertiesType() { Id = "b", String = "Hey", Version = "abc" };
            await table.UpdateAsync(updatedItem);

            // then push it to server
            var ex = await Assert.ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);

            Assert.NotNull(ex.PushResult);
            MobileServiceTableOperationError error = ex.PushResult.Errors.FirstOrDefault();
            Assert.NotNull(error);

            Assert.Equal(1L, service.SyncContext.PendingOperations); // operation is not removed
            updatedItem = await table.LookupAsync("b");
            Assert.Equal("Hey", updatedItem.String); // item is not updated 

            await error.CancelAndUpdateItemAsync(error.Result);

            Assert.Equal(0L, service.SyncContext.PendingOperations); // operation is removed
            updatedItem = await table.LookupAsync("b");
            Assert.Equal("Wow", updatedItem.String); // item is updated             
        }

        [Fact]
        public async Task PushAsync_DiscardsOperationAndDeletesTheItem_WhenCancelAndDiscardItemAsync()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            string conflictResult = "{\"id\":\"b\",\"String\":\"Wow\",\"version\":\"def\"}";
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.PreconditionFailed) { Content = new StringContent(conflictResult) }); // first push

            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithSystemPropertiesType>();

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<ToDoWithSystemPropertiesType> table = service.GetSyncTable<ToDoWithSystemPropertiesType>();

            // first insert an item
            var updatedItem = new ToDoWithSystemPropertiesType() { Id = "b", String = "Hey", Version = "abc" };
            await table.UpdateAsync(updatedItem);

            // then push it to server
            var ex = await Assert.ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);

            Assert.NotNull(ex.PushResult);
            MobileServiceTableOperationError error = ex.PushResult.Errors.FirstOrDefault();
            Assert.NotNull(error);

            Assert.Equal(1L, service.SyncContext.PendingOperations); // operation is not removed
            updatedItem = await table.LookupAsync("b");
            Assert.Equal("Hey", updatedItem.String); // item is not updated 

            await error.CancelAndDiscardItemAsync();

            Assert.Equal(0L, service.SyncContext.PendingOperations); // operation is removed
            updatedItem = await table.LookupAsync("b");
            Assert.Null(updatedItem); // item is deleted
        }

        [Fact]
        public async Task Insert_AllTypes_ThenRead_ThenPush_ThenLookup()
        {
            ResetDatabase("AllBaseTypesWithAllSystemPropertiesType");

            var hijack = new TestHttpHandler();
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<AllBaseTypesWithAllSystemPropertiesType>();

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<AllBaseTypesWithAllSystemPropertiesType> table = service.GetSyncTable<AllBaseTypesWithAllSystemPropertiesType>();

            // first insert an item
            var inserted = new AllBaseTypesWithAllSystemPropertiesType()
            {
                Id = "abc",
                Bool = true,
                Byte = 11,
                SByte = -11,
                UShort = 22,
                Short = -22,
                UInt = 33,
                Int = -33,
                ULong = 44,
                Long = -44,
                Float = 55.66f,
                Double = 66.77,
                Decimal = 77.88M,
                String = "EightyEight",
                Char = '9',
                DateTime = new DateTime(2010, 10, 10, 10, 10, 10, DateTimeKind.Utc),
                DateTimeOffset = new DateTimeOffset(2011, 11, 11, 11, 11, 11, 11, TimeSpan.Zero),
                Nullable = 12.13,
                NullableDateTime = new DateTime(2010, 10, 10, 10, 10, 10, DateTimeKind.Utc),
                TimeSpan = new TimeSpan(0, 12, 12, 15, 95),
                Uri = new Uri("http://example.com"),
                Enum1 = Enum1.Enum1Value2,
                Enum2 = Enum2.Enum2Value2,
                Enum3 = Enum3.Enum3Value2,
                Enum4 = Enum4.Enum4Value2,
                Enum5 = Enum5.Enum5Value2,
                Enum6 = Enum6.Enum6Value2
            };

            await table.InsertAsync(inserted);

            IList<AllBaseTypesWithAllSystemPropertiesType> records = await table.ToListAsync();
            Assert.Equal(1, records.Count);

            Assert.Equal(records.First(), inserted);

            // now push
            hijack.AddResponseContent(@"
{""id"":""abc"",
""bool"":true,
""byte"":11,
""sByte"":-11,
""uShort"":22,
""short"":-22,
""uInt"":33,
""int"":-33,
""uLong"":44,
""long"":-44,
""float"":55.66,
""double"":66.77,
""decimal"":77.88,
""string"":""EightyEight"",
""char"":""9"",
""dateTime"":""2010-10-10T10:10:10.000Z"",
""dateTimeOffset"":""2011-11-11T11:11:11.011Z"",
""nullableDateTime"":""2010-10-10T10:10:10.000Z"",
""timeSpan"":""12:12:15.095"",
""nullable"":12.13,
""uri"":""http://example.com/"",
""enum1"":""Enum1Value2"",
""enum2"":""Enum2Value2"",
""enum3"":""Enum3Value2"",
""enum4"":""Enum4Value2"",
""enum5"":""Enum5Value2"",
""enum6"":""Enum6Value2"",
""version"":""XYZ""}");
            await service.SyncContext.PushAsync();
            AllBaseTypesWithAllSystemPropertiesType lookedUp = await table.LookupAsync("abc");
            inserted.Version = "XYZ";
            Assert.Equal(inserted, lookedUp);
        }

        [Fact]
        public async Task Insert_ThenPush_ThenPull_ThenRead_ThenUpdate_ThenRefresh_ThenDelete_ThenLookup_ThenPush_ThenPurge_ThenRead()
        {
            ResetDatabase(TestTable);

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"b\",\"String\":\"Hey\"}"); // insert response
            hijack.AddResponseContent("[{\"id\":\"b\",\"String\":\"Hey\"},{\"id\":\"a\",\"String\":\"World\"}]"); // pull response            
            hijack.AddResponseContent("[]"); // pull last page

            IMobileServiceClient service = await CreateTodoClient(hijack);
            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            // first insert an item
            await table.InsertAsync(new ToDoWithStringId() { Id = "b", String = "Hey" });

            // then push it to server
            await service.SyncContext.PushAsync();

            // then pull changes from server
            await table.PullAsync(null, null);

            // order the records by id so we can assert them predictably 
            IList<ToDoWithStringId> items = await table.OrderBy(i => i.Id).ToListAsync();

            // we should have 2 records 
            Assert.Equal(2, items.Count);

            // according to ordering a id comes first
            Assert.Equal("a", items[0].Id);
            Assert.Equal("World", items[0].String);

            // then comes b record
            Assert.Equal("b", items[1].Id);
            Assert.Equal("Hey", items[1].String);

            // we made 2 requests, one for push and two for pull
            Assert.Equal(3, hijack.Requests.Count);

            // recreating the client from state in the store
            service = await CreateTodoClient(hijack);
            table = service.GetSyncTable<ToDoWithStringId>();

            // update the second record
            items[1].String = "Hello";
            await table.UpdateAsync(items[1]);

            // create an empty record with same id as modified record
            var second = new ToDoWithStringId() { Id = items[1].Id };
            // refresh the empty record
            await table.RefreshAsync(second);

            // make sure it is same as modified record now
            Assert.Equal(second.String, items[1].String);

            // now delete the record
            await table.DeleteAsync(second);

            // now try to get the deleted record
            ToDoWithStringId deleted = await table.LookupAsync(second.Id);

            // this should be null
            Assert.Null(deleted);

            // try to get the non-deleted record
            ToDoWithStringId first = await table.LookupAsync(items[0].Id);

            // this should still be there;
            Assert.NotNull(first);

            // make sure it is same as 
            Assert.Equal(first.String, items[0].String);

            // recreating the client from state in the store
            service = await CreateTodoClient(hijack);
            table = service.GetSyncTable<ToDoWithStringId>();

            await service.SyncContext.PushAsync();
            // now purge the remaining records
            await table.PurgeAsync();

            // now read one last time
            IEnumerable<ToDoWithStringId> remaining = await table.ReadAsync();

            // There shouldn't be anything remaining
            Assert.Empty(remaining);
        }

        private static async Task<IMobileServiceSyncTable<T>> GetSynctable<T>(TestHttpHandler hijack)
        {
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<T>();

            IMobileServiceClient service = await CreateClient(hijack, store);
            IMobileServiceSyncTable<T> table = service.GetSyncTable<T>();
            return table;
        }

        private static async Task<IMobileServiceClient> CreateTodoClient(TestHttpHandler hijack)
        {
            var store = new MobileServiceSQLiteStore(TestDbName);
            store.DefineTable<ToDoWithStringId>();
            return await CreateClient(hijack, store);
        }

        private static async Task<IMobileServiceClient> CreateClient(TestHttpHandler hijack, MobileServiceSQLiteStore store)
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            return service;
        }

        private static void ResetDatabase(string testTableName)
        {
            TestUtilities.DropTestTable(TestDbName, testTableName);
            TestUtilities.ResetDatabase(TestDbName);
        }

        private void QueryEquals(string a, string b) => Assert.Equal(Uri.UnescapeDataString(a), Uri.UnescapeDataString(b));
    }
}
