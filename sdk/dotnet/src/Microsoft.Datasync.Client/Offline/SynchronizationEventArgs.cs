// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The list of synchronization events that we support.
    /// </summary>
    public enum SynchronizationEventType
    {
        PushStarted,
        ItemWillBePushed,
        ItemWasPushed,
        PushFinished,
        PullStarted,
        ItemWillBeStored,
        ItemWasStored,
        PullFinished
    }

    /// <summary>
    /// The event arguments sent when a synchronization event occurs.
    /// </summary>
    public class SynchronizationEventArgs
    {
        /// <summary>
        /// The type of event.
        /// </summary>
        public SynchronizationEventType EventType { get; internal set; }

        /// <summary>
        /// When an item is indicated, the ID of the item that was processed.
        /// </summary>
        public string ItemId { get; internal set; }

        /// <summary>
        /// When pulling records, the number of items that have been processed.
        /// </summary>
        public long ItemsProcessed { get; internal set; } = -1;

        /// <summary>
        /// The number of items remaining in the queue.
        /// </summary>
        public long QueueLength { get; internal set; }

        /// <summary>
        /// The name of the table containing the item (if appropriate).
        /// </summary>
        public string TableName { get; internal set; }

        /// <summary>
        /// In the case of a result, whether the push was successful or not.
        /// </summary>
        public bool IsSuccessful { get; internal set; }
    }
}
