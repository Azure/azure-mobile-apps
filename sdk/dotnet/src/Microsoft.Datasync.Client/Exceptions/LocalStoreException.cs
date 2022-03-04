// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Datasync.Client.Offline
{
    [Serializable]
    internal class LocalStoreException : Exception
    {
        public LocalStoreException()
        {
        }

        public LocalStoreException(string message) : base(message)
        {
        }

        public LocalStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LocalStoreException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}