// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

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
        internal static void IsNotNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Contract - parameter is not allowed to be null or empty
        /// </summary>
        internal static void IsNotNullOrEmpty(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (string.IsNullOrEmpty(param))
            {
                throw new ArgumentException($"'{paramName}' is empty", paramName);
            }
        }

        /// <summary>
        /// Contract - paramter is not allowed to be null or empty
        /// </summary>
        internal static void IsNotNullOrEmpty<T>(IEnumerable<T> param, string paramName)
        {
            IsNotNull(param, paramName);
            if (!param.Any())
            {
                throw new ArgumentException($"'{paramName}' is empty", paramName);
            }
        }

        /// <summary>
        /// Contract - parameter is not allowed to be null or whitespace
        /// </summary>
        internal static void IsNotNullOrWhitespace(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException($"'{paramName}' is whitespace", paramName);
            }
        }

        /// <summary>
        /// Contract - parameter is a valid endpoint (HTTP with loopbackonly or HTTPS, absolute URI)
        /// </summary>
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

        /// <summary>
        /// Contract - paramter is a relative URI
        /// </summary>
        internal static void IsRelativeUri(string relativeUri, string paramName)
        {
            IsNotNull(relativeUri, paramName);

            // Easiest way to validate is to construct the URI and check it.
            string absoluteUri = $"http://localhost/{relativeUri.TrimStart('/')}";
            if (!Uri.IsWellFormedUriString(absoluteUri, UriKind.Absolute))
            {
                throw new ArgumentException($"'{relativeUri}' is not a valid relative URI", paramName);
            }
        }
    }
}
