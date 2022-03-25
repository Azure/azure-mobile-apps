// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Offline
{
    /// <summary>
    /// Provides additional details of a failed offline store operation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OfflineStoreException : Exception
    {
        public OfflineStoreException() : base()
        {
        }

        public OfflineStoreException(string message) : base(message)
        {
        }

        public OfflineStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
