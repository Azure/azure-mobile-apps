// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    /// <summary>
    /// Represents a series of operations that happened within the context of a single action such as a
    /// server push or pull.
    /// </summary>
    public sealed class StoreOperationsBatch
    {
        private readonly Dictionary<LocalStoreOperationKind, int> operationsCountByType;
        private readonly SemaphoreSlim operationsCountSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// Create a new batch of store operations.
        /// </summary>
        /// <param name="batchId">A unique ID to identify the batch</param>
        /// <param name="source">The source of the batch</param>
        public StoreOperationsBatch(string batchId, StoreOperationSource source)
        {
            this.BatchId = batchId;
            this.Source = source;
            this.operationsCountByType = new Dictionary<LocalStoreOperationKind, int>();
        }

        /// <summary>
        /// The ID of the batch this operation belongs to.
        /// </summary>
        public string BatchId { get; private set; }

        /// <summary>
        /// Describes the source this operation was triggered from.
        /// </summary>
        public StoreOperationSource Source { get; private set; }

        /// <summary>
        /// The number of operations executed within this batch.
        /// </summary>
        public int OperationCount => operationsCountByType.Sum(kvp => kvp.Value);

        /// <summary>
        /// Gets the number of operations matching the provided operation kind executed within this batch.
        /// </summary>
        /// <param name="operationKind">The kind of operation.</param>
        /// <returns>The number of operations matching the provided count.</returns>
        public int GetOperationCountByKind(LocalStoreOperationKind operationKind)
            => operationsCountByType.ContainsKey(operationKind) ? operationsCountByType[operationKind] : 0;

        internal async Task IncrementOperationCount(LocalStoreOperationKind operationKind)
        {
            try
            {
                await operationsCountSemaphore.WaitAsync();

                if (!operationsCountByType.ContainsKey(operationKind))
                {
                    operationsCountByType.Add(operationKind, 1);
                }
                else
                {
                    operationsCountByType[operationKind]++;
                }
            }
            finally
            {
                operationsCountSemaphore.Release();
            }
        }
    }
}
