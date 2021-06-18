// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Internal;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A client class for accessing an ASP.NET Core datasync service
    /// </summary>
    public class DatasyncClient : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> using the default client options.
        /// </summary>
        /// <param name="endpoint">The HTTP endpoint for the datasync service</param>
        /// <exception cref="UriFormatException">if the HTTP endpoint is invalid</exception>
        public DatasyncClient(string endpoint)
            : this(new Uri(endpoint), new DatasyncClientOptions())
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> using the default client options.
        /// </summary>
        /// <param name="endpoint">The HTTP endpoint for the datasync service</param>
        /// <exception cref="UriFormatException">if the HTTP endpoint is invalid</exception>
        public DatasyncClient(Uri endpoint)
            : this(endpoint, new DatasyncClientOptions())
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> using the given client options.
        /// </summary>
        /// <param name="endpoint">The HTTP endpoint for the datasync service</param>
        /// <param name="options">The client options</param>
        /// <exception cref="UriFormatException">if the HTTP endpoint is invalid</exception>
        public DatasyncClient(string endpoint, DatasyncClientOptions options)
            : this(new Uri(endpoint), options)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> using the given client options.
        /// </summary>
        /// <param name="endpoint">The HTTP endpoint for the datasync service</param>
        /// <param name="options">The client options</param>
        /// <exception cref="UriFormatException">if the HTTP endpoint is invalid</exception>
        public DatasyncClient(Uri endpoint, DatasyncClientOptions options)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));
            Validate.IsNotNull(options, nameof(options));

            Endpoint = new UriBuilder(endpoint).Normalized().Uri;
            ClientOptions = options;
            HttpClient = new InternalHttpClient(Endpoint, ClientOptions);
        }

        /// <summary>
        /// The HTTP endpoint for the datasync service.
        /// </summary>
        internal Uri Endpoint { get; }

        /// <summary>
        /// The client options used to communicate with the datasync service.
        /// </summary>
        internal DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// The <see cref="HttpClient"/> used for communicating with the datasync service.
        /// </summary>
        internal InternalHttpClient HttpClient { get; }

        /// <summary>
        /// Returns a <see cref="IDatasyncTable{T}"/> instance, which provides typed data operations for
        /// that table.
        /// </summary>
        /// <typeparam name="T">The type of the table entities</typeparam>
        /// <returns>The table instance.</returns>
        public IDatasyncTable<T> GetTable<T>() where T : notnull
            => GetTable<T>($"{ClientOptions.TablesUri}/{typeof(T).Name.ToLowerInvariant()}");

        /// <summary>
        /// Returns a <see cref="IDatasyncTable{T}"/> instance, which provides typed data operations for
        /// that table.
        /// </summary>
        /// <typeparam name="T">The type of the table entities</typeparam>
        /// <param name="relativeUri">The relative Uri to the table, from the service endpoint</param>
        /// <returns>The table instance.</returns>
        public IDatasyncTable<T> GetTable<T>(string relativeUri) where T : notnull
            => GetTable<T>(new Uri(relativeUri, UriKind.Relative));

        /// <summary>
        /// Returns a <see cref="IDatasyncTable{T}"/> instance, which provides typed data operations for
        /// that table.
        /// </summary>
        /// <typeparam name="T">The type of the table entities</typeparam>
        /// <param name="relativeUri">The relative Uri to the table, from the service endpoint</param>
        /// <returns>The table instance.</returns>
        public IDatasyncTable<T> GetTable<T>(Uri relativeUri) where T : notnull
        {
            Validate.IsNotNull(relativeUri, nameof(relativeUri));
            if (relativeUri.IsAbsoluteUri)
            {
                throw new UriFormatException($"{nameof(relativeUri)} is not relative");
            }
            return new DatasyncTable<T>(new Uri(Endpoint, relativeUri), HttpClient, ClientOptions);
        }

        #region Disposable Interface
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    HttpClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
