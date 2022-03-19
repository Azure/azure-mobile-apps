// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Queue;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Datasync.Client.Offline
{
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
            Errors = new ReadOnlyCollection<TableOperationError>(errors?.ToList() ?? new List<TableOperationError>());
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
