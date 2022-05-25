// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DeviceTests.Shared.Tests
{
    [Collection(nameof(SingleThreadedCollection))]
    public class Offline_Tests : E2ETestBase
    {
        public static string StoreFileName = "store.bin";

        [Fact]
        public async Task BasicOfflineTest()
        {
            ClearStore();
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);

            CountingHandler handler = new CountingHandler();
            var requestsSentToServer = 0;
            var offlineReadyClient = CreateClient(handler);

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            localStore.DefineTable<OfflineReadyItem>();

            await offlineReadyClient.SyncContext.InitializeAsync(localStore);

            var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
            var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();

            var item = new OfflineReadyItem(rndGen);
            try
            {
                await localTable.InsertAsync(item);
                await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () =>
                {
                    requestsSentToServer++;
                    await remoteTable.LookupAsync(item.Id);
                });

                Func<int, bool> validateRequestCount = expectedCount => (handler.RequestCount == expectedCount);
                Assert.True(validateRequestCount(requestsSentToServer));

                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer++;

                Assert.True(validateRequestCount(requestsSentToServer));

                var serverItem = await remoteTable.LookupAsync(item.Id);
                requestsSentToServer++;
                Assert.Equal(serverItem, item);

                item.Flag = !item.Flag;
                item.Age++;
                item.Date = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, DateTimeKind.Utc);
                await localTable.UpdateAsync(item);

                var newItem = new OfflineReadyItem(rndGen);
                await localTable.InsertAsync(newItem);

                Assert.True(validateRequestCount(requestsSentToServer));

                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer += 2;

                Assert.True(validateRequestCount(requestsSentToServer));

                serverItem = await remoteTable.LookupAsync(item.Id);
                requestsSentToServer++;
                Assert.Equal(serverItem, item);

                serverItem = await remoteTable.LookupAsync(newItem.Id);
                requestsSentToServer++;
                Assert.Equal(serverItem, newItem);

                await localTable.DeleteAsync(item);
                await localTable.DeleteAsync(newItem);
                await offlineReadyClient.SyncContext.PushAsync();
                requestsSentToServer += 2;
                Assert.True(validateRequestCount(requestsSentToServer));
            }
            catch (MobileServicePushFailedException)
            {
                throw;
            }
            finally
            {
                localStore.Dispose();
                ClearStore();
            }
        }

        [Fact]
        public async Task ClientResolvesConflictsTest()
        {
            await CreateSyncConflict(true);
        }

        [Fact]
        public async Task PushFailsAfterConflictsTest()
        {
            await CreateSyncConflict(false);
        }

        [Fact]
        public async Task AbortPushAtStartSyncTest()
        {
            await AbortPushDuringSync(SyncAbortLocation.Start);
        }

        [Fact]
        public async Task AbortPushAtMiddleSyncTest()
        {
            await AbortPushDuringSync(SyncAbortLocation.Middle);
        }

        [Fact]
        public async Task AbortPushAtEndSyncTest()
        {
            await AbortPushDuringSync(SyncAbortLocation.End);
        }

        //[Fact]
        //[Tag("notpassing")]
        //private async Task NoOptimisticConcurrencyTest()
        //{
        //    // If a table does not have a version column, then offline will still
        //    // work, but there will be no conflicts
        //    DateTime now = DateTime.UtcNow;
        //    int seed = now.Year * 10000 + now.Month * 100 + now.Day;
        //    Log("Using random seed: {0}", seed);
        //    Random rndGen = new Random(seed);

        //    var offlineReadyClient = CreateClient();

        //    var localStore = new MobileServiceSQLiteStore(StoreFileName);
        //    Log("Defined the table on the local store");
        //    localStore.DefineTable<OfflineReadyItemNoVersion>();

        //    await offlineReadyClient.SyncContext.InitializeAsync(localStore);
        //    Log("Initialized the store and sync context");

        //    var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItemNoVersion>();
        //    var remoteTable = offlineReadyClient.GetTable<OfflineReadyItemNoVersion>();

        //    var item = new OfflineReadyItemNoVersion(rndGen);
        //    try
        //    {
        //        offlineReadyClient.CurrentUser = await Utilities.GetDummyUser(offlineReadyClient);
        //        await localTable.InsertAsync(item);
        //        Log("Inserted the item to the local store:", item);
        //        await offlineReadyClient.SyncContext.PushAsync();

        //        Log("Pushed the changes to the server");

        //        var serverItem = await remoteTable.LookupAsync(item.Id);
        //        serverItem.Name = "changed name";
        //        serverItem.Age = 0;
        //        await remoteTable.UpdateAsync(serverItem);
        //        Log("Server item updated (changes will be overwritten later");

        //        item.Age = item.Age + 1;
        //        item.Name = item.Name + " - modified";
        //        await localTable.UpdateAsync(item);
        //        Log("Updated item locally, will now push changes to the server: {0}", item);
        //        await offlineReadyClient.SyncContext.PushAsync();

        //        serverItem = await remoteTable.LookupAsync(item.Id);
        //        Log("Retrieved the item from the server: {0}", serverItem);

        //        if (serverItem.Equals(item))
        //        {
        //            Log("Items are the same");
        //        }
        //        else
        //        {
        //            Assert.Fail(string.Format("Items are different. Local: {0}; remote: {1}", item, serverItem));
        //        }

        //        Log("Cleaning up");
        //        localTable.DeleteAsync(item).Wait();
        //        Log("Local table cleaned up. Now sync'ing once more");
        //        await offlineReadyClient.SyncContext.PushAsync();
        //    }

        //    catch (MobileServicePushFailedException ex)
        //    {
        //        Log("PushResult status: " + ex.PushResult.Status);
        //        throw;
        //    }
        //    finally
        //    {
        //        localStore.Dispose();
        //        ClearStore();
        //    }
        //    await offlineReadyClient.LogoutAsync();
        //}

        private MobileServiceClient CreateClient(params HttpMessageHandler[] handlers)
        {
            var globalClient = GetClient();
            var offlineReadyClient = new MobileServiceClient(
                globalClient.MobileAppUri,
                handlers);

            if (globalClient.CurrentUser != null)
            {
                offlineReadyClient.CurrentUser = new MobileServiceUser(globalClient.CurrentUser.UserId);
                offlineReadyClient.CurrentUser.MobileServiceAuthenticationToken = globalClient.CurrentUser.MobileServiceAuthenticationToken;
            }

            return offlineReadyClient;
        }

        enum SyncAbortLocation { Start, Middle, End };

        class AbortingSyncHandler : IMobileServiceSyncHandler
        {
            Offline_Tests test;

            public AbortingSyncHandler(Offline_Tests offlineTest, Func<string, bool> shouldAbortForId)
            {
                this.test = offlineTest;
                this.AbortCondition = shouldAbortForId;
            }

            public Func<string, bool> AbortCondition { get; set; }

            public Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
            {
                var itemId = (string)operation.Item[MobileServiceSystemColumns.Id];
                if (this.AbortCondition(itemId))
                {
                    operation.AbortPush();
                }

                return operation.ExecuteAsync();
            }

            public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
            {
                return Task.FromResult(0);
            }
        }

        private async Task AbortPushDuringSync(SyncAbortLocation whereToAbort)
        {
            ClearStore();
            SyncAbortLocation abortLocation = whereToAbort;
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);

            var offlineReadyClient = CreateClient();

            var items = Enumerable.Range(0, 10).Select(_ => new OfflineReadyItem(rndGen)).ToArray();
            foreach (var item in items)
            {
                item.Id = Guid.NewGuid().ToString("D");
            }

            int abortIndex = abortLocation == SyncAbortLocation.Start ? 0 :
                (abortLocation == SyncAbortLocation.End ? items.Length - 1 : rndGen.Next(1, items.Length - 1));
            var idToAbort = items[abortIndex].Id;

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            localStore.DefineTable<OfflineReadyItem>();

            var syncHandler = new AbortingSyncHandler(this, id => id == idToAbort);
            await offlineReadyClient.SyncContext.InitializeAsync(localStore, syncHandler);

            var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
            var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();
            try
            {
                foreach (var item in items)
                {
                    await localTable.InsertAsync(item);
                }

                await Assert.ThrowsAsync<MobileServicePushFailedException>(() => offlineReadyClient.SyncContext.PushAsync());

                var expectedOperationQueueSize = items.Length - abortIndex;
                Assert.False(expectedOperationQueueSize != offlineReadyClient.SyncContext.PendingOperations);

                foreach (var allItemsPushed in new bool[] { false, true })
                {
                    HashSet<OfflineReadyItem> itemsInServer, itemsNotInServer;
                    if (allItemsPushed)
                    {
                        itemsInServer = new HashSet<OfflineReadyItem>(items.ToArray());
                        itemsNotInServer = new HashSet<OfflineReadyItem>(Enumerable.Empty<OfflineReadyItem>());
                    }
                    else
                    {
                        itemsInServer = new HashSet<OfflineReadyItem>(items.Where((item, index) => index < abortIndex));
                        itemsNotInServer = new HashSet<OfflineReadyItem>(items.Where((item, index) => index >= abortIndex));
                    }

                    foreach (var item in items)
                    {
                        var itemFromServer = (await remoteTable.Where(i => i.Id == item.Id).Take(1).ToEnumerableAsync()).FirstOrDefault();
                        Assert.False(itemsInServer.Contains(item) && itemFromServer == null);
                        Assert.False(itemsNotInServer.Contains(item) && itemFromServer != null);
                    }

                    if (!allItemsPushed)
                    {
                        syncHandler.AbortCondition = _ => false;
                        await offlineReadyClient.SyncContext.PushAsync();
                    }
                }

                syncHandler.AbortCondition = _ => false;

                foreach (var item in items)
                {
                    await localTable.DeleteAsync(item);
                }

                await offlineReadyClient.SyncContext.PushAsync();
            }
            finally
            {
                localStore.Dispose();
                ClearStore();
            }
        }

        class CountingHandler : DelegatingHandler
        {
            public int RequestCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.RequestCount++;
                return base.SendAsync(request, cancellationToken);
            }
        }

        class ConflictResolvingSyncHandler<T> : IMobileServiceSyncHandler
        {
            public delegate T ConflictResolution(T clientItem, T serverItem);
            IMobileServiceClient client;
            ConflictResolution conflictResolution;
            Offline_Tests test;

            public ConflictResolvingSyncHandler(Offline_Tests offlineTest, IMobileServiceClient client, ConflictResolution resolutionPolicy)
            {
                this.client = client;
                this.conflictResolution = resolutionPolicy;
                this.test = offlineTest;
            }

            public async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
            {
                MobileServicePreconditionFailedException ex = null;
                JObject result = null;
                do
                {
                    ex = null;
                    try
                    {
                        result = await operation.ExecuteAsync();
                    }
                    catch (MobileServicePreconditionFailedException e)
                    {
                        ex = e;
                    }

                    if (ex != null)
                    {
                        var serverItem = ex.Value;
                        if (serverItem == null)
                        {
                            serverItem = (JObject)(await client.GetTable(operation.Table.TableName).LookupAsync((string)operation.Item["id"]));
                        }

                        var typedClientItem = operation.Item.ToObject<T>();
                        var typedServerItem = serverItem.ToObject<T>();
                        var typedMergedItem = conflictResolution(typedClientItem, typedServerItem);
                        var mergedItem = JObject.FromObject(typedMergedItem);
                        mergedItem[MobileServiceSystemColumns.Version] = serverItem[MobileServiceSystemColumns.Version];
                        operation.Item = mergedItem;
                    }
                } while (ex != null);

                return result;
            }

            public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
            {
                return Task.FromResult(0);
            }
        }

        private async Task CreateSyncConflict(bool autoResolve)
        {
            ClearStore();
            bool resolveConflictsOnClient = autoResolve;
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);

            var offlineReadyClient = CreateClient();

            var localStore = new MobileServiceSQLiteStore(StoreFileName);
            localStore.DefineTable<OfflineReadyItem>();

            ConflictResolvingSyncHandler<OfflineReadyItem>.ConflictResolution conflictHandlingPolicy;
            conflictHandlingPolicy = (client, server) =>
                    new OfflineReadyItem
                    {
                        Id = client.Id,
                        Age = Math.Max(client.Age, server.Age),
                        Date = client.Date > server.Date ? client.Date : server.Date,
                        Flag = client.Flag || server.Flag,
                        FloatingNumber = Math.Max(client.FloatingNumber, server.FloatingNumber),
                        Name = client.Name
                    };
            if (resolveConflictsOnClient)
            {
                var handler = new ConflictResolvingSyncHandler<OfflineReadyItem>(this, offlineReadyClient, conflictHandlingPolicy);
                await offlineReadyClient.SyncContext.InitializeAsync(localStore, handler);
            }
            else
            {
                await offlineReadyClient.SyncContext.InitializeAsync(localStore);
            }

            var localTable = offlineReadyClient.GetSyncTable<OfflineReadyItem>();
            var remoteTable = offlineReadyClient.GetTable<OfflineReadyItem>();

            await localTable.PurgeAsync();

            var item = new OfflineReadyItem(rndGen);
            await remoteTable.InsertAsync(item);

            var pullQuery = "$filter=id eq '" + item.Id + "'";
            await localTable.PullAsync(null, pullQuery);

            item.Age++;
            await remoteTable.UpdateAsync(item);

            var localItem = await localTable.LookupAsync(item.Id);
            localItem.Date = localItem.Date.AddDays(1);
            await localTable.UpdateAsync(localItem);

            string errorMessage = string.Empty;
            try
            {
                await localTable.PullAsync(null, pullQuery);
                if (!autoResolve)
                {
                    errorMessage = "Error, pull (push) should have caused a conflict, but none happened.";
                }
                else
                {
                    var expectedMergedItem = conflictHandlingPolicy(localItem, item);
                    var localMergedItem = await localTable.LookupAsync(item.Id);
                    Assert.Equal(expectedMergedItem, localMergedItem);

                }
            }
            catch (MobileServicePushFailedException)
            {
                if (autoResolve)
                {
                    errorMessage = "Error, push should have succeeded.";
                }
            }

            await localTable.DeleteAsync(item);
            await offlineReadyClient.SyncContext.PushAsync();
            localStore.Dispose();
            ClearStore();
            Assert.True(String.IsNullOrEmpty(errorMessage));
        }

        private static void ClearStore()
        {
            File.Delete(StoreFileName);
        }
    }
}
