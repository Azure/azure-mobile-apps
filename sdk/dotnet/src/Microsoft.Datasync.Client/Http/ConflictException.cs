// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// An exception that is thrown when there is a conflict returned
    /// </summary>
    /// <typeparam name="T">The type of entity in conflict</typeparam>
    [SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "Specialized Exception does not require additional constructors")]
    public class ConflictException<T> : RequestFailedException
    {
        /// <summary>
        /// Creates a new <see cref="ConflictException{T}"/> based on a response.
        /// </summary>
        /// <param name="response">The response from the service.</param>
        public ConflictException(HttpResponse<T> response) : base(response.StatusCode)
        {
            Response = response;
        }

        /// <summary>
        /// The stored response from the service.
        /// </summary>
        public HttpResponse<T> Response { get; }

        /// <summary>
        /// The item causing the conflict, as stored on the service.
        /// </summary>
        public T? ServerItem { get => Response.Value; }
    }
}
