// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// An exception thrown when the library wishes to return an augmented HTTP
    /// response that isn't standard.  Common cases include "Entity not found",
    /// "Precondition failed", or "Internal server error".
    /// </summary>
    public class HttpException : DatasyncException
    {
        [ExcludeFromCodeCoverage(Justification = "Standard constructor with no method body")]
        public HttpException() : base()
        {
        }

        [ExcludeFromCodeCoverage(Justification = "Standard constructor with no method body")]
        public HttpException(string message) : base(message)
        {
        }

        [ExcludeFromCodeCoverage(Justification = "Standard constructor with no method body")]
        public HttpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpException"/> with a status code and optional payload.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to send with the response.</param>
        /// <param name="payload">The payload to send with the response.</param>
        public HttpException(int statusCode, object payload = null) : base()
        {
            StatusCode = statusCode;
            Payload = payload;
        }

        /// <summary>
        /// The HTTP status code to send with the response.
        /// </summary>
        public int StatusCode { get; set; } = StatusCodes.Status500InternalServerError;

        /// <summary>
        /// If provided, the content that should be sent as the payload of the response.
        /// </summary>
        public object Payload { get; set; }

        /// <summary>
        /// Converts the exception into an <see cref="IActionResult"/> suitable for sending
        /// as a HTTP response message.
        /// </summary>
        /// <returns>The <see cref="IActionResult"/> the represents this error.</returns>
        public IActionResult ToActionResult()
            => Payload != null ? new ObjectResult(Payload) { StatusCode = StatusCode } : new StatusCodeResult(StatusCode);
    }
}
