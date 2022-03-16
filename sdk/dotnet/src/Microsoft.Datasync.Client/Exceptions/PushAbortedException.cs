// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Exception that is thrown from a table operation when the push is aborted.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
