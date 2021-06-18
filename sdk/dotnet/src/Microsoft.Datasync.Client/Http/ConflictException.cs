// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// An exception that indicates the submitted change was in conflict with a prior change.
    /// </summary>
    /// <typeparam name="T">The type of the entity</typeparam>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "HttpResponse<T> is a HttpResponse")]
    public class ConflictException<T> : RequestFailedException where T : notnull
    {
        /// <summary>
        /// Creates a new <see cref="ConflictException{T}"/> based on a response.
        /// </summary>
        /// <param name="response">The source response.</param>
        public ConflictException(HttpResponse<T> response) : base()
        {
            Validate.IsNotNull(response, nameof(response));
            Response = response;
            ServerItem = response.Value;
        }

        /// <summary>
        /// The item on the server side causing the conflict.
        /// </summary>
        public T? ServerItem { get; }
    }
}
