// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MobileClient.Tests.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests.Table
{
    public class LocalStoreChangeTracker_Test
    {
        [Fact]
        public void Constructor_Throws_WhenTrackingOptionsAreInvalid()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty, StoreTrackingOptions.None);
            var eventManager = new MobileServiceEventManager();
            var settings = new MobileServiceSyncSettingsManager(store);
            Assert.Throws<InvalidOperationException>(() => new LocalStoreChangeTracker(store, trackingContext, eventManager, settings));
        }


        [Fact]
        public void Disposing_CompletesBatch()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.ServerPull, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);

            StoreOperationsBatchCompletedEvent batchEvent = null;
            eventManager.PublishAsyncFunc = e =>
            {
                batchEvent = e as StoreOperationsBatchCompletedEvent;
                return Task.FromResult(0);
            };

            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);
            changeTracker.Dispose();

            Assert.NotNull(batchEvent);
        }

        [Fact]
        public async Task BatchNotification_ReportsOperationCount()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.ServerPull, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);
            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

            EnqueueSimpleObjectResponse(store, "123", "XXX", "789");

            await changeTracker.UpsertAsync("test", new JObject() { { "id", "123" }, { "version", "2" } }, true); // Update
            await changeTracker.UpsertAsync("test", new JObject() { { "id", "456" }, { "version", "2" } }, true); // Insert
            await changeTracker.DeleteAsync("test", "789"); // Delete

            StoreOperationsBatchCompletedEvent batchEvent = null;
            eventManager.PublishAsyncFunc = e =>
            {
                batchEvent = e as StoreOperationsBatchCompletedEvent;
                return Task.FromResult(0);
            };

            changeTracker.Dispose();

            Assert.NotNull(batchEvent);
            Assert.Equal(3, batchEvent.Batch.OperationCount);
            Assert.Equal(1, batchEvent.Batch.GetOperationCountByKind(LocalStoreOperationKind.Update));
            Assert.Equal(1, batchEvent.Batch.GetOperationCountByKind(LocalStoreOperationKind.Insert));
            Assert.Equal(1, batchEvent.Batch.GetOperationCountByKind(LocalStoreOperationKind.Delete));
        }

        [Fact]
        public async Task UpsertAsync_SuppressesUpdateNotifications_WhenServerChangesMatchesLocalRecordVersion()
        {
            var operationSources = new[] { StoreOperationSource.ServerPull, StoreOperationSource.ServerPush };

            await AssertNotificationResultWithMatchingLocalRecordVersion(operationSources, false);
        }

        [Fact]
        public async Task UpsertAsync_DoesNotSuppressUpdateNotifications_WhenLocalChangesMatchesLocalRecordVersion()
        {
            await AssertNotificationResultWithMatchingLocalRecordVersion(new[] { StoreOperationSource.Local }, true);
        }

        [Fact]
        public async Task DeleteAsync_WithTableNameAndRecordIds_SendsNotification()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);
            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

            JObject item = EnqueueSimpleObjectResponse(store);

            StoreOperationCompletedEvent operationEvent = null;
            eventManager.PublishAsyncFunc = t =>
            {
                operationEvent = t as StoreOperationCompletedEvent;
                return Task.FromResult(0);
            };

            await changeTracker.DeleteAsync("test", "123");

            Assert.NotNull(operationEvent);
            Assert.Equal(LocalStoreOperationKind.Delete, operationEvent.Operation.Kind);
            Assert.Equal("123", operationEvent.Operation.RecordId);
            Assert.Equal("test", operationEvent.Operation.TableName);
        }

        [Fact]
        public async Task DeleteAsync_WithQuery_SendsNotification()
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty);
            var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
            var settings = new MobileServiceSyncSettingsManager(store);
            var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

            JObject item = EnqueueSimpleObjectResponse(store);

            StoreOperationCompletedEvent operationEvent = null;
            eventManager.PublishAsyncFunc = t =>
            {
                operationEvent = t as StoreOperationCompletedEvent;
                return Task.FromResult(0);
            };

            MobileServiceTableQueryDescription query = new MobileServiceTableQueryDescription("test");
            query.Filter = new BinaryOperatorNode(BinaryOperatorKind.Equal, new MemberAccessNode(null, MobileServiceSystemColumns.Id), new ConstantNode("123"));

            await changeTracker.DeleteAsync(query);

            Assert.NotNull(operationEvent);
            Assert.Equal(LocalStoreOperationKind.Delete, operationEvent.Operation.Kind);
            Assert.Equal("123", operationEvent.Operation.RecordId);
            Assert.Equal("test", operationEvent.Operation.TableName);
        }

        private async Task AssertNotificationResultWithMatchingLocalRecordVersion(StoreOperationSource[] operationSources, bool shouldNotify)
        {
            foreach (var operationSource in operationSources)
            {
                var store = new MobileServiceLocalStoreMock();
                var trackingContext = new StoreTrackingContext(operationSource, string.Empty);
                var eventManager = new MobileServiceEventManagerMock<IMobileServiceEvent>();
                var settings = new MobileServiceSyncSettingsManager(store);
                var changeTracker = new LocalStoreChangeTracker(store, trackingContext, eventManager, settings);

                JObject item = EnqueueSimpleObjectResponse(store);

                bool notificationSent = false;
                eventManager.PublishAsyncFunc = t =>
                {
                    notificationSent = true;
                    return Task.FromResult(0);
                };

                await changeTracker.UpsertAsync("test", item, true);

                Assert.Equal(notificationSent, shouldNotify);
            }
        }

        private JObject EnqueueSimpleObjectResponse(MobileServiceLocalStoreMock store)
        {
            return EnqueueSimpleObjectResponse(store, "123").First();
        }

        private IEnumerable<JObject> EnqueueSimpleObjectResponse(MobileServiceLocalStoreMock store, params string[] ids)
        {
            var results = new List<JObject>();
            foreach (var id in ids)
            {
                var item = new JObject() { { "id", id }, { "version", "1" } };
                store.ReadResponses.Enqueue(string.Format("[{0}]", item.ToString()));

                results.Add(item);
            }

            return results;
        }
    }
}
