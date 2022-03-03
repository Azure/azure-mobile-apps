// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Operations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The state in which push completed.
    /// </summary>
    public enum PushStatus
    {
        /// <summary>
        /// All operations were completed, possibly with errors.
        /// </summary>
        Complete = 0,

        /// <summary>
        /// The push was aborted due to a network error.
        /// </summary>
        CancelledByNetworkError,

        /// <summary>
        /// The push was aborted due to an authentication error.
        /// </summary>
        CancelledByAuthenticationError,

        /// <summary>
        /// The push was aborted due to an error from the offline store.
        /// </summary>
        CancelledByOfflineStoreError,

        /// <summary>
        /// The push was aborted because of a <see cref="CancellationToken"/>
        /// </summary>
        CancelledByToken,

        /// <summary>
        /// The push was aborted by a <see cref="TableOperation"/>.
        /// </summary>
        CancelledByOperation,

        /// <summary>
        /// The push failed due to an internal error.
        /// </summary>
        InternalError = Int32.MaxValue
    }

    /// <summary>
    /// The result of a push completion event.
    /// </summary>
    public class PushCompletionResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PushCompletionResult"/>.
        /// </summary>
        /// <param name="errors">Collection of errors that occured on executing operation on remote table.</param>
        /// <param name="status">The state in which push completed.</param>
        public PushCompletionResult(IEnumerable<TableOperationError> errors, PushStatus status)
        {
            Errors = new ReadOnlyCollection<TableOperationError>(errors as IList<TableOperationError> ?? new List<TableOperationError>());
            Status = status;
        }

        /// <summary>
        /// The Errors caused by executing the operation against a remote table.
        /// </summary>
        public IReadOnlyCollection<TableOperationError> Errors { get; }

        /// <summary>
        /// The stat in which the push completed.
        /// </summary>
        public PushStatus Status { get; }
    }
}
