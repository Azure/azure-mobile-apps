// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// A mobile service event that is published when an operations batch against the local store is completed.
    /// </summary>
    public sealed class StoreOperationsBatchCompletedEvent : StoreChangeEvent
    {
        /// <summary>
        /// The store operation batch completed event name.
        /// </summary>
        public const string EventName = "MobileServices.StoreOperationBatchCompleted";

        /// <summary>
        /// Create a new Batch completed event.
        /// </summary>
        /// <param name="batch">The batch that was completed.</param>
        public StoreOperationsBatchCompletedEvent(StoreOperationsBatch batch)
        {
            Arguments.IsNotNull(batch, nameof(batch));
            
            Batch = batch;
        }

        /// <summary>
        /// Gets the event name.
        /// </summary>
        public override string Name => EventName;

        /// <summary>
        /// The operations batch instance.
        /// </summary>
        public StoreOperationsBatch Batch { get; private set; }
    }
}
