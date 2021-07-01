// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Extensions to the standard library
    /// </summary>
    internal static class StdLibExtensions
    {
        /// <summary>
        /// Normalize an endpoint by removing any query and fragment, then ensuring that the
        /// path has a trailing slash.
        /// </summary>
        /// <param name="endpoint">The endpoint to normalize.</param>
        /// <returns>The normalized endpoint.</returns>
        internal static Uri NormalizeEndpoint(this Uri endpoint)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));

            var builder = new UriBuilder(endpoint) { Query = string.Empty, Fragment = string.Empty };
            builder.Path = builder.Path.TrimEnd('/') + "/";
            return builder.Uri;
        }
    }
}
