// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A base exception that is thrown when a HTTP request returns an error condition.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Special use exception")]
    public class RequestFailedException : Exception
    {
        /// <summary>
        /// Used for setting up other exceptions and for unit testing
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected RequestFailedException() : base()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Creates a new <see cref="RequestFailedException"/> based on a <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="response">The HTTP response message</param>
        public RequestFailedException(HttpResponse response) : base()
        {
            Validate.IsNotNull(response, nameof(response));
            Response = response;
        }

        /// <summary>
        /// The HTTP response message that caused the exception.
        /// </summary>
        public HttpResponse Response { get; protected set; }

        /// <summary>
        /// The status code for the HTTP response that caused the exception.
        /// </summary>
        public int StatusCode { get => Response.StatusCode; }
    }

    /// <summary>
    /// An exception thrown in response to a 304 Not Modified exception.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Special use exception")]
    public class NotModifiedException : RequestFailedException
    {
        /// <summary>
        /// Creates a new <see cref="NotModifiedException"/>.
        /// </summary>
        /// <param name="response">The HTTP response message that causes the exception.</param>
        public NotModifiedException(HttpResponse response) : base()
        {
            Validate.IsNotNull(response, nameof(response));
            Response = response;
        }
    }
}
