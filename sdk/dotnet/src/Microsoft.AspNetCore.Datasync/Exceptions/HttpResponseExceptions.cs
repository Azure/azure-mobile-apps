// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// A <see cref="HttpException"/> that returns 304 Not Modified to the client.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Prevent accidentally using default constructors")]
    public class NotModifiedException : HttpException
    {
        public NotModifiedException() : base(StatusCodes.Status304NotModified)
        {
        }
    }

    /// <summary>
    /// A <see cref="HttpException"/> that returns 400 Bad Request to the client.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Prevent accidentally using default constructors")]
    public class BadRequestException : HttpException
    {
        public BadRequestException() : base(StatusCodes.Status400BadRequest)
        {
        }

        public BadRequestException(string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = StatusCodes.Status400BadRequest;
            Payload = innerException;
        }
    }

    /// <summary>
    /// A <see cref="HttpException"/> that returns 404 Not Found to the client.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Prevent accidentally using default constructors")]
    public class NotFoundException : HttpException
    {
        public NotFoundException() : base(StatusCodes.Status404NotFound)
        {
        }
    }

    /// <summary>
    /// A <see cref="HttpException"/> that returns 409 Conflict to the client.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Prevent accidentally using default constructors")]
    public class ConflictException : HttpException
    {
        public ConflictException(object entity) : base(StatusCodes.Status409Conflict, entity)
        {
        }
    }


    /// <summary>
    /// A <see cref="HttpException"/> that returns 412 Precondition Failed to the client.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Prevent accidentally using default constructors")]
    public class PreconditionFailedException : HttpException
    {
        public PreconditionFailedException(object entity) : base(StatusCodes.Status412PreconditionFailed, entity)
        {
        }
    }
}
