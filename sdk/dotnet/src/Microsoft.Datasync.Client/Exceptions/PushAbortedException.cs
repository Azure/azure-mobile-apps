// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Exceptions
{
    /// <summary>
    /// Exception that is thrown from a table operation when the push is aborted.
    /// </summary>
    internal class PushAbortedException : Exception
    {
        public PushAbortedException() : base()
        {
        }

        public PushAbortedException(string message) : base(message)
        {
        }

        public PushAbortedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
