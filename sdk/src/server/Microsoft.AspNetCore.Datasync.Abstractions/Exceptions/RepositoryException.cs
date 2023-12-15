// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// An exception to indicate that an error occurred within the repository implementation.
/// </summary>
[ExcludeFromCodeCoverage]
public class RepositoryException : DatasyncFrameworkException
{
    public RepositoryException()
    {
    }

    public RepositoryException(string? message) : base(message)
    {
    }

    public RepositoryException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
