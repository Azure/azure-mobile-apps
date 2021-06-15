// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Exceptions
{
    /// <summary>
    /// This is the base class for all exceptions thrown by the
    /// Datasync Framework.  Do not throw this yourself.  Use a
    /// more specific exception.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DatasyncFrameworkException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasyncFrameworkException"/> class.
        /// </summary>
        public DatasyncFrameworkException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasyncFrameworkException"/> class with
        /// the specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DatasyncFrameworkException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatasyncFrameworkException"/> class with the
        /// specified error message and a reference to the inner exception that is the cause of this
        /// exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DatasyncFrameworkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
