﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// An exception to provide additional details of an invalid operation
    /// specific to a Datasync service.
    /// </summary>
    public class DatasyncInvalidOperationException : InvalidOperationException
    {
        /// <summary>
        /// Creates a new <see cref="DatasyncInvalidOperationException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="request">The originating service request.</param>
        /// <param name="response">The returned service response.</param>
        public DatasyncInvalidOperationException(string message, HttpRequestMessage request, HttpResponseMessage response)
            : this(message, request, response, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncInvalidOperationException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="request">The originating service request.</param>
        /// <param name="response">The returned service response.</param>
        public DatasyncInvalidOperationException(string message, Exception innerException, HttpRequestMessage request, HttpResponseMessage response)
            : base(message, innerException)
        {
            Request = request;
            Response = response;
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncInvalidOperationException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="request">The originating service request.</param>
        /// <param name="response">The returned service response.</param>
        /// <param name="value">The server response deserialized as a <see cref="JObject"/>.</param>
        public DatasyncInvalidOperationException(string message, HttpRequestMessage request, HttpResponseMessage response, JObject value)
            : base(message)
        {
            Request = request;
            Response = response;
            Value = value;
        }

        /// <summary>
        /// The originating service request.
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// The returned service response.
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// The server response deserialized as a JObject.
        /// </summary>
        public JObject Value { get; }
    }
}
