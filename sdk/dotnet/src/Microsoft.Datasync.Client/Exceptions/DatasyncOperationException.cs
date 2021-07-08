// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Exception that provides additional details of an invalid operation specific to a datasync service.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Specialized Exception")]
    public class DatasyncOperationException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="DatasyncOperationException"/> based on the provided request
        /// and response.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <param name="response">The <see cref="HttpResponseMessage"/></param>
        internal DatasyncOperationException(HttpRequestMessage request, HttpResponseMessage response) : base(response?.ReasonPhrase)
        {
            Validate.IsNotNull(request, nameof(request));
            Validate.IsNotNull(response, nameof(response));

            Request = request;
            Response = response;
        }

        /// <summary>
        /// The HTTP request message that generated the error.
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// The HTTP response message that generated the error.
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// The integer status code.
        /// </summary>
        public int StatusCode { get => (int)Response.StatusCode; }
    }
}
