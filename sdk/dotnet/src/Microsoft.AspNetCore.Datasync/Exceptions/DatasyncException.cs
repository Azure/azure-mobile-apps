// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// The base exception class from which other Datasync exception are derived.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "We don't check coverage on purely derived exceptions.")]
    public class DatasyncException : Exception
    {
        public DatasyncException() : base()
        {
        }

        public DatasyncException(string message) : base(message)
        {
        }

        public DatasyncException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
