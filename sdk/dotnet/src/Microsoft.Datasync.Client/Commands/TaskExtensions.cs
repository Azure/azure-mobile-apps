// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Commands
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Wraps task execution in a provided error handler.  When the error handler fires, it sends the
        /// excepton to the error handler.
        /// </summary>
        /// <param name="task">The task to execute</param>
        /// <param name="handler">The error handler</param>
        public static async void FireAndForgetSafeAsync(this Task task, IAsyncExceptionHandler handler = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                handler?.OnAsyncException(ex);
            }
        }
    }
}
