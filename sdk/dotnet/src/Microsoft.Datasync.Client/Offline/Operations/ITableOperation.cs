// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Offline.Operations
{
    /// <summary>
    /// Definition of a table operation against a remote table.
    /// </summary>
    public interface ITableOperation
    {
        /// <summary>
        /// The kind of operation
        /// </summary>
        TableOperationKind Kind { get; }

        /// <summary>
        /// The state of the operation
        /// </summary>
        TableOperationState State { get; }

        /// <summary>
        /// The table that the operation will be executed against.
        /// </summary>
        IRemoteTable Table { get; }

        /// <summary>
        /// The item associated with the operation.
        /// </summary>
        JObject Item { get; set; }

        /// <summary>
        /// Abort the parent push operation.
        /// </summary>
        void AbortPush();

        /// <summary>
        /// Executes the operation against remote table.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the operation result when complete.</returns>
        Task<JObject> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
