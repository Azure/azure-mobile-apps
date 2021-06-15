// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// An exception that is thrown when a request to the datasync service fails
    /// due to a protocol level error condition.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Specialized Exception does not require additional constructors")]
    public class RequestFailedException : DatasyncFrameworkException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestFailedException"/> class.
        /// </summary>
        public RequestFailedException(HttpStatusCode statusCode) : base()
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// The <see cref="HttpStatusCode"/> error condition.
        /// </summary>
        public HttpStatusCode StatusCode { get; }
    }

    /// <summary>
    /// An exception that is thrown when the response from the server is 304 Not Modified.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Specialized Exception does not require additional constructors")]
    public class NotModifiedException : RequestFailedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotModifiedException"/> class.
        /// </summary>
        public NotModifiedException() : base(HttpStatusCode.NotModified)
        {
        }
    }

    /// <summary>
    /// An exception that is thrown when the response from the server is 404 Not Found
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Specialized Exception does not require additional constructors")]
    public class NotFoundException : RequestFailedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException() : base(HttpStatusCode.NotFound)
        {
        }
    }
}
