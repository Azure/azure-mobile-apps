// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline.Queue;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// A parallel async queue runner that runs the table operations.
    /// </summary>
    internal class QueueHandler
    {
        /// <summary>
        /// The jobs.
        /// </summary>
        private readonly ActionBlock<TableOperation> _jobs;

        /// <summary>
        /// Creates a new <see cref="QueueHandler"/>.
        /// </summary>
        /// <param name="maxThreads">The maximum number of threads to run.</param>
        /// <param name="jobRunner">The job runner method.</param>
        public QueueHandler(int maxThreads, Func<TableOperation, Task> jobRunner)
        {
            var options = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = maxThreads
            };

            _jobs = new ActionBlock<TableOperation>(jobRunner, options);
        }

        /// <summary>
        /// Enqueue a new job.
        /// </summary>
        /// <param name="operation">The operation to be enqueued.</param>
        public void Enqueue(TableOperation operation)
        {
            _jobs.Post(operation);
        }

        /// <summary>
        /// Wait until all jobs have been completed.
        /// </summary>
        public Task WhenComplete()
        {
            _jobs.Complete();
            return _jobs.Completion;
        }

        /// <summary>
        /// The number of items still to be posted.
        /// </summary>
        public int Count { get => _jobs.InputCount; }
    }
}
