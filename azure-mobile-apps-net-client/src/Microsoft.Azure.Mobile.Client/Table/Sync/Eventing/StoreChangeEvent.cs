// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices.Eventing;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Represents an event raised as a result of a change against the local store.
    /// </summary>
    public abstract class StoreChangeEvent : IMobileServiceEvent
    {
        /// <summary>
        /// Gets the event name.
        /// </summary>
        public abstract string Name { get; }
    }
}
