// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices.Threading
{
    internal struct DisposeAction: IDisposable
    {
        bool isDisposed;
        private readonly Action action;

        public DisposeAction(Action action)
        {
            this.isDisposed = false;
            this.action = action;
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.action();
        }
    }
}
