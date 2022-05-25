// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Eventing
{
    /// <summary>
    /// Represents a mobile service event.
    /// </summary>
    public interface IMobileServiceEvent
    {
        /// <summary>
        /// Gets the event name.
        /// </summary>
        string Name { get; }
    }
}
