// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A base exception that is thrown when a HTTP request returns an error condition.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Special use exception")]
    public class RequestFailedException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="RequestFailedException"/> based on a <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="response">The HTTP response message</param>
        public RequestFailedException(HttpResponseMessage response) : base(response.ReasonPhrase)
        {
            Response = response;
        }

        /// <summary>
        /// The HTTP response message that caused the exception.
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// The status code for the HTTP response that caused the exception.
        /// </summary>
        public HttpStatusCode StatusCode { get => Response.StatusCode; }
    }

    /// <summary>
    /// An exception thrown in response to a 304 Not Modified exception.
    /// </summary>
    public class NotModifiedException : RequestFailedException
    {
        /// <summary>
        /// Creates a new <see cref="NotModifiedException"/>.
        /// </summary>
        /// <param name="response">The HTTP response message that causes the exception.</param>
        public NotModifiedException(HttpResponseMessage response) : base(response)
        {
        }
    }
}
