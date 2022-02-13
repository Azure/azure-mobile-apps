// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Provides basic access to a Microsoft Datasync service.
    /// </summary>
    public class DatasyncClient : IDisposable
    {
        /// <summary>
        /// Constructor, used for unit-testing
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected DatasyncClient()
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <exception cref="UriFormatException">if the endpoint is not a valid Uri.</exception>
        public DatasyncClient(string endpoint)
            : this(new Uri(endpoint, UriKind.Absolute), null, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <param name="clientOptions">The client options used to modify any request/response that is sent.</param>
        /// <exception cref="UriFormatException">if the endpoint is not a valid Uri.</exception>
        public DatasyncClient(string endpoint, DatasyncClientOptions clientOptions)
            : this(new Uri(endpoint, UriKind.Absolute), null, clientOptions)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <param name="authenticationProvider">The authentication provider to use for authenticating the request</param>
        /// <exception cref="UriFormatException">if the endpoint is malformed</exception>
        public DatasyncClient(string endpoint, AuthenticationProvider authenticationProvider)
            : this(new Uri(endpoint, UriKind.Absolute), authenticationProvider, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <param name="authenticationProvider">The authentication provider to use for authenticating the request</param>
        /// <param name="clientOptions">The client options used to modify any request/response that is sent.</param>
        /// <exception cref="UriFormatException">if the endpoint is malformed</exception>
        public DatasyncClient(string endpoint, AuthenticationProvider authenticationProvider, DatasyncClientOptions clientOptions)
            : this(new Uri(endpoint, UriKind.Absolute), authenticationProvider, clientOptions)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <exception cref="ArgumentNullException">if the endpoint is null</exception>
        /// <exception cref="UriFormatException">if the endpoint is malformed</exception>
        public DatasyncClient(Uri endpoint)
            : this(endpoint, null, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <param name="clientOptions">The client options used to modify any request/response that is sent.</param>
        /// <exception cref="ArgumentNullException">if the endpoint is null</exception>
        /// <exception cref="UriFormatException">if the endpoint is malformed</exception>
        public DatasyncClient(Uri endpoint, DatasyncClientOptions clientOptions)
            : this(endpoint, null, clientOptions)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <param name="authenticationProvider">The authentication provider to use for authenticating the request</param>
        /// <exception cref="ArgumentNullException">if the endpoint is null</exception>
        /// <exception cref="UriFormatException">if the endpoint is malformed</exception>
        public DatasyncClient(Uri endpoint, AuthenticationProvider authenticationProvider)
            : this(endpoint, authenticationProvider, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <param name="authenticationProvider">The authentication provider to use for authenticating the request</param>
        /// <param name="clientOptions">The client options used to modify any request/response that is sent.</param>
        /// <exception cref="ArgumentNullException">if the endpoint is null</exception>
        /// <exception cref="UriFormatException">if the endpoint is malformed</exception>
        public DatasyncClient(Uri endpoint, AuthenticationProvider authenticationProvider, DatasyncClientOptions clientOptions)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));

            Endpoint = endpoint.NormalizeEndpoint();
            ClientOptions = clientOptions ?? new DatasyncClientOptions();
            HttpClient = new ServiceHttpClient(Endpoint, authenticationProvider, ClientOptions);
        }

        /// <summary>
        /// The base <see cref="Uri"/> for the datasync service this client is communicating with.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// The client options used to communicate with the remote datasync service.
        /// </summary>
        public DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// The <see cref="ServiceHttpClient"/> used to communicate with the remote datasync service.
        /// </summary>
        internal ServiceHttpClient HttpClient { get; private set; }

        /// <summary>
        /// Obtains a reference to a remote table, which provides untyped data operations for the
        /// specified table.
        /// </summary>
        /// <param name="tableName">The name of the table, or relative URI to the table endpoint.</param>
        /// <returns>A reference to the remote table.</returns>
        public IRemoteTable GetRemoteTable(string tableName)
        {
            string relativeUri = tableName.StartsWith("/") ? tableName : ToRelativeUri(tableName);
            Validate.IsRelativeUri(relativeUri, nameof(relativeUri));
            return new RemoteTable(relativeUri, HttpClient, ClientOptions);
        }

        /// <summary>
        /// Obtain an <see cref="IDatasyncTable{T}"/> instance, which provides typed data operations for the specified type.
        /// The table is converted to lower case and then combined with the <see cref="DatasyncClientOptions.TablesPrefix"/>
        /// to generate the relative URI.
        /// </summary>
        /// <typeparam name="T">The strongly-typed model type</typeparam>
        /// <returns>A generic typed table reference.</returns>
        public IRemoteTable<T> GetTable<T>()
            => GetTable<T>(ToRelativeUri(typeof(T).Name.ToLowerInvariant()));

        /// <summary>
        /// Obtain an <see cref="IDatasyncTable{T}"/> instance, which provides typed data operations for the specified table.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="tableName"/> starts with a <c>/</c>, then it is assumed to be a relative URI
        /// instead of a table name, and used directly.
        /// </remarks>
        /// <typeparam name="T">The strongly-typed model type.</typeparam>
        /// <param name="tableName">The name of the table, or relative URI to the table.</param>
        /// <returns>A generic typed table reference.</returns>
        public IRemoteTable<T> GetTable<T>(string tableName)
        {
            string relativeUri = tableName.StartsWith("/") ? tableName : ToRelativeUri(tableName);
            Validate.IsRelativeUri(relativeUri, nameof(relativeUri));
            return new RemoteTable<T>(relativeUri, HttpClient, ClientOptions);
        }

        /// <summary>
        /// Converts the provided <paramref name="tableName"/> into a relative URI.
        /// </summary>
        /// <param name="tableName">The table name to convert</param>
        /// <returns>The relative URI to the table</returns>
        private string ToRelativeUri(string tableName) => $"/{ClientOptions.TablesPrefix.Trim('/')}/{tableName}";

        #region IDisposable
        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> pattern for derived classes to use.
        /// </summary>
        /// <param name="disposing">Indicates if being called from the <see cref="Dispose"/> method or the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (HttpClient != null)
                {
                    HttpClient.Dispose();
                    HttpClient = null;
                }
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> pattern.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
