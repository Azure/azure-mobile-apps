// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Methods to validate that arguments meet contracts.
    /// </summary>
    internal static class Validate
    {
        /// <summary>
        /// Contract - parameter is not allowed to be null
        /// </summary>
        /// <param name="param">Parameter value</param>
        /// <param name="paramName">Parameter name</param>
        internal static void IsNotNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Contract - parameter is a valid endpoint (HTTP with loopbackonly or HTTPS, absolute URI)
        /// </summary>
        /// <param name="endpoint">Parameter value</param>
        /// <param name="paramName">Parameter name</param>
        internal static void IsValidEndpoint(Uri endpoint, string paramName)
        {
            IsNotNull(endpoint, paramName);
            if (!endpoint.IsAbsoluteUri)
            {
                throw new UriFormatException($"'{paramName}' must use an absolute URI");
            }
            if (endpoint.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!endpoint.IsLoopback)
                {
                    throw new UriFormatException($"'{paramName}' must use secure (https) endpoint when not loopback");
                }
            }
            else if (!endpoint.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UriFormatException($"'{paramName}' must use HTTP protocol");
            }
        }
    }
}
