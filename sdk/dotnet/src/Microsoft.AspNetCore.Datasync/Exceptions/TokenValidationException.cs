// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// An exception thrown when the Azure App Service authentication token is invalid.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "We don't check coverage on purely derived exceptions.")]
    public class TokenValidationException : Exception
    {
        public TokenValidationException() : base()
        {
        }

        public TokenValidationException(string message) : base(message)
        {
        }

        public TokenValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
