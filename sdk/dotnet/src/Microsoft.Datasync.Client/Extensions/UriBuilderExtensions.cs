// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Extensions
{
    /// <summary>
    /// A set of extension methods to the <see cref="UriBuilder"/> class that turns it into
    /// a fluent API.
    /// </summary>
    internal static class UriBuilderExtensions
    {
        /// <summary>
        /// Normalizes the Uri for use within the datasync service.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/> to modify</param>
        /// <returns>The modified <see cref="UriBuilder"/></returns>
        internal static UriBuilder Normalized(this UriBuilder builder)
            => builder.WithQuery(string.Empty).WithFragment(string.Empty).WithTrailingSlash();

        /// <summary>
        /// Sets the fragment of the Uri.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/> to modify</param>
        /// <param name="fragment">The fragment to set.</param>
        /// <returns>The modified <see cref="UriBuilder"/></returns>
        internal static UriBuilder WithFragment(this UriBuilder builder, string fragment)
        {
            builder.Fragment = string.IsNullOrWhiteSpace(fragment) ? string.Empty : fragment.Trim();
            return builder;
        }

        /// <summary>
        /// Sets the query string.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/> to modify</param>
        /// <param name="queryString">The query string to set.</param>
        /// <returns>The modified <see cref="UriBuilder"/></returns>
        internal static UriBuilder WithQuery(this UriBuilder builder, string queryString)
        {
            builder.Query = string.IsNullOrWhiteSpace(queryString) ? string.Empty : queryString.Trim();
            return builder;
        }

        /// <summary>
        /// If the path does not have a slash on it, add one.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/> to modify</param>
        /// <returns>The modified <see cref="UriBuilder"/></returns>
        internal static UriBuilder WithTrailingSlash(this UriBuilder builder)
        {
            if (!builder.Path.EndsWith("/"))
            {
                builder.Path += "/";
            }
            return builder;
        }
    }
}
