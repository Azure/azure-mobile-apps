// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Internal
{
    /// <summary>
    /// A set of variable validation methods.
    /// </summary>
    internal static class Validate
    {
        /// <summary>
        /// Validates that the parameter is not null.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        internal static void IsNotNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Validates that the parameter is not null or empty.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        internal static void IsNotNullOrEmpty(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (string.IsNullOrEmpty(param))
            {
                throw new ArgumentException($"{paramName} cannot be empty", paramName);
            }
        }

        /// <summary>
        /// Validates that the parameter is not null or empty.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        internal static void IsNotNullOrEmpty(byte[] param, string paramName)
        {
            IsNotNull(param, paramName);
            if (param.Length == 0)
            {
                throw new ArgumentException($"{paramName} cannot be empty", paramName);
            }
        }

        /// <summary>
        /// Validates that the parameter is not null or whitespace.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        internal static void IsNotNullOrWhitespace(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException($"{paramName} cannot be whitespace", paramName);
            }
        }

        /// <summary>
        /// Checks that the provided string is a valid Precondition header name.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        internal static void IsPreconditionHeader(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (!param.Equals("If-Match", StringComparison.InvariantCultureIgnoreCase) &&
                !param.Equals("If-None-Match", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"{paramName} must be a valid RFC7232 precondition header", paramName);
            }
        }

        /// <summary>
        /// Checks that the provided Uri is a valid endpoint.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="paramName"></param>
        internal static void IsValidEndpoint(Uri param, string paramName)
        {
            IsNotNull(param, paramName);
            if (!param.IsAbsoluteUri)
            {
                throw new UriFormatException($"{paramName} is not an absolute Uri");
            }
            if (param.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && !param.IsLoopback)
            {
                throw new UriFormatException($"{paramName} is insecure and not localhost");
            }
            if (param.Scheme != Uri.UriSchemeHttp && param.Scheme != Uri.UriSchemeHttps)
            {
                throw new UriFormatException($"{paramName} must use http");
            }
        }
    }
}
