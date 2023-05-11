// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Methods required to validate that method arguments meet
    /// the contract requirements.
    /// </summary>
    public static class Arguments
    {
        /// <summary>
        /// The regular expression to match for a valid table name.
        /// </summary>
        /// <remarks>
        /// This has to meet multiple standards.  The first is that it must be valid
        /// as a path segment according to RFC 2396.  It must also be a valid table
        /// name for any table in the offline store.  Since these vary wildly, we use
        /// a case of "least common denominator".
        /// </remarks>
        private static readonly Regex validTableNameRegex = new("^[a-z][a-z0-9_]{0,63}$");

        /// <summary>
        /// The regular expression to match for a valid item ID.
        /// </summary>
        /// <remarks>
        /// This has to meet multiple standards.  The first is that it must be valid
        /// as a path segment according to RFC 2396.  It must also be suitable for storing
        /// a GUID, and for storing in a database string.
        /// </remarks>
        private static readonly Regex validIdRegex = new("^[a-zA-Z0-9][a-zA-Z0-9_.|:-]{0,126}$");

        /// <summary>
        /// Returns if the parameter is not null.
        /// </summary>
        /// <param name="param">The parameter to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">if the parameter is <c>null</c>.</exception>
        public static void IsNotNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Returns if the parameter is not null or whitespace.
        /// </summary>
        /// <param name="param">The parameter to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">if the parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">if the parameter comprises of only whitespace.</exception>
        public static void IsNotNullOrWhitespace(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException($"'{paramName}' is whitespace", paramName);
            }
        }

        /// <summary>
        /// Returns the parameter if it is positive.
        /// </summary>
        /// <param name="param">The parameter to validate.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The parameter, if valid.</returns>
        /// <exception cref="ArgumentException">if the parameter is invalid.</exception>
        public static int IsPositiveInteger(int param, string paramName)
        {
            if (param <= 0)
            {
                throw new ArgumentException($"Parameter '{paramName}' must be a positive integer.", paramName);
            }
            return param;
        }

        /// <summary>
        /// Returns if the parameter is a valid endpoint for a Datasync service.
        /// </summary>
        /// <remarks>
        /// A valid endpoint is a HTTP endpoint (when using a loopback address) or
        /// an absolute HTTP URI.
        /// </remarks>
        /// <param name="endpoint">The endpoint to validate.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">if the endpoint is null.</exception>
        /// <exception cref="UriFormatException">if the endpoint is not valid.</exception>
        public static void IsValidEndpoint(Uri endpoint, string paramName)
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
        /// Returns if the parameter can be used as an ID.
        /// </summary>
        /// <param name="param">The parameter to validate.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">if the parameter is null.</exception>
        /// <exception cref="ArgumentException">if the parameter cannot be used as an ID</exception>
        public static void IsValidId(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (!validIdRegex.IsMatch(param))
            {
                throw new ArgumentException($"'{param}' is an invalid ID", paramName);
            }
        }

        /// <summary>
        /// Returns if the provided table name is valid.
        /// </summary>
        /// <param name="tableName">The name of the table to validate.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">if the table name is null.</exception>
        /// <exception cref="ArgumentException">if the table name is invalid.</exception>
        public static void IsValidTableName(string tableName, string paramName)
            => IsValidTableName(tableName, false, paramName);

        /// <summary>
        /// Returns if the provided table name is valid.
        /// </summary>
        /// <param name="tableName">The name of the table to validate.</param>
        /// <param name="allowSystemTables">If <c>true</c>, allow system tables.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">if the table name is null.</exception>
        /// <exception cref="ArgumentException">if the table name is invalid.</exception>
        public static void IsValidTableName(string tableName, bool allowSystemTables, string paramName)
        {
            IsNotNull(tableName, paramName);
            if (allowSystemTables && tableName.StartsWith(SystemTables.Prefix))
            {
                // Skip the prefix - the rest of the system tables must still be valid
                tableName = tableName.Substring(SystemTables.Prefix.Length);
            }
            if (!validTableNameRegex.IsMatch(tableName))
            {
                throw new ArgumentException($"'{tableName}' is an invalid table name", paramName);
            }
        }
    }
}
