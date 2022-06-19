// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// The options used to configure a push operation.
    /// </summary>
    public class PushOptions
    {
        private int _parallelOperations = 0;

        /// <summary>
        /// The number of parallel operations that can occur when running the 
        /// push operation queue.  By default, we do this serially (i.e. one
        /// thread).  If zero, use the global version.
        /// </summary>
        public int ParallelOperations
        {
            get => _parallelOperations;
            set
            {
                if (value < 0 || value > DatasyncClientOptions.MAX_PARALLEL_OPERATIONS)
                {
                    throw new ArgumentOutOfRangeException(nameof(ParallelOperations));
                }
                _parallelOperations = value;
            }
        }
    }
}
