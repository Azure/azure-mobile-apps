// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AzureMobile.Server.Authentication
{
    /// <summary>
    /// An exception thrown when the App Service authentication token is invalid.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AppServiceTokenValidationException : Exception
    {
        public AppServiceTokenValidationException() : base()
        {
        }

        public AppServiceTokenValidationException(string message) : base(message)
        {
        }

        public AppServiceTokenValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
