// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// An exception that is thrown when a repository produces an error.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "We don't check coverage on purely derived exceptions.")]
    public class RepositoryException : HttpException
    {
        public RepositoryException() : base()
        {
        }

        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new <see cref="RepositoryException"/> with an HTTP status code and optional payload.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to send with the response.</param>
        /// <param name="payload">The payload to send with the response.</param>
        public RepositoryException(int statusCode, object payload = null) : base(statusCode, payload)
        {
        }
    }
}
