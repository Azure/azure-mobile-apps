// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AzureMobile.Server.Exceptions
{
    /// <summary>
    /// The base exception class from which all other Azure Mobile exceptions are derived.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Don't test code coverage of standard exception forms")]
    public class AzureMobileException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="AzureMobleException"/> without a detail message.
        /// </summary>
        public AzureMobileException() : base()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AzureMobileException"/> with a detail message.
        /// </summary>
        /// <param name="message"></param>
        public AzureMobileException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="AzureMobileException"/> with a detail message and
        /// inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public AzureMobileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
