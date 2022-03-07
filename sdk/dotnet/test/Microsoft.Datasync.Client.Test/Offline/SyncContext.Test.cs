// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Operations;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Offline
{
    [ExcludeFromCodeCoverage]
    public class SyncContext_Tests : BaseOfflineTest
    {
        [Fact]
        public void PendingOperations_DoesNotThrow_IfItIsNotInitialized()
        {
            var client = new Mock<DatasyncClient>();
            var context = new SyncContext(client.Object);
            Assert.Equal(0, context.PendingOperations);
        }

        [Fact]
        public async Task InitializeAsync_Throws_WhenStoreIsNull()
        {
            var client = GetMockClient();
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.SyncContext.InitializeAsync(null));
        }

        [Fact]
        public async Task InitializeAsync_DoesNotThrow_WhenSyncHandlerIsNull()
        {
            var client = GetMockClient();
            await client.SyncContext.InitializeAsync(new MockLocalStore(), null);
        }

        [Fact]
        public async Task PushAsync_ExecutesThePendingOperations_InOrder()
        {
            var store = new MockLocalStore();
            var service = GetHijackedClient();
            await service.SyncContext.InitializeAsync(store);
            var table = service.GetOfflineTable("someTable");

            JObject item1 = new() { { "id", "abc" } }, item2 = new() { { "id", "def" } };

            await table.InsertItemAsync(item1);
            await table.InsertItemAsync(item2);

            Assert.Empty(MockHandler.Requests);

            // create a new service to test that operations are loaded from store
            service = GetHijackedClient();
            await service.SyncContext.InitializeAsync(store);

            Assert.Empty(hijack.Requests);
            await service.SyncContext.PushAsync();
            Assert.Equal(2, hijack.Requests.Count);

            Assert.Equal(item1.ToString(Formatting.None), await hijack.Requests[0].Content.ReadAsStringAsync());
            Assert.Equal(item2.ToString(Formatting.None), await hijack.Requests[1].Content.ReadAsStringAsync());

            // create yet another service to make sure the old items were purged from queue
            service = GetHijackedClient();
            hijack.AddResponseContent("{\"id\":\"abc\",\"String\":\"Hey\"}");
            await service.SyncContext.InitializeAsync(store);
            Assert.Empty(hijack.Requests);
            await service.SyncContext.PushAsync();
            Assert.Empty(hijack.Requests);
        }

        [Fact]
        public async Task PushAsync_ReplaysStoredErrors_IfTheyAreInStore()
        {
            var error = new TableOperationError("abc", 1, TableOperationKind.Update, HttpStatusCode.PreconditionFailed, "test", new JObject(), "{}", new JObject());
            var store = new MockLocalStore();
            await store.UpsertAsync(OfflineSystemTables.SyncErrors, error.Serialize(), fromServer: false);

            var service = GetHijackedClient();
            await service.SyncContext.InitializeAsync(store);
            await Assert.ThrowsAsync<PushFailedException>(() => service.SyncContext.PushAsync());
        }

        [Fact]
        public async Task PushAsync_Succeeds_WhenDeleteReturnsNotFound()
        {
            var store = new MockLocalStore();
            var service = GetHijackedClient();
            await service.SyncContext.InitializeAsync(store);
            var table = service.GetOfflineTable("someTable");
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.NotFound));

            await table.DeleteItemAsync(new JObject() { { "id", "abc" }, { "version", "Wow" } });
            await service.SyncContext.PushAsync();
        }

        [Fact]
        public async Task PushAsync_DoesNotRunHandler_WhenTableTypeIsNotTable()
        {
            var store = new MockLocalStore();
            var service = GetHijackedClient();
            bool invoked = false;

            var handler = new MockSyncHandler
            {
                TableOperationAction = op => { invoked = true; throw new InvalidOperationException(); }
            };
            hijack.AddResponseContent("{\"id\":\"abc\",\"version\":\"Hey\"}");

            await service.SyncContext.InitializeAsync(store, handler);
            var table = service.GetOfflineTable("someTable");

            await table.InsertItemAsync(new JObject() { { "id", "abc" }, { "version", "Wow" } });
            await service.SyncContext.PushAsync();

            Assert.False(invoked);
        }

        [Fact]
        public async Task PushAsync_InvokesHandler_WhenTableTypeIsTable()
        {
            var store = new MockLocalStore();
            var service = GetHijackedClient();
            bool invoked = false;

            var handler = new MockSyncHandler
            {
                TableOperationAction = op => { invoked = true; return Task.FromResult(JObject.Parse("{\"id\":\"abc\",\"version\":\"Hey\"}")); }
            };
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            await service.SyncContext.InitializeAsync(store, handler);
            var table = service.GetOfflineTable("someTable");

            await table.InsertItemAsync(new JObject() { { "id", "abc" }, { "version", "Wow" } });
            await service.SyncContext.PushAsync();

            Assert.True(invoked);
        }

        [Fact]
        public async Task UpdateOperationAsync_UpsertsTheItemInLocalStore_AndDeletesTheError_FromSyncHandler()
        {
            // Arrange
            string itemId = "abc";
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var handler = new MobileServiceSyncHandlerMock();
            handler.PushCompleteAction = async pushCompletionResult =>
            {
                foreach (var error in pushCompletionResult.Errors)
                {
                    await error.UpdateOperationAsync(JObject.Parse("{\"id\":\"abc\",\"__version\":\"Hey\"}"));
                }
            };
            var store = new MobileServiceLocalStoreMock();
            IDatasyncClient service = new DatasyncClient("http://www.test.com", hijack);
            await service.SyncContext.InitializeAsync(store, handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            await table.InsertAsync(new JObject() { { "id", "abc" }, { "__version", "Wow" } });

            // Act
            await (service.SyncContext as MobileServiceSyncContext).PushAsync(CancellationToken.None, MobileServiceTableKind.Table);


            // Assert
            var syncError = store.TableMap[OfflineSystemTables.SyncErrors].Values;
            var operation = store.TableMap[OfflineSystemTables.OperationQueue].Values.FirstOrDefault();
            var item = JObject.Parse("{\"id\":\"abc\",\"__version\":\"Hey\"}");
            JObject upserted = await store.LookupAsync("someTable", itemId);
            // item is upserted
            Assert.NotNull(upserted);

            // verify if the record was updated
            Assert.Equal(item.ToString(), upserted.ToString());

            // verify if the errors were cleared
            Assert.Empty(syncError);

            // Verify operation is still present
            Assert.Equal(operation.Value<string>("itemId"), itemId);
        }

        [Fact]
        public async Task PushAsync_Succeeds_WithClientWinsPolicy()
        {
            var hijack = new TestHttpHandler();
            hijack.Responses.Add(new HttpResponseMessage(HttpStatusCode.PreconditionFailed)
            {
                Content = new StringContent("{\"id\":\"abc\",\"version\":\"Hey\"}")
            });
            hijack.AddResponseContent(@"{""id"": ""abc""}");

            var handler = new MobileServiceSyncHandlerMock();
            handler.TableOperationAction = async op =>
            {
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        return await op.ExecuteAsync();
                    }
                    catch (MobileServicePreconditionFailedException ex)
                    {
                        op.Item[MobileServiceSystemColumns.Version] = ex.Value[MobileServiceSystemColumns.Version];
                    }
                }
                return null;
            };
            IDatasyncClient service = new DatasyncClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), handler);

            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            await table.UpdateAsync(new JObject() { { "id", "abc" }, { "version", "Wow" } });

            await service.SyncContext.PushAsync();
        }

        class LocalStoreWithDelay : MobileServiceLocalStoreMock
        {
            private int _lookupDelayInMilliseconds = 0;

            public override async Task<JObject> LookupAsync(string tableName, string id)
            {
                if (0 != _lookupDelayInMilliseconds)
                {
                    // releases this thread and causes this lookup to take longer
                    await Task.Delay(_lookupDelayInMilliseconds);
                }

                return await base.LookupAsync(tableName, id);
            }

            /// <summary>
            /// Tells this mock to yield and to delay for the given number of milliseconds before proceeding with the LookupAsync().
            /// </summary>
            /// <param name="delayInMilliseconds">The delay in milliseconds.</param>
            internal void SetLookupDelay(int delayInMilliseconds)
            {
                _lookupDelayInMilliseconds = delayInMilliseconds;
            }
        }

        [Fact]
        public async Task PushAsync_Succeeds_WithPendingOperations_AndOpQueueIsConsistent()
        {
            // Essentially async ManualResetEvents
            SemaphoreSlim untilPendingOpsCreated = new SemaphoreSlim(0, 1);
            SemaphoreSlim untilAboutToExecuteOp = new SemaphoreSlim(0, 1);

            int pushState = 0;

            var handler = new MobileServiceSyncHandlerMock();

            handler.TableOperationAction = async op =>
            {
                untilAboutToExecuteOp.Release();
                await untilPendingOpsCreated.WaitAsync();

                JObject result = await op.ExecuteAsync();

                if (0 == pushState)
                {
                    Assert.Equal(MobileServiceTableOperationKind.Insert, op.Kind);
                    Assert.Equal(0, op.Item.Value<int>("value"));
                }
                else
                {
                    Assert.Equal(MobileServiceTableOperationKind.Update, op.Kind);
                    Assert.Equal(2, op.Item.Value<int>("value")); // We shouldn't see the value == 1, since it should have been collapsed
                }

                // We don't care what the server actually returned, as long as there was no exception raised in our Push logic
                return result;
            };

            var hijack = new TestHttpHandler();

            IDatasyncClient service = new DatasyncClient("http://www.test.com", hijack);
            LocalStoreWithDelay mockLocalStore = new LocalStoreWithDelay();
            await service.SyncContext.InitializeAsync(mockLocalStore, handler);

            JObject item = null;

            // Add the initial operation and perform a push
            IMobileServiceSyncTable table = service.GetSyncTable("someTable");

            string responseContent = @"{ ""id"": ""abc"", ""value"": ""0"", ""version"": ""v0"" }"; // Whatever is fine, since we won't use it or look at it

            // Do this Insert/Push/Update+Update/Push cycle several times fast to try to hit any race conditions that would cause an error
            for (int id = 0; id < 10; id++)
            {
                hijack.SetResponseContent(responseContent);
                string idStr = "id" + id; // Generate a new Id each time in case the mock objects ever care that we insert an item that already exists

                // The Operations and PushAction don't necessarily clone the JObject, so we need a fresh one for each operation or else we'll change
                // the in-memory representation of the JObject stored in all operations, as well as in the "batch" the PushAction owns. This is problematic.
                item = new JObject() { { "id", idStr }, { "value", 0 } };
                await table.InsertAsync(item);

                Task pushComplete = service.SyncContext.PushAsync();

                // Make sure the PushAction has actually called into our SyncHandler, otherwise the two UpdateOperations could collapse onto it, and
                // there won't necessarily even be a second PushAction
                await untilAboutToExecuteOp.WaitAsync();

                // Add some more operations while that push is in flight. Since these operations affect the same item in someTable, the operations
                // will be stuck awaiting the PushAction since it locks on the row.
                item = new JObject() { { "id", idStr }, { "value", 1 } };
                Task updateOnce = table.UpdateAsync(item);

                item = new JObject() { { "id", idStr }, { "value", 2 } };
                Task updateTwice = table.UpdateAsync(item);

                // Before we let the push finish, let's inject a delay that will cause it to take a long time deleting the operation from the queue.
                // This will give the other operations, if there's an unaddressed race condition, a chance to wreak havoc on the op queue.
                mockLocalStore.SetLookupDelay(500);

                // Let the first push finish
                untilPendingOpsCreated.Release();
                await pushComplete;

                mockLocalStore.SetLookupDelay(0);

                await updateOnce;
                await updateTwice;

                // Push again, but now the operation condensed from the two updates should be executed remotely
                pushState = (pushState + 1) % 2;
                hijack.SetResponseContent(responseContent);
                pushComplete = service.SyncContext.PushAsync();
                await untilAboutToExecuteOp.WaitAsync(); // not strictly necessary other than to keep the semaphore count at 0
                untilPendingOpsCreated.Release();

                await pushComplete;
                pushState = (pushState + 1) % 2;
            }
        }

        [Fact]
        public async Task CancelAndUpdateItemAsync_UpsertsTheItemInLocalStore_AndDeletesTheOperationAndError()
        {
            var client = new DatasyncClient(MobileAppUriValidator.DummyMobileApp);
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";


            store.TableMap[OfflineSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { operationId, new JObject() } };
            store.TableMap[OfflineSystemTables.OperationQueue].Add(operationId, new JObject());

            // operation exists before cancel
            Assert.NotNull(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // item doesn't exist before upsert
            Assert.Null(await store.LookupAsync(tableName, itemId));

            var error = new MobileServiceTableOperationError(operationId,
                                                             0,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());

            var item = new JObject() { { "id", itemId }, { "name", "unknown" } };
            await context.CancelAndUpdateItemAsync(error, item);

            // operation is deleted
            Assert.Null(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // error is deleted
            Assert.Null(await store.LookupAsync(OfflineSystemTables.SyncErrors, operationId));

            JObject upserted = await store.LookupAsync(tableName, itemId);
            // item is upserted
            Assert.NotNull(upserted);
            Assert.Equal(item, upserted);
        }

        [Fact]
        public async Task UpdateOperationAsync_UpsertsTheItemInLocalStore_AndDeletesTheError_AndUpdatesTheOperation()
        {
            var client = new DatasyncClient("http://www.test.com");
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";


            store.TableMap[OfflineSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { operationId, new JObject() { { "id", operationId }, { "version", 1 } } } };
            store.TableMap[OfflineSystemTables.OperationQueue].Add(operationId, new JObject() { { "id", operationId }, { "version", 1 } });
            store.TableMap.Add(tableName, new Dictionary<string, JObject>() { { itemId, new JObject() } });

            // operation exists before cancel
            Assert.NotNull(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // item exists before upsert
            Assert.NotNull(await store.LookupAsync(tableName, itemId));

            var error = new MobileServiceTableOperationError(operationId,
                                                             1,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());

            var item = new JObject() { { "id", itemId }, { "name", "unknown" } };
            await context.UpdateOperationAsync(error, item);

            // operation is updated
            Assert.NotNull(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // error is deleted
            Assert.Null(await store.LookupAsync(OfflineSystemTables.SyncErrors, operationId));

            JObject upserted = await store.LookupAsync(tableName, itemId);
            // item is upserted
            Assert.NotNull(upserted);
            Assert.Equal(item, upserted);
        }

        [Fact]
        public async Task UpdateOperationAsync_UpsertTheItemInOperation_AndDeletesTheError()
        {
            var client = new DatasyncClient("http://www.test.com");
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";

            var item = new JObject() { { "id", itemId }, { "name", "unknown" } };

            store.TableMap[OfflineSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { operationId, new JObject() { { "id", operationId }, { "version", 1 } } } };
            store.TableMap[OfflineSystemTables.OperationQueue].Add(operationId, new JObject() { { "id", operationId }, { "version", 1 }, { "item", item.ToString() }, { "kind", (int)MobileServiceTableOperationKind.Delete } });

            // operation exists before cancel
            Assert.NotNull(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // item does not exist
            Assert.Null(await store.LookupAsync(tableName, itemId));

            var error = new MobileServiceTableOperationError(operationId,
                                                             1,
                                                             MobileServiceTableOperationKind.Delete,
                                                             HttpStatusCode.PreconditionFailed,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());
            var item2 = new JObject() { { "id", itemId }, { "name", "unknown" }, { "version", 2 } };
            await context.UpdateOperationAsync(error, item2);

            var operation = await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId);
            // operation is updated
            Assert.NotNull(operation);
            // error is deleted
            Assert.Null(await store.LookupAsync(OfflineSystemTables.SyncErrors, operationId));

            Assert.Equal(operation.GetValue("item").ToString(), item2.ToString(Formatting.None));
        }

        [Fact]
        public async Task UpdateOperationAsync_Throws_IfOperationIsModified()
        {
            string errorMessage = "The operation has been updated and cannot be updated again";
            await TestOperationModifiedException(true, (error, context) => context.UpdateOperationAsync(error, new JObject()), errorMessage);
        }

        [Fact]
        public async Task UpdateOperationAsync_Throws_IfOperationDoesNotExist()
        {
            string errorMessage = "The operation has been updated and cannot be updated again";
            await TestOperationModifiedException(false, (error, context) => context.UpdateOperationAsync(error, new JObject()), errorMessage);
        }

        [Fact]
        public async Task CancelAndDiscardItemAsync_DeletesTheItemInLocalStore_AndDeletesTheOperationAndError()
        {
            var client = new DatasyncClient(MobileAppUriValidator.DummyMobileApp);
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";

            store.TableMap[OfflineSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { operationId, new JObject() } };
            store.TableMap[OfflineSystemTables.OperationQueue].Add(operationId, new JObject());
            store.TableMap.Add(tableName, new Dictionary<string, JObject>() { { itemId, new JObject() } });

            // operation exists before cancel
            Assert.NotNull(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // item exists before upsert
            Assert.NotNull(await store.LookupAsync(tableName, itemId));

            var error = new MobileServiceTableOperationError(operationId,
                                                             0,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());

            await context.CancelAndDiscardItemAsync(error);

            // operation is deleted
            Assert.Null(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // error is deleted
            Assert.Null(await store.LookupAsync(OfflineSystemTables.SyncErrors, operationId));

            // item is upserted
            Assert.Null(await store.LookupAsync(tableName, itemId));
        }

        [Fact]
        public async Task CancelAndUpdateItemAsync_Throws_IfOperationDoesNotExist()
        {
            string errorMessage = "The operation has been updated and cannot be cancelled.";
            await TestOperationModifiedException(false, (error, context) => context.CancelAndUpdateItemAsync(error, new JObject()), errorMessage);
        }

        [Fact]
        public async Task CancelAndDiscardItemAsync_Throws_IfOperationDoesNotExist()
        {
            string errorMessage = "The operation has been updated and cannot be cancelled.";
            await TestOperationModifiedException(false, (error, context) => context.CancelAndDiscardItemAsync(error), errorMessage);
        }


        [Fact]
        public async Task CancelAndUpdateItemAsync_Throws_IfOperationIsModified()
        {
            string errorMessage = "The operation has been updated and cannot be cancelled.";
            await TestOperationModifiedException(true, (error, context) => context.CancelAndUpdateItemAsync(error, new JObject()), errorMessage);
        }

        [Fact]
        public async Task CancelAndDiscardItemAsync_Throws_IfOperationIsModified()
        {
            string errorMessage = "The operation has been updated and cannot be cancelled.";
            await TestOperationModifiedException(true, (error, context) => context.CancelAndDiscardItemAsync(error), errorMessage);
        }

        private async Task TestOperationModifiedException(bool operationExists, Func<MobileServiceTableOperationError, MobileServiceSyncContext, Task> action, String errorMessage)
        {
            var client = new DatasyncClient(MobileAppUriValidator.DummyMobileApp);
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store);

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";

            if (operationExists)
            {
                store.TableMap[OfflineSystemTables.OperationQueue].Add(operationId, new JObject() { { "version", 3 } });
            }
            else
            {
                // operation exists before cancel
                Assert.Null(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            }

            var error = new MobileServiceTableOperationError(operationId,
                                                             1,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());

            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => action(error, context));
            Assert.Equal(errorMessage, ex.Message);
        }

        [Fact]
        public async Task CancelAndDiscardItemAsync_WithNotificationsEnabled_UsesTheCorrectStoreOperationSource()
        {
            await ConflictOperation_WithNotificationsEnabled_UsesTheCorrectStoreOperationSource(
                async (context, error) => await context.CancelAndDiscardItemAsync(error));
        }

        [Fact]
        public async Task CancelAndUpdateItemAsync_WithNotificationsEnabled_UsesTheCorrectStoreOperationSource()
        {
            await ConflictOperation_WithNotificationsEnabled_UsesTheCorrectStoreOperationSource(
                async (context, error) => await context.CancelAndUpdateItemAsync(error, new JObject() { { "id", string.Empty } }));
        }

        [Fact]
        private async Task UpdateOperationAsync_ConflictOperation_WithNotificationsEnabled_UsesTheCorrectStoreSource()
        {
            var client = new DatasyncClient("http://www.test.com");
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            await context.InitializeAsync(store, StoreTrackingOptions.NotifyLocalConflictResolutionOperations);
            var manualResetEvent = new ManualResetEventSlim();

            string operationId = "abc";
            string itemId = "def";
            string tableName = "test";


            store.TableMap[OfflineSystemTables.SyncErrors] = new Dictionary<string, JObject>() { { operationId, new JObject() { { "id", operationId }, { "version", 1 } } } };
            store.TableMap[OfflineSystemTables.OperationQueue].Add(operationId, new JObject() { { "id", operationId }, { "version", 1 } });
            store.TableMap.Add(tableName, new Dictionary<string, JObject>() { { itemId, new JObject() } });

            // operation exists before cancel
            Assert.NotNull(await store.LookupAsync(OfflineSystemTables.OperationQueue, operationId));
            // item exists before upsert
            Assert.NotNull(await store.LookupAsync(tableName, itemId));


            var error = new MobileServiceTableOperationError(operationId,
                                                             1,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());
            var item = new JObject() { { "id", itemId }, { "name", "unknown" } };

            bool sourceIsLocalConflict = false;
            IDisposable subscription = client.EventManager.Subscribe<StoreOperationCompletedEvent>(o =>
            {
                sourceIsLocalConflict = o.Operation.Source == StoreOperationSource.LocalConflictResolution;
                manualResetEvent.Set();
            });

            await context.UpdateOperationAsync(error, item);

            bool resetEventSignaled = manualResetEvent.Wait(1000);
            subscription.Dispose();

            Assert.True(resetEventSignaled);
            Assert.True(sourceIsLocalConflict);
        }

        private async Task ConflictOperation_WithNotificationsEnabled_UsesTheCorrectStoreOperationSource(Func<MobileServiceSyncContext, MobileServiceTableOperationError, Task> handler)
        {
            var client = new DatasyncClient(MobileAppUriValidator.DummyMobileApp);
            var store = new MobileServiceLocalStoreMock();
            var context = new MobileServiceSyncContext(client);
            var manualResetEvent = new ManualResetEventSlim();

            await context.InitializeAsync(store, StoreTrackingOptions.NotifyLocalConflictResolutionOperations);

            string operationId = "abc";
            string itemId = string.Empty;
            string tableName = "test";

            store.TableMap[OfflineSystemTables.OperationQueue].Add(operationId, new JObject() { { "version", 1 } });


            var error = new MobileServiceTableOperationError(operationId,
                                                             1,
                                                             MobileServiceTableOperationKind.Update,
                                                             HttpStatusCode.Conflict,
                                                             tableName,
                                                             item: new JObject() { { "id", itemId } },
                                                             rawResult: "{}",
                                                             result: new JObject());


            bool sourceIsLocalConflict = false;
            IDisposable subscription = client.EventManager.Subscribe<StoreOperationCompletedEvent>(o =>
            {
                sourceIsLocalConflict = o.Operation.Source == StoreOperationSource.LocalConflictResolution;
                manualResetEvent.Set();
            });

            await handler(context, error);

            bool resetEventSignaled = manualResetEvent.Wait(1000);
            subscription.Dispose();

            Assert.True(resetEventSignaled);
            Assert.True(sourceIsLocalConflict);
        }
    }
}
