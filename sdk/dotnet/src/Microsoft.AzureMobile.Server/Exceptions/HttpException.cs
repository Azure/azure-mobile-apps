// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AzureMobile.Server.Exceptions
{
    /// <summary>
    /// An exception thrown when the library wishes to return an augmented HTTP
    /// response that isn't standard.  Common cases include entity not found,
    /// preconditions failed, or internal server errors.
    /// </summary>
    public class HttpException : AzureMobileException
    {
        /// <summary>
        /// Creates a new <see cref="HttpException"/> without any detail.
        /// </summary>
        public HttpException() : base()
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpException"/> with a detail message.
        /// </summary>
        /// <param name="message"></param>
        public HttpException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpException"/> with a detail message and inner exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public HttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpException"/> with a status code an optional payload.
        /// </summary>
        /// <param name="statusCode">The status code to send with the response</param>
        /// <param name="payload">The payload to send with the response</param>
        public HttpException(int statusCode, object payload = null) : base()
        {
            StatusCode = statusCode;
            Payload = payload;
        }

        /// <summary>
        /// The HTTP status code that should be sent as part of the response.
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status500InternalServerError;

        /// <summary>
        /// If provided, the content that should be sent as the payload of the response.
        /// </summary>
        public object Payload { get; set; }

        public IActionResult ToActionResult()
            => Payload != null ? new ObjectResult(Payload) { StatusCode = StatusCode } : new StatusCodeResult(StatusCode);
    }

    #region Sub-classes
    /// <summary>
    /// A <see cref="HttpException"/> that returns 304 Not Modified to the client.
    /// </summary>
#pragma warning disable RCS1194 // Implement exception constructors.
    public class NotModifiedException : HttpException
    {
        public NotModifiedException() : base(StatusCodes.Status304NotModified)
        {
        }
    }

    /// <summary>
    /// A <see cref="HttpException"/> that returns 400 Bad Request to the client.
    /// </summary>
    public class BadRequestException : HttpException
    {
        public BadRequestException() : base(StatusCodes.Status400BadRequest)
        {
        }
    }

    /// <summary>
    /// A <see cref="HttpException"/> that returns 404 Not Found to the client.
    /// </summary>
    public class NotFoundException : HttpException
    {
        public NotFoundException() : base(StatusCodes.Status404NotFound)
        {
        }
    }

    /// <summary>
    /// A <see cref="HttpException"/> that returns 409 Conflict to the client.
    /// </summary>
    public class ConflictException : HttpException
    {
        public ConflictException(object entity) : base(StatusCodes.Status409Conflict, entity)
        {
        }
    }

    /// <summary>
    /// A <see cref="HttpException"/> that returns 412 Precondition Failed to the client.
    /// </summary>
    public class PreconditionFailedException : HttpException
    {
        public PreconditionFailedException(object entity) : base(StatusCodes.Status412PreconditionFailed, entity)
        {
        }
    }
#pragma warning restore RCS1194 // Implement exception constructors.
    #endregion
}
