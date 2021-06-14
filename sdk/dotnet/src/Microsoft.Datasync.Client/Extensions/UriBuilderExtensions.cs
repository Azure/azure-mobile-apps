// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Datasync.Client.Extensions
{
    /// <summary>
    /// A set of extension methods for <see cref="UriBuilder"/> that implements a
    /// Fluent API.
    /// </summary>
    public static class UriBuilderExtensions
    {
        /// <summary>
        /// Sets the Fragment portion of the <see cref="UriBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <param name="fragment">The new fragment</param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithFragment(this UriBuilder builder, string fragment)
        {
            builder.Fragment = fragment;
            return builder;
        }
        /// <summary>
        /// Sets the Fragment portion of the <see cref="UriBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <param name="fragment">The new fragment</param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithHost(this UriBuilder builder, string host)
        {
            builder.Host = host;
            return builder;
        }
        /// <summary>
        /// Sets the Credentials portion of the <see cref="UriBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <param name="username">The new username</param>
        /// <param name="password">The new password</param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithCredentials(this UriBuilder builder, string username, string password)
        {
            builder.UserName = username;
            builder.Password = password;
            return builder;
        }
        /// <summary>
        /// Sets the Fragment portion of the <see cref="UriBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <param name="fragment">The new fragment</param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithPath(this UriBuilder builder, string path)
        {
            builder.Path = path;
            return builder;
        }
        /// <summary>
        /// Sets the Port portion of the <see cref="UriBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <param name="port">The new port</param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithPort(this UriBuilder builder, int port)
        {
            if (port == -1 || (port > 1 && port < 65535))
            {
                builder.Port = port;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            return builder;
        }
        /// <summary>
        /// Sets the Query portion of the <see cref="UriBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <param name="query">The new query</param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithQuery(this UriBuilder builder, string query)
        {
            builder.Query = query;
            return builder;
        }

        /// <summary>
        /// Sets the Scheme portion of the <see cref="UriBuilder"/>
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <param name="scheme">The new scheme</param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithScheme(this UriBuilder builder, string scheme)
        {
            if (scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) || scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase))
            {
                builder.Scheme = scheme;
            }
            else
            {
                throw new NotSupportedException("Non-HTTP scheme is not supported");
            }
            return builder;
        }
        /// <summary>
        /// Adds a trailing slash to the path if it hasn't got one.
        /// </summary>
        /// <param name="builder">The <see cref="UriBuilder"/></param>
        /// <returns>The <see cref="UriBuilder"/></returns>
        public static UriBuilder WithTrailingSlash(this UriBuilder builder)
        {
            if (!builder.Path.EndsWith("/"))
            {
                builder.Path += "/";
            }
            return builder;
        }
    }
}
