// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Exception that provides additional details of an invalid operation specific to a datasync service.
    /// </summary>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Specialized Exception")]
    public class DatasyncOperationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatasyncOperationException"/> class.
        /// </summary>
        /// <param name="request">The request message</param>
        /// <param name="response">The response message</param>
        public DatasyncOperationException(HttpRequestMessage request, HttpResponseMessage response)
            : base(response?.ReasonPhrase)
        {
            Validate.IsNotNull(request, nameof(request));
            Validate.IsNotNull(response, nameof(response));

            Request = request;
            Response = response;
        }

        /// <summary>
        /// The originating request message
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// The resulting response message
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// If true, this exception was thrown due to a service conflict.
        /// </summary>
        public bool IsConflictStatusCode { get => Response?.StatusCode == HttpStatusCode.Conflict || Response?.StatusCode == HttpStatusCode.PreconditionFailed;  }
    }
}
