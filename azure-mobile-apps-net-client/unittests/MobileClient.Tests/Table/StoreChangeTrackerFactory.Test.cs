// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Eventing;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MobileClient.Tests.Helpers;
using Xunit;

namespace MobileClient.Tests.Table
{
    public class StoreChangeTrackerFactory_Test
    {
        [Fact]
        public void CreateTrackedStore_ReturnsUntrackedProxyForLocalSource_WhenNoNotificationsAreEnabledForSource()
        {
            StoreTrackingOptions trackingOptions = StoreTrackingOptions.AllNotificationsAndChangeDetection & ~StoreTrackingOptions.NotifyLocalOperations;

            AssertUntrackedStoreForSourceWithOptions(StoreOperationSource.Local, trackingOptions);
        }

        [Fact]
        public void CreateTrackedStore_ReturnsUntrackedProxyForServerPullSource_WhenNoNotificationsAreEnabledForSource()
        {
            StoreTrackingOptions trackingOptions = StoreTrackingOptions.AllNotificationsAndChangeDetection & ~(StoreTrackingOptions.NotifyServerPullBatch | StoreTrackingOptions.NotifyServerPullOperations);

            AssertUntrackedStoreForSourceWithOptions(StoreOperationSource.ServerPull, trackingOptions);
        }

        [Fact]
        public void CreateTrackedStore_ReturnsUntrackedProxyForServerPushSource_WhenNoNotificationsAreEnabledForSource()
        {
            StoreTrackingOptions trackingOptions = StoreTrackingOptions.AllNotificationsAndChangeDetection & ~(StoreTrackingOptions.NotifyServerPushBatch | StoreTrackingOptions.NotifyServerPushOperations);

            AssertUntrackedStoreForSourceWithOptions(StoreOperationSource.ServerPush, trackingOptions);
        }

        [Fact]
        public void CreateTrackedStore_ReturnsUntrackedProxyForLocalConflictResolutionSource_WhenNoNotificationsAreEnabledForSource()
        {
            StoreTrackingOptions trackingOptions = StoreTrackingOptions.AllNotificationsAndChangeDetection & ~StoreTrackingOptions.NotifyLocalConflictResolutionOperations;

            AssertUntrackedStoreForSourceWithOptions(StoreOperationSource.LocalConflictResolution, trackingOptions);
        }

        private void AssertUntrackedStoreForSourceWithOptions(StoreOperationSource source, StoreTrackingOptions trackingOptions)
        {
            var store = new MobileServiceLocalStoreMock();
            var trackingContext = new StoreTrackingContext(StoreOperationSource.Local, string.Empty, StoreTrackingOptions.None);
            var eventManager = new MobileServiceEventManager();
            var settings = new MobileServiceSyncSettingsManager(store);

            IMobileServiceLocalStore trackedStore = StoreChangeTrackerFactory.CreateTrackedStore(store, source, trackingOptions, eventManager, settings);

            Assert.NotNull(trackedStore);
            Assert.IsType<LocalStoreProxy>(trackedStore);
        }
    }
}
