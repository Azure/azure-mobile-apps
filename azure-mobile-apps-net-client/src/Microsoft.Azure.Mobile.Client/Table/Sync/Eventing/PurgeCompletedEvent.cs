// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// A mobile service event that is published when a purge operation against the local store is completed.
    /// </summary>
    public sealed class PurgeCompletedEvent : StoreChangeEvent
    {
        /// <summary>
        /// The purge completed event name.
        /// </summary>
        public const string EventName = "MobileServices.PurgeCompleted";

        /// <summary>
        /// Creates a new PurgeCompleted event
        /// </summary>
        /// <param name="tableName">The name of the table that was purged</param>
        public PurgeCompletedEvent(string tableName)
        {
            Arguments.IsNotNull(tableName, nameof(tableName));

            TableName = tableName;
        }

        /// <summary>
        /// Gets the event name.
        /// </summary>
        public override string Name => EventName;

        /// <summary>
        /// Gets the name of the table that was the target of the purge operation.
        /// </summary>
        public string TableName { get; private set; }
    }
}
