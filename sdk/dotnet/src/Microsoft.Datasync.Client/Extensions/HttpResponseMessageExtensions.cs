// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;

namespace Microsoft.Datasync.Client.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="HttpResponseMessage"/> class.
    /// </summary>
    internal static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Returns true if the response is a conflict.
        /// </summary>
        /// <param name="message">The <see cref="HttpResponseMessage"/></param>
        internal static bool IsConflictStatusCode(this HttpResponseMessage message)
            => message.StatusCode == HttpStatusCode.Conflict || message.StatusCode == HttpStatusCode.PreconditionFailed;
    }
}
