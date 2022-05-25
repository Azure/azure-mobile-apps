// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Eventing;
using System;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal static class StoreChangeTrackerFactory
    {
        internal static IMobileServiceLocalStore CreateTrackedStore(IMobileServiceLocalStore targetStore, StoreOperationSource source, StoreTrackingOptions trackingOptions, 
            IMobileServiceEventManager eventManager, MobileServiceSyncSettingsManager settings)
        {
            if (IsTrackingEnabled(trackingOptions, source))
            {
                Guid batchId = source == StoreOperationSource.Local ? Guid.Empty : Guid.NewGuid();

                return new LocalStoreChangeTracker(targetStore, new StoreTrackingContext(source, batchId.ToString(), trackingOptions), eventManager, settings);
            }
            else
            {
                return new LocalStoreProxy(targetStore);               
            }
        }

        private static bool IsTrackingEnabled(StoreTrackingOptions trackingOptions, StoreOperationSource source)
        {
            switch (source)
            {
                case StoreOperationSource.Local:
                case StoreOperationSource.LocalPurge:
                    return trackingOptions.HasFlag(StoreTrackingOptions.NotifyLocalOperations);
                case StoreOperationSource.LocalConflictResolution:
                    return trackingOptions.HasFlag(StoreTrackingOptions.NotifyLocalConflictResolutionOperations);
                case StoreOperationSource.ServerPull:
                    return (trackingOptions & (StoreTrackingOptions.NotifyServerPullBatch | StoreTrackingOptions.NotifyServerPullOperations)) != StoreTrackingOptions.None;
                case StoreOperationSource.ServerPush:
                    return (trackingOptions & (StoreTrackingOptions.NotifyServerPushBatch | StoreTrackingOptions.NotifyServerPushOperations)) != StoreTrackingOptions.None;
                default:
                    throw new InvalidOperationException("Unknown store operation source.");
            }
        }
    }
}
