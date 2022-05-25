// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MobileClient.Tests.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests.Table
{
    public class MobileServiceSyncTable_Generic_Test
    {
        [Fact]
        public async Task RefreshAsync_Succeeds_WhenIdIsNull()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { String = "what?" };
            await table.RefreshAsync(item);
        }

        [Fact]
        public async Task RefreshAsync_ThrowsInvalidOperationException_WhenIdItemDoesNotExist()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "abc" };
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.RefreshAsync(item));
        }

        [Fact]
        public async Task RefreshAsync_UpdatesItem_WhenItExistsInStore()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            // add item to store
            var item = new StringIdType() { String = "what?" };
            await table.InsertAsync(item);

            Assert.NotNull(item.Id); // Id must be generated

            // update it in store
            item.String = "nothing!";
            await table.UpdateAsync(item);

            // read it back into new object
            var refreshed = new StringIdType() { Id = item.Id };
            await table.RefreshAsync(refreshed);

            Assert.Equal("nothing!", refreshed.String);
        }

        [Fact]
        public async Task PullAsync_Cancels_WhenCancellationTokenIsCancelled()
        {
            var store = new MobileServiceLocalStoreMock();

            var handler = new MobileServiceSyncHandlerMock();
            handler.TableOperationAction = op => Task.Delay(TimeSpan.MaxValue).ContinueWith<JObject>(t => null); // long slow operation
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = async req =>
            {
                await Task.Delay(1000);
                return req;
            };
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            using (var cts = new CancellationTokenSource())
            {
                Task pullTask = table.PullAsync(null, null, null, cancellationToken: cts.Token);
                cts.Cancel();

                var ex = await Assert.ThrowsAnyAsync<Exception>(() => pullTask);

                Assert.True((ex is OperationCanceledException || ex is TaskCanceledException));
                Assert.Equal(TaskStatus.Canceled, pullTask.Status);
            }
        }

        [Fact]
        public async Task PullAsync_DoesNotPurge_WhenItemIsMissing()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"def\",\"String\":\"World\"}]"); // remote item
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } }); // insert an item
            await service.SyncContext.PushAsync(); // push to clear the queue

            // now pull
            await table.PullAsync(null, null);

            Assert.Equal(2, store.TableMap[table.TableName].Count); // 1 from remote and 1 from local
            Assert.Equal(3, hijack.Requests.Count); // one for push and 2 for pull
        }

        [Fact]
        public async Task PullAsync_DoesNotTriggerPush_WhenThereIsNoOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert item in pull table
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });

            // but push to clear the queue
            await service.SyncContext.PushAsync();
            Assert.Single(store.TableMap[table1.TableName]); // item is inserted
            Assert.Single(hijack.Requests); // first push

            // then insert item in other table
            IMobileServiceSyncTable<StringIdType> table2 = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table2.InsertAsync(item);

            await table1.PullAsync(null, null);

            Assert.Equal(2, store.TableMap[table1.TableName].Count); // table should contain 2 pulled items
            Assert.Equal(3, hijack.Requests.Count); // 1 for push and 2 for pull
            Assert.Single(store.TableMap[table2.TableName]); // this table should not be touched
        }

        [Fact]
        public async Task PullAsync_TriggersPush_WhenThereIsOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.Single(store.TableMap[table1.TableName]); // item is inserted

            // this should trigger a push
            await table1.PullAsync(null, null);

            Assert.Equal(3, hijack.Requests.Count); // 1 for push and 2 for pull
            Assert.Equal(2, store.TableMap[table1.TableName].Count); // table is populated
        }

        [Fact]
        public async Task PullAsync_TriggersPush_FeatureHeaderInOperation()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });

            // this should trigger a push
            await table.PullAsync(null, null);

            Assert.Equal("TU,OL", hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First());
            Assert.Equal("QS,OL", hijack.Requests[1].Headers.GetValues("X-ZUMO-FEATURES").First());
            Assert.Equal("QS,OL", hijack.Requests[2].Headers.GetValues("X-ZUMO-FEATURES").First());
        }

        [Fact]
        public async Task PullAsync_FollowsNextLinks()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, null);

            Assert.Equal(4, store.TableMap["stringId_test_table"].Count);
            Assert.Equal("Hey", store.TableMap["stringId_test_table"]["abc"].Value<string>("String"));
            Assert.Equal("How", store.TableMap["stringId_test_table"]["def"].Value<string>("String"));
            Assert.Equal("Are", store.TableMap["stringId_test_table"]["ghi"].Value<string>("String"));
            Assert.Equal("You", store.TableMap["stringId_test_table"]["jkl"].Value<string>("String"));

            MatchUris(hijack.Requests, 
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=0&$top=50&__includeDeleted=true"),
                "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2",
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=4&$top=50&__includeDeleted=true"));

        }

        [Fact]
        public async Task PullAsync_UpdatesDeltaToken_AfterEachResult_IfOrderByIsSupported()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                         {""id"":""def"",""String"":""How"", ""updatedAt"": ""2001-02-04T00:00:00.0000000+00:00""}]"); // first page
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.InternalServerError)); // throw on second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => table.PullAsync("items", table.CreateQuery()));

            Assert.Equal(2, store.TableMap["stringId_test_table"].Count);
            Assert.Equal("Hey", store.TableMap["stringId_test_table"]["abc"].Value<string>("String"));
            Assert.Equal("How", store.TableMap["stringId_test_table"]["def"].Value<string>("String"));

            MatchUris(hijack.Requests, 
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'2001-02-04T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"));

            Assert.Equal("2001-02-04T00:00:00.0000000+00:00", store.TableMap[MobileServiceLocalSystemTables.Config]["deltaToken|stringId_test_table|items"]["value"]);
        }

        [Fact]
        public async Task PullAsync_DoesNotUpdateDeltaToken_AfterEachResult_IfOrderByIsNotSupported()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                         {""id"":""def"",""String"":""How"", ""updatedAt"": ""2001-02-04T00:00:00.0000000+00:00""}]"); // first page
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.InternalServerError)); // throw on second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.OrderBy;

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => table.PullAsync("items", table.CreateQuery()));

            Assert.Equal(2, store.TableMap["stringId_test_table"].Count);
            Assert.Equal("Hey", store.TableMap["stringId_test_table"]["abc"].Value<string>("String"));
            Assert.Equal("How", store.TableMap["stringId_test_table"]["def"].Value<string>("String"));

            MatchUris(hijack.Requests, 
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$skip=2&$top=50&__includeDeleted=true"));

            Assert.False(store.TableMap[MobileServiceLocalSystemTables.Config].ContainsKey("deltaToken|stringId_test_table|items"));
        }

        [Fact]
        public async Task PullAsync_UsesSkipAndTakeThenFollowsLinkThenUsesSkipAndTake()
        {
            var hijack = new TestHttpHandler();
            // first page
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]");
            // second page with a link
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]");
            hijack.Responses[1].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            // forth page without link
            hijack.AddResponseContent("[{\"id\":\"mno\",\"String\":\"Mr\"},{\"id\":\"pqr\",\"String\":\"X\"}]");
            // last page
            hijack.AddResponseContent("[]");

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(51).Skip(3));

            Assert.Equal(6, store.TableMap["stringId_test_table"].Count);
            Assert.Equal("Hey", store.TableMap["stringId_test_table"]["abc"].Value<string>("String"));
            Assert.Equal("How", store.TableMap["stringId_test_table"]["def"].Value<string>("String"));
            Assert.Equal("Are", store.TableMap["stringId_test_table"]["ghi"].Value<string>("String"));
            Assert.Equal("You", store.TableMap["stringId_test_table"]["jkl"].Value<string>("String"));
            Assert.Equal("Mr", store.TableMap["stringId_test_table"]["mno"].Value<string>("String"));
            Assert.Equal("X", store.TableMap["stringId_test_table"]["pqr"].Value<string>("String"));

            MatchUris(hijack.Requests, 
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=3&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=5&$top=49&__includeDeleted=true"),
                "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2",
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=9&$top=45&__includeDeleted=true"));
        }

        [Theory]
        [InlineData("http://test.com/", "http://test.com/api/todoitem", "http://test.com/api/todoitem")]
        [InlineData("http://test.com", "/api/todoitem", "http://test.com/api/todoitem")]
        [InlineData("http://test.com/folder/", "http://test.com/folder/api/todoitem", "http://test.com/folder/api/todoitem")]
        [InlineData("http://test.com/folder", "/api/todoitem", "http://test.com/folder/api/todoitem")]
        public async Task PullAsync_Supports_AbsoluteAndRelativeUri(string serviceUri, string queryUri, string result)
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.AddResponseContent("[]");

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(serviceUri, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, queryUri);

            Assert.Equal(2, store.TableMap["stringId_test_table"].Count);

            MatchUris(hijack.Requests, result + "?$skip=0&$top=50&__includeDeleted=true",
                                                result + "?$skip=2&$top=50&__includeDeleted=true");

            Assert.Equal("QS,OL", hijack.Requests[0].Headers.GetValues("X-ZUMO-FEATURES").First());
            Assert.Equal("QS,OL", hijack.Requests[1].Headers.GetValues("X-ZUMO-FEATURES").First());
        }

        [Fact]
        public async Task PullASync_Throws_IfAbsoluteUriHostNameDoesNotMatch()
        {
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, new TestHttpHandler());
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDo> table = service.GetSyncTable<ToDo>();

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync(null, "http://www.contoso.com/about?$filter=a eq b&$orderby=c"));

            Assert.Equal("The query uri must be on the same host as the Mobile Service.", ex.Message);
        }

        [Fact]
        public async Task PullAsync_DoesNotFollowLink_IfRelationIsNotNext()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://contoso.com:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=prev");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, null);

            Assert.Equal(4, store.TableMap["stringId_test_table"].Count);

            MatchUris(hijack.Requests, 
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=2&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=4&$top=50&__includeDeleted=true"));
        }

        [Fact]
        public async Task PullAsync_DoesNotFollowLink_IfLinkHasNonSupportedOptions()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://contoso.com:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.Skip;

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, null);

            Assert.Equal(2, store.TableMap["stringId_test_table"].Count);

            MatchUris(hijack.Requests, mobileAppUriValidator.GetTableUri("stringId_test_table?$top=50&__includeDeleted=true"));
        }

        [Fact]
        public async Task PullAsync_UsesTopInQuery_IfLessThanMaxPageSize()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            var pullOptions = new PullOptions
            {
                MaxPageSize = 50,
            };

            await table.PullAsync(null, table.Take(49), pullOptions);

            Assert.Equal(2, store.TableMap["stringId_test_table"].Count);

            MatchUris(hijack.Requests,
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=0&$top=49&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=2&$top=47&__includeDeleted=true"));
        }

        [Fact]
        public async Task PullAsync_DefaultsTo50_IfGreaterThanMaxPageSize()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.AddResponseContent("[]"); // end of the list

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            var pullOptions = new PullOptions
            {
                MaxPageSize = 50,
            };
            await table.PullAsync(null, table.Take(51), pullOptions);

            Assert.Equal(2, store.TableMap["stringId_test_table"].Count);

            MatchUris(hijack.Requests, 
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$skip=2&$top=49&__includeDeleted=true"));
        }

        [Fact]
        public async Task PullAsync_DoesNotFollowLink_IfMaxRecordsAreRetrieved()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"How\"}]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(1));

            Assert.Single(store.TableMap["stringId_test_table"]);
        }

        [Fact]
        public async Task PullAsync_DoesNotFollowLink_IfResultIsEmpty()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("[]"); // first page
            hijack.Responses[0].Headers.Add("Link", "http://localhost:31475/tables/Green?$top=1&$select=Text%2CDone%2CId&$skip=2; rel=next");
            hijack.AddResponseContent("[{\"id\":\"ghi\",\"String\":\"Are\"},{\"id\":\"jkl\",\"String\":\"You\"}]"); // second page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            await table.PullAsync(null, table.Take(1));

            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));
        }

        [Fact]
        public async Task PullAsync_Throws_WhenPushThrows()
        {
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.NotFound)); // for push

            var store = new MobileServiceLocalStoreMock();

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.Single(store.TableMap[table.TableName]); // item is inserted

            // this should trigger a push
            var ex = await Assert.ThrowsAsync<MobileServicePushFailedException>(() => table.PullAsync(null, null));

            Assert.Single(ex.PushResult.Errors);
            Assert.Single(hijack.Requests); // 1 for push
        }

        [Fact]
        public async Task PullAsync_Throws_WhenSelectClauseIsSpecified()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.Select(x => x.String);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query, cancellationToken: CancellationToken.None));
            Assert.Equal("query", exception.ParamName);
            Assert.StartsWith("Pull query with select clause is not supported.", exception.Message);
        }

        [Fact]
        public async Task PullAsync_Throws_WhenOrderByClauseIsSpecifiedWithQueryId()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.OrderBy(x => x.String);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync("incQuery", query, cancellationToken: CancellationToken.None));
            Assert.Equal("query", exception.ParamName);
            Assert.StartsWith("Incremental pull query must not have orderby clause.", exception.Message);
        }

        [Fact]
        public async Task PullAsync_Throws_WhenOrderByClauseIsSpecifiedAndOptionIsNotSet()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.OrderBy;
            var query = table.OrderBy(x => x.String);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query));
            Assert.Equal("query", exception.ParamName);
            Assert.StartsWith("The supported table options does not include orderby.", exception.Message);
        }

        [Fact]
        public async Task PullAsync_Throws_WhenTopClauseIsSpecifiedAndOptionIsNotSet()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.Top;
            var query = table.Take(30);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query));
            Assert.Equal("query", exception.ParamName);
            Assert.StartsWith("The supported table options does not include top.", exception.Message);
        }

        [Fact]
        public async Task PullAsync_Throws_WhenSkipClauseIsSpecifiedAndOptionIsNotSet()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions &= ~MobileServiceRemoteTableOptions.Skip;
            var query = table.Skip(30);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query));
            Assert.Equal("query", exception.ParamName);
            Assert.StartsWith("The supported table options does not include skip.", exception.Message);
        }

        [Fact]
        public async Task PullAsync_Throws_WhenTopOrSkipIsSpecifiedWithQueryId()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            string expectedError = "Incremental pull query must not have skip or top specified.";

            var query = table.Take(5);
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync("incQuery", query, cancellationToken: CancellationToken.None));
            Assert.Equal("query", exception.ParamName);
            Assert.StartsWith(expectedError, exception.Message);

            query = table.Skip(5);
            exception = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync("incQuery", query, cancellationToken: CancellationToken.None));
            Assert.Equal("query", exception.ParamName);
            Assert.StartsWith(expectedError, exception.Message);
        }

        [Fact]
        public async Task PullAsync_Succeeds()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                return Task.FromResult(req);
            };
            hijack.AddResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"},{\"id\":\"def\",\"String\":\"World\"}]"); // for pull
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.Skip(5)
                             .Take(3)
                             .Where(t => t.String == "world")
                             .OrderBy(o => o.Id)
                             .WithParameters(new Dictionary<string, string>() { { "param1", "val1" } })
                             .OrderByDescending(o => o.String)
                             .IncludeTotalCount();

            await table.PullAsync(null, query, cancellationToken: CancellationToken.None);
            Assert.Equal(2, hijack.Requests.Count);
            Assert.Equal(Uri.UnescapeDataString("?$filter=(String%20eq%20'world')&$orderby=String%20desc,id&$skip=5&$top=3&param1=val1&__includeDeleted=true"), Uri.UnescapeDataString(hijack.Requests[0].RequestUri.Query));
        }

        [Fact]
        public async Task PullAsync_Incremental_AllOptions_MovesByUpdatedAt()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.All,
                    "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$orderby=updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true",
                    "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00'))&$orderby=updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_Incremental_WithNullUpdatedAt_Succeeds()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                return Task.FromResult(req);
            };
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""updatedAt"": null}]");
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await table.PullAsync("items", table.CreateQuery());
            MatchUris(hijack.Requests,
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=1&$top=50&__includeDeleted=true"));
        }

        [Fact]
        public async Task PullAsync_Incremental_MovesByUpdatedAt_ThenUsesSkipAndTop_WhenUpdatedAtDoesNotChange()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                return Task.FromResult(req);
            };
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""}]");
            hijack.AddResponseContent(@"[{""id"":""def"",""String"":""World"", ""updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""}]");
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await table.PullAsync("items", table.CreateQuery());
            MatchUris(hijack.Requests,
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=1&$top=50&__includeDeleted=true"));
        }

        [Fact]
        public async Task PullAsync_Incremental_WithoutOrderBy_MovesBySkipAndTop()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.Skip | MobileServiceRemoteTableOptions.Top,
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$skip=0&$top=50&param1=val1&__includeDeleted=true",
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$skip=2&$top=50&param1=val1&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_Incremental_WithoutSkipAndOrderBy_CanNotMoveForward()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.Top,
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$top=50&param1=val1&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_Incremental_WithoutTopAndOrderBy_MovesBySkip()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.Skip,
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&param1=val1&__includeDeleted=true",
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$skip=2&param1=val1&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_Incremental_WithoutSkipAndTop_MovesByUpdatedAt()
        {
            await TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions.OrderBy,
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-01T00:00:00.0000000%2B00:00'))&$orderby=updatedAt&param1=val1&__includeDeleted=true",
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00'))&$orderby=updatedAt&param1=val1&__includeDeleted=true");
        }

        [Fact]
        public async Task PullAsync_Incremental_PageSize()
        {
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                return Task.FromResult(req);
            };
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey""}]");
            hijack.AddResponseContent("[]"); // last page

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com", hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            PullOptions pullOptions = new PullOptions
            {
                MaxPageSize = 10
            };

            await table.PullAsync("items", table.CreateQuery(), pullOptions: pullOptions);
            MatchUris(hijack.Requests,
                string.Format("http://www.test.com/tables/stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top={0}&__includeDeleted=true", pullOptions.MaxPageSize),
                string.Format("http://www.test.com/tables/stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=1&$top={0}&__includeDeleted=true", pullOptions.MaxPageSize));
        }

        private static async Task TestPullAsyncIncrementalWithOptions(MobileServiceRemoteTableOptions options, params string[] uris)
        {
            var store = new MobileServiceLocalStoreMock();
            var settings = new MobileServiceSyncSettingsManager(store);
            await settings.SetDeltaTokenAsync("stringId_test_table", "incquery", new DateTime(2001, 02, 01, 0, 0, 0, DateTimeKind.Utc));
            await TestIncrementalPull(store, options, uris);
        }

        [Fact]
        public async Task PullAsync_Incremental_WithoutDeltaTokenInDb()
        {
            var store = new MobileServiceLocalStoreMock();
            store.TableMap[MobileServiceLocalSystemTables.Config] = new Dictionary<string, JObject>();
            await TestIncrementalPull(store, MobileServiceRemoteTableOptions.All,
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00'))&$orderby=updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true",
                "stringId_test_table?$filter=((String eq 'world') and (updatedAt ge datetimeoffset'2001-02-03T00:00:00.0000000%2B00:00'))&$orderby=updatedAt&$skip=0&$top=50&param1=val1&__includeDeleted=true");
        }

        private static async Task TestIncrementalPull(MobileServiceLocalStoreMock store, MobileServiceRemoteTableOptions options, params string[] expectedTableUriComponents)
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                        {""id"":""def"",""String"":""World"", ""updatedAt"": ""2001-02-03T00:03:00.0000000+07:00""}]"); // for pull
            hijack.AddResponseContent(@"[]");

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            table.SupportedOptions = options;
            var query = table.Where(t => t.String == "world")
                             .WithParameters(new Dictionary<string, string>() { { "param1", "val1" } })
                             .IncludeTotalCount();

            await table.PullAsync("incquery", query, cancellationToken: CancellationToken.None);

            var expectedTableUris = expectedTableUriComponents.Select(expectedTableUriComponent => mobileAppUriValidator.GetTableUri(expectedTableUriComponent)).ToArray();
            MatchUris(hijack.Requests, expectedTableUris);
        }


        [Fact]
        public async Task PullAsync_Throws_IfIncludeDeletedProvidedInParameters()
        {
            await this.TestPullQueryOverrideThrows(new Dictionary<string, string>()
                             {
                                { "__includeDeleted", "false" },
                                { "param1", "val1" }
                             },
                             "The key '__includeDeleted' is reserved and cannot be specified as a query parameter.");
        }

        [Fact]
        public async Task PullAsync_Throws_WhenQueryIdIsInvalid()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, new TestHttpHandler());
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync("2as|df", table.CreateQuery(), CancellationToken.None));
        }

        private async Task TestPullQueryOverrideThrows(IDictionary<string, string> parameters, string errorMessage)
        {
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();
            var query = table.CreateQuery()
                             .WithParameters(parameters);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => table.PullAsync(null, query, cancellationToken: CancellationToken.None));
            Assert.Equal(errorMessage, ex.Message);
        }

        [Fact]
        public async Task PurgeAsync_DoesNotTriggerPush_WhenThereIsNoOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert item in purge table
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });

            // but push to clear the queue
            await service.SyncContext.PushAsync();
            Assert.Single(store.TableMap[table1.TableName]); // item is inserted
            Assert.Single(hijack.Requests); // first push

            // then insert item in other table
            IMobileServiceSyncTable<StringIdType> table2 = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table2.InsertAsync(item);

            // try purge on first table now
            await table1.PurgeAsync();

            Assert.Equal(store.DeleteQueries[0].TableName, MobileServiceLocalSystemTables.SyncErrors); // push deletes all sync erros
            Assert.Equal(store.DeleteQueries[1].TableName, table1.TableName); // purged table
            Assert.Single(hijack.Requests); // still 1 means no other push happened
            Assert.Single(store.TableMap[table2.TableName]); // this table should not be touched
        }

        [Fact]
        public async Task PurgeAsync_ResetsDeltaToken_WhenQueryIdIsSpecified()
        {
            var hijack = new TestHttpHandler();
            hijack.AddResponseContent(@"[{""id"":""abc"",""String"":""Hey"", ""updatedAt"": ""2001-02-03T00:00:00.0000000+00:00""},
                                         {""id"":""def"",""String"":""How"", ""updatedAt"": ""2001-02-04T00:00:00.0000000+00:00""}]"); // first page
            hijack.AddResponseContent("[]"); // last page of first pull
            hijack.AddResponseContent("[]"); // second pull

            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            // ensure there is no delta token present already
            Assert.False(store.TableMap.ContainsKey("stringId_test_table"));

            // now pull down data
            await table.PullAsync("items", table.CreateQuery());

            // ensure items were pulled down
            Assert.Equal(2, store.TableMap["stringId_test_table"].Count);
            Assert.Equal("Hey", store.TableMap["stringId_test_table"]["abc"].Value<string>("String"));
            Assert.Equal("How", store.TableMap["stringId_test_table"]["def"].Value<string>("String"));

            // ensure delta token was updated
            Assert.Equal("2001-02-04T00:00:00.0000000+00:00", store.TableMap[MobileServiceLocalSystemTables.Config]["deltaToken|stringId_test_table|items"]["value"]);

            // now purge and forget the delta token
            await table.PurgeAsync("items", null, false, CancellationToken.None);

            // make sure data is purged
            Assert.Empty(store.TableMap["stringId_test_table"]);
            // make sure delta token is removed
            Assert.False(store.TableMap[MobileServiceLocalSystemTables.Config].ContainsKey("deltaToken|stringId_test_table|items"));

            // pull again
            await table.PullAsync("items", table.CreateQuery());

            // verify request urls
            MatchUris(hijack.Requests, 
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'2001-02-04T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"),
                mobileAppUriValidator.GetTableUri("stringId_test_table?$filter=(updatedAt ge datetimeoffset'1970-01-01T00:00:00.0000000%2B00:00')&$orderby=updatedAt&$skip=0&$top=50&__includeDeleted=true"));
        }

        [Fact]
        public async Task PurgeAsync_Throws_WhenQueryIdIsInvalid()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, new TestHttpHandler());
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            IMobileServiceSyncTable<ToDoWithStringId> table = service.GetSyncTable<ToDoWithStringId>();

            await Assert.ThrowsAsync<ArgumentException>(() => table.PurgeAsync("2as|df", table.CreateQuery(), CancellationToken.None));
        }

        [Fact]
        public async Task PurgeAsync_Throws_WhenThereIsOperationInTable()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.Single(store.TableMap[table1.TableName]); // item is inserted

            // this should trigger a push
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => table1.PurgeAsync());

            Assert.Equal("The table cannot be purged because it has pending operations.", ex.Message);
            Assert.Equal(1L, service.SyncContext.PendingOperations); // operation still in queue
        }

        [Fact]
        public async Task PurgeAsync_Throws_WhenThereIsOperationInTable_AndForceIsTrue_AndQueryIsSpecified()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.Single(store.TableMap[table.TableName]); // item is inserted

            // this should trigger a push
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => table.PurgeAsync(null, "$filter=a eq b", true, CancellationToken.None));

            Assert.Equal("The table cannot be purged because it has pending operations.", ex.Message);
            Assert.Equal(1L, service.SyncContext.PendingOperations); // operation still in queue
        }

        [Fact]
        public async Task PurgeAsync_DeletesOperations_WhenThereIsOperationInTable_AndForceIsTrue()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            var store = new MobileServiceLocalStoreMock();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            // put a dummy delta token
            string deltaKey = "deltaToken|someTable|abc";
            store.TableMap[MobileServiceLocalSystemTables.Config] = new Dictionary<string, JObject>() { { deltaKey, new JObject() } };

            // insert an item but don't push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");
            await table.InsertAsync(new JObject() { { "id", "abc" } });
            Assert.Single(store.TableMap[table.TableName]); // item is inserted
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            await table.PurgeAsync("abc", null, force: true, cancellationToken: CancellationToken.None);

            Assert.Empty(store.TableMap[table.TableName]); // item is deleted
            Assert.Equal(0L, service.SyncContext.PendingOperations); // operation is also removed

            // deleted delta token
            Assert.False(store.TableMap[MobileServiceLocalSystemTables.Config].ContainsKey(deltaKey));
        }

        [Fact]
        public async Task PushAsync_ExecutesThePendingOperations_OnAllTables()
        {
            var hijack = new TestHttpHandler();
            var store = new MobileServiceLocalStoreMock();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.AddResponseContent("{\"id\":\"def\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await table.InsertAsync(item);

            IMobileServiceSyncTable table1 = service.GetSyncTable("someTable");
            await table1.InsertAsync(new JObject() { { "id", "def" } });

            Assert.Single(store.TableMap[table.TableName]);
            Assert.Single(store.TableMap[table1.TableName]);

            await service.SyncContext.PushAsync();
        }

        [Fact]
        public async Task PushAsync_ExecutesThePendingOperations()
        {
            var hijack = new TestHttpHandler();
            var store = new MobileServiceLocalStoreMock();
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);

            Assert.Single(store.TableMap[table.TableName]);

            await service.SyncContext.PushAsync();
        }

        [Fact]
        public async Task PushAsync_IsAborted_OnNetworkError()
        {
            await TestPushAbort(new HttpRequestException(), MobileServicePushStatus.CancelledByNetworkError);
        }

        [Fact]
        public async Task PushAsync_IsAborted_OnAuthenticationError()
        {
            var authError = new MobileServiceInvalidOperationException(String.Empty, new HttpRequestMessage(), new HttpResponseMessage(HttpStatusCode.Unauthorized));
            await TestPushAbort(authError, MobileServicePushStatus.CancelledByAuthenticationError);
        }

        [Fact]
        public async Task DeleteAsync_DoesNotUpsertResultOnStore_WhenOperationIsPushed()
        {
            var storeMock = new MobileServiceLocalStoreMock();

            var hijack = new TestHttpHandler();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for insert
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}"); // for delete
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            await service.SyncContext.InitializeAsync(storeMock, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            // first add an item
            var item = new StringIdType() { Id = "abc", String = "what?" };
            await table.InsertAsync(item);

            Assert.Single(storeMock.TableMap[table.TableName]);

            // for good measure also push it
            await service.SyncContext.PushAsync();

            await table.DeleteAsync(item);

            Assert.Empty(storeMock.TableMap[table.TableName]);

            // now play it on server
            await service.SyncContext.PushAsync();

            // wait we don't want to upsert the result back because its delete operation
            Assert.Empty(storeMock.TableMap[table.TableName]);
            // looks good
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenInsertWasAttempted()
        {
            var hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.RequestTimeout); // insert response
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);
            // insert is in queue
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            var pushException = await Assert.ThrowsAsync<MobileServicePushFailedException>(() => service.SyncContext.PushAsync());
            Assert.Single(pushException.PushResult.Errors);

            var delException = await Assert.ThrowsAsync<InvalidOperationException>(() => table.DeleteAsync(item));
            Assert.Equal("The item is in inconsistent state in the local store. Please complete the pending sync by calling PushAsync() before deleting the item.", delException.Message);

            // insert still in queue
            Assert.Equal(1L, service.SyncContext.PendingOperations);
        }

        [Fact]
        public Task DeleteAsync_Throws_WhenDeleteIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.DeleteAsync(item),
                                          secondOperation: (table, item) => table.DeleteAsync(item));
        }

        [Fact]
        public async Task DeleteAsync_CancelsAll_WhenInsertIsInQueue()
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":\"abc\",\"String\":\"Hey\"}]");
            hijack.OnSendingRequest = req =>
            {
                Assert.False(true, "No requests should be made");
                return Task.FromResult(req);
            };
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };

            await table.InsertAsync(item);
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            await table.DeleteAsync(item);
            await service.SyncContext.PushAsync();

            Assert.Equal(0L, service.SyncContext.PendingOperations);
        }

        [Fact]
        public async Task DeleteAsync_CancelsUpdate_WhenUpdateIsInQueue()
        {
            var store = new MobileServiceLocalStoreMock();
            await this.TestCollapseCancel(firstOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                          operationOnItem2: (table, item2) => table.InsertAsync(item2),
                                          secondOperationOnItem1: (table, item1) => table.DeleteAsync(item1),
                                          assertRequest: (req, executed) =>
                                          {
                                              if (executed == 1) // order is maintained by doing insert first and delete after that. This means first update was cancelled, not the second one.
                                              {
                                                  Assert.Equal(req.Method, HttpMethod.Post);
                                              }
                                              else
                                              {
                                                  Assert.Equal(req.Method, HttpMethod.Delete);
                                              }
                                          },
                                          assertQueue: queue =>
                                          {
                                              var op = queue.Values.Single(o => o.Value<string>("itemId") == "item1");
                                              Assert.Equal(2L, op.Value<long>("version"));
                                          });
        }

        [Fact]
        public Task InsertAsync_Throws_WhenDeleteIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.DeleteAsync(item),
                                          secondOperation: (table, item) => table.InsertAsync(item));
        }

        [Fact]
        public Task InsertAsync_Throws_WhenUpdateIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.UpdateAsync(item),
                                          secondOperation: (table, item) => table.InsertAsync(item));
        }

        [Fact]
        public Task InsertAsync_Throws_WhenInsertIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.InsertAsync(item),
                                          secondOperation: (table, item) => table.InsertAsync(item));
        }

        [Fact]
        public Task UpdateAsync_Throws_WhenDeleteIsInQueue()
        {
            return this.TestCollapseThrow(firstOperation: (table, item) => table.DeleteAsync(item),
                                          secondOperation: (table, item) => table.UpdateAsync(item));
        }

        [Fact]
        public async Task UpdateAsync_CancelsSecondUpdate_WhenUpdateIsInQueue()
        {
            await this.TestCollapseCancel(firstOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                        operationOnItem2: (table, item2) => table.DeleteAsync(item2),
                                        secondOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                        assertRequest: (req, executed) =>
                                        {
                                            if (executed == 1) // order is maintained by doing update first and delete after that. This means second update was cancelled, not the first one.
                                            {
                                                Assert.Equal(req.Method, new HttpMethod("Patch"));
                                            }
                                            else
                                            {
                                                Assert.Equal(req.Method, HttpMethod.Delete);
                                            }
                                        },
                                        assertQueue: queue =>
                                        {
                                            var op = queue.Values.Single(o => o.Value<string>("itemId") == "item1");
                                            Assert.Equal(2L, op.Value<long>("version"));
                                        });
        }

        [Fact]
        public async Task Collapse_DeletesTheError_OnMutualCancel()
        {
            var store = new MobileServiceLocalStoreMock();
            var hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var item = new StringIdType() { Id = "item1", String = "what?" };

            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            await table.InsertAsync(item);
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            string id = store.TableMap[MobileServiceLocalSystemTables.OperationQueue].Values.First().Value<string>("id");
            // inject an error to test if it is deleted on collapse
            store.TableMap[MobileServiceLocalSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { id, new JObject() } };

            await table.DeleteAsync(item);
            Assert.Equal(0L, service.SyncContext.PendingOperations);

            // error should be deleted
            Assert.Empty(store.TableMap[MobileServiceLocalSystemTables.SyncErrors]);
        }

        [Fact]
        public async Task Collapse_DeletesTheError_OnReplace()
        {
            var store = new MobileServiceLocalStoreMock();
            var hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var item = new StringIdType() { Id = "item1", String = "what?" };

            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            await table.InsertAsync(item);
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            string id = store.TableMap[MobileServiceLocalSystemTables.OperationQueue].Values.First().Value<string>("id");

            // inject an error to test if it is deleted on collapse
            store.TableMap[MobileServiceLocalSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { id, new JObject() } };

            await table.UpdateAsync(item);
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            // error should be deleted
            Assert.Empty(store.TableMap[MobileServiceLocalSystemTables.SyncErrors]);
        }


        [Fact]
        public async Task UpdateAsync_CancelsSecondUpdate_WhenInsertIsInQueue()
        {
            await this.TestCollapseCancel(firstOperationOnItem1: (table, item1) => table.InsertAsync(item1),
                                        operationOnItem2: (table, item2) => table.DeleteAsync(item2),
                                        secondOperationOnItem1: (table, item1) => table.UpdateAsync(item1),
                                        assertRequest: (req, executed) =>
                                        {
                                            if (executed == 1) // order is maintained by doing insert first and delete after that. This means second update was cancelled.
                                            {
                                                Assert.Equal(req.Method, HttpMethod.Post);
                                            }
                                            else
                                            {
                                                Assert.Equal(req.Method, HttpMethod.Delete);
                                            }
                                        },
                                        assertQueue: queue =>
                                        {
                                            var op = queue.Values.Single(o => o.Value<string>("itemId") == "item1");
                                            Assert.Equal(2L, op.Value<long>("version"));
                                        });
        }

        [Fact]
        public async Task ReadAsync_PassesOdataToStore_WhenLinqIsUsed()
        {
            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("{count: 1, results: [{\"id\":\"abc\",\"String\":\"Hey\"}]}");

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            IMobileServiceTableQuery<string> query = table.Skip(5)
                                                          .Take(3)
                                                          .Where(t => t.String == "world")
                                                          .OrderBy(o => o.Id)
                                                          .OrderByDescending(o => o.String)
                                                          .IncludeTotalCount()
                                                          .Select(x => x.String);

            IEnumerable<string> result = await table.ReadAsync(query);

            string odata = store.ReadQueries.First().ToODataString();
            Assert.Equal(odata, "$filter=(String eq 'world')&" +
                                    "$orderby=String desc,id&" +
                                    "$skip=5&" +
                                    "$top=3&" +
                                    "$select=String&" +
                                    "$inlinecount=allpages");
        }

        [Fact]
        public async Task ToEnumerableAsync_ParsesOData_WhenRawQueryIsProvided()
        {
            var store = new MobileServiceLocalStoreMock();
            store.ReadResponses.Enqueue("{count: 1, results: [{\"id\":\"abc\",\"String\":\"Hey\"}]}");

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            string odata = "$filter=(String eq 'world')&" +
                            "$orderby=String desc,id&" +
                            "$skip=5&" +
                            "$top=3&" +
                            "$select=String&" +
                            "$inlinecount=allpages";

            await table.ReadAsync(odata);

            string odata2 = store.ReadQueries.First().ToODataString();
            Assert.Equal(odata, odata2);
        }

        [Fact]
        public async Task InsertAsync_Throws_WhenObjectIsNull()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            var table = service.GetSyncTable<StringIdType>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertAsync(null));
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenObjectIsNull()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            var table = service.GetSyncTable<StringIdType>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => table.UpdateAsync(null));
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenObjectIsNull()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            var table = service.GetSyncTable<StringIdType>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteAsync(null));
        }

        /// <summary>
        /// Tests that the second operation on the same item will cancel one of the two operations how ever other operations between the two (on other items) are not reordered
        /// </summary>
        /// <param name="firstOperationOnItem1">first operation on item 1</param>
        /// <param name="operationOnItem2">operation on item 2</param>
        /// <param name="secondOperationOnItem1">second operation on item 1</param>
        /// <param name="assertRequest">To check which of the two operations got cancelled</param>
        private async Task TestCollapseCancel(Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> firstOperationOnItem1,
                                              Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> operationOnItem2,
                                              Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> secondOperationOnItem1,
                                              Action<HttpRequestMessage, int> assertRequest,
                                              Action<Dictionary<string, JObject>> assertQueue)
        {
            var store = new MobileServiceLocalStoreMock();
            var hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var item1 = new StringIdType() { Id = "item1", String = "what?" };
            var item2 = new StringIdType() { Id = "item2", String = "this" };
            int executed = 0;
            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            hijack.OnSendingRequest = req =>
            {
                ++executed;
                assertRequest(req, executed);
                return Task.FromResult(req);
            };

            await service.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            await firstOperationOnItem1(table, item1);
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            await operationOnItem2(table, item2);
            Assert.Equal(2L, service.SyncContext.PendingOperations);

            await secondOperationOnItem1(table, item1);
            Assert.Equal(2L, service.SyncContext.PendingOperations);

            Dictionary<string, JObject> queue = store.TableMap[MobileServiceLocalSystemTables.OperationQueue];
            assertQueue(queue);

            await service.SyncContext.PushAsync();

            Assert.Equal(0L, service.SyncContext.PendingOperations);
            Assert.Equal(2, executed); // total two operations executed
        }

        /// <summary>
        /// Tests that the second operation on the same item will throw if first operation is in the queue
        /// </summary>
        /// <param name="firstOperation">The operation that was already in queue.</param>
        /// <param name="secondOperation">The operation that came in late but could not be collapsed.</param>
        /// <returns></returns>
        private async Task TestCollapseThrow(Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> firstOperation, Func<IMobileServiceSyncTable<StringIdType>, StringIdType, Task> secondOperation)
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());
            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await firstOperation(table, item);
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            await Assert.ThrowsAsync<InvalidOperationException>(() => secondOperation(table, item));

            Assert.Equal(1L, service.SyncContext.PendingOperations);
        }

        /// <summary>
        /// Tests that throwing an exception of type toThrow from the http handler causes the push sync to be aborted
        /// </summary>
        /// <param name="toThrow">The exception to simulate coming from http layer</param>
        /// <param name="expectedStatus">The expected status of push operation as reported in PushCompletionResult and PushFailedException</param>
        /// <returns></returns>
        private async Task TestPushAbort(Exception toThrow, MobileServicePushStatus expectedStatus)
        {
            bool thrown = false;
            var hijack = new TestHttpHandler();
            hijack.OnSendingRequest = req =>
            {
                if (!thrown)
                {
                    thrown = true;
                    throw toThrow;
                }
                return Task.FromResult(req);
            };

            var operationHandler = new MobileServiceSyncHandlerMock();

            hijack.SetResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), operationHandler);

            IMobileServiceSyncTable<StringIdType> table = service.GetSyncTable<StringIdType>();

            var item = new StringIdType() { Id = "an id", String = "what?" };
            await table.InsertAsync(item);

            Assert.Equal(1L, service.SyncContext.PendingOperations);

            MobileServicePushFailedException ex = await Assert.ThrowsAsync<MobileServicePushFailedException>(service.SyncContext.PushAsync);

            Assert.Equal(ex.PushResult.Status, expectedStatus);
            Assert.Empty(ex.PushResult.Errors);

            Assert.Equal(operationHandler.PushCompletionResult.Status, expectedStatus);

            // the insert operation is still in queue
            Assert.Equal(1L, service.SyncContext.PendingOperations);

            await service.SyncContext.PushAsync();

            Assert.Equal(0L, service.SyncContext.PendingOperations);
            Assert.Equal(MobileServicePushStatus.Complete, operationHandler.PushCompletionResult.Status);
        }

        private static void MatchUris(List<HttpRequestMessage> requests, params string[] uris)
        {
            Assert.Equal(uris.Length, requests.Count);
            for (int i = 0; i < uris.Length; i++)
            {
                var actualUri = requests[i].RequestUri;
                var expectedUri = new Uri(uris[i], UriKind.Absolute);
                Assert.Equal(expectedUri.Host, actualUri.Host);
                Assert.Equal(expectedUri.AbsolutePath, actualUri.AbsolutePath);
                Assert.Equal(Uri.UnescapeDataString(expectedUri.Query), Uri.UnescapeDataString(actualUri.Query));
            }
        }
    }
}
