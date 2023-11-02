// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.", Justification = "Specialist Exception")]
[ExcludeFromCodeCoverage]
public class HttpException : DatasyncFrameworkException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to use.</param>
    public HttpException(int statusCode) : base()
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpException"/> class with a specified error message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to use.</param>
    /// <param name="message">The message that describes the error.</param>
    public HttpException(int statusCode, string? message) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to use.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that was the cause of the error.</param>
    public HttpException(int statusCode, string? message, Exception? innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// The HTTP status code for the exception.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// If set, the payload for the body of the HTTP exception.
    /// </summary>
    public object? Payload { get; set; }
}
