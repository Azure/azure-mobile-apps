// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json;

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

        /// <summary>
        /// Sets the query parameters of a Uri.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/> to modify</param>
        /// <param name="queryString">the query to set</param>
        /// <returns>The updated <see cref="UriBuilder"/></returns>
        internal static UriBuilder WithQuery(this UriBuilder builder, string queryString)
        {
            builder.Query = string.IsNullOrWhiteSpace(queryString) ? string.Empty : queryString.Trim();
            return builder;
        }

        /// <summary>
        /// Gets the ID for a given <see cref="JsonDocument"/> entity.
        /// </summary>
        /// <param name="document">The document to process.</param>
        /// <returns>The ID of the document.</returns>
        internal static string GetId(this JsonDocument document)
        {
            if (document?.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (document.RootElement.TryGetProperty("id", out JsonElement idElement))
                {
                    if (idElement.ValueKind == JsonValueKind.String)
                    {
                        return idElement.GetString();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the version for a given <see cref="JsonDocument"/> entity.
        /// </summary>
        /// <param name="document">The document to process.</param>
        /// <returns>The version of the document.</returns>
        internal static string GetVersion(this JsonDocument document)
        {
            if (document?.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (document.RootElement.TryGetProperty("version", out JsonElement versionElement))
                {
                    if (versionElement.ValueKind == JsonValueKind.String)
                    {
                        return versionElement.GetString();
                    }
                }
            }

            return null;
        }
    }
}
