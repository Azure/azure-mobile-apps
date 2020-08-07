// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Zumo.Server.Exceptions
{
    /// <summary>
    /// Exception thrown when an entity is expected to exist, but doesn't; for example, during a Delete
    /// or Replace operation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EntityDoesNotExistException : Exception
    {
        public EntityDoesNotExistException()
        {
        }

        public EntityDoesNotExistException(string message) : base(message)
        {
        }

        public EntityDoesNotExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EntityDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
