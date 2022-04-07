// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// An <see cref="IDisposable"/> that runs an action on disposing.
    /// </summary>
    /// <remarks>
    /// This is most often used to release an asynchronous lock when disposed.
    /// </remarks>
    internal struct DisposeAction : IDisposable
    {
        bool isDisposed;
        private readonly Action action;

        public DisposeAction(Action action)
        {
            isDisposed = false;
            this.action = action;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            action();
        }
    }
}
