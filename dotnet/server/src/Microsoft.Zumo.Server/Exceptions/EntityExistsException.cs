// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Zumo.Server.Exceptions
{
    /// <summary>
    /// Exception that is thrown when an entity needs to not exist, but it exists.  This is
    /// generally thrown on a <see cref="ITableRepository{TEntity}.CreateAsync(TEntity, System.Threading.CancellationToken)"/>
    /// call.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntityExistsException : Exception
    {
        public EntityExistsException()
        {
        }

        public EntityExistsException(string message) : base(message)
        {
        }

        public EntityExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
