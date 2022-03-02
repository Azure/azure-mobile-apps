﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Serialization;
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
        ///  This is for unit testing only
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
            Arguments.IsValidEndpoint(endpoint, nameof(endpoint));

            Endpoint = endpoint.NormalizeEndpoint();
            ClientOptions = clientOptions ?? new DatasyncClientOptions();
            HttpClient = new ServiceHttpClient(Endpoint, authenticationProvider, ClientOptions);
            if (ClientOptions.SerializerSettings != null)
            {
                Serializer.SerializerSettings = ClientOptions.SerializerSettings;
            }
            if (ClientOptions.OfflineStore != null)
            {
                SyncContext.OfflineStore = ClientOptions.OfflineStore;
            }
        }

        /// <summary>
        /// The client options for the service.
        /// </summary>
        public DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// Absolute URI of the Microsoft Azure Mobile App.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// Gets the <see cref="MobileServiceHttpClient"/> associated with the Azure Mobile App.
        /// </summary>
        internal ServiceHttpClient HttpClient { get; }

        /// <summary>
        /// The id used to identify this installation of the application to
        /// provide telemetry data.
        /// </summary>
        public string InstallationId { get => HttpClient.InstallationId; }

        /// <summary>
        /// The serializer to use for serializing and deserializing content.
        /// </summary>
        internal ServiceSerializer Serializer { get; } = new();

        /// <summary>
        /// The synchronization context.
        /// </summary>
        internal SyncContext SyncContext { get; } = new();

        /// <summary>
        /// Returns a reference to an offline table, providing untyped (JSON) data
        /// operations for that table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A reference to the table.</returns>
        public virtual IOfflineTable GetOfflineTable(string tableName)
            => new OfflineTable(tableName, this);

        /// <summary>
        /// Returns a reference to an offline table, providing typed data
        /// operations for that table.
        /// </summary>
        /// <remarks>
        /// If <paramref name="tableName"/> is not specified, the name of the
        /// type is used as the table name.
        /// </remarks>
        /// <typeparam name="T">The type of the data transfer object (model) being used.</typeparam>
        /// <param name="tableName">The (optional) name of the table.</param>
        /// <returns>A reference to the table.</returns>
        public virtual IOfflineTable<T> GetOfflineTable<T>(string tableName = null)
            => new OfflineTable<T>(tableName ?? Serializer.ResolveTableName<T>(), this);

        /// <summary>
        /// Returns a reference to a remote table, providing untyped (JSON) data
        /// operations for that table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A reference to the table.</returns>
        public virtual IRemoteTable GetRemoteTable(string tableName)
            => new RemoteTable(tableName, this);

        /// <summary>
        /// Returns a reference to a remote table, providing typed data
        /// operations for that table.
        /// </summary>
        /// <remarks>
        /// If <paramref name="tableName"/> is not specified, the name of the type is used as
        /// the table name.
        /// </remarks>
        /// <typeparam name="T">The type of the data transfer object (model) being used.</typeparam>
        /// <param name="tableName">The (optional) name of the table.</param>
        /// <returns>A reference to the table.</returns>
        public virtual IRemoteTable<T> GetRemoteTable<T>(string tableName = null)
            => new RemoteTable<T>(tableName ?? Serializer.ResolveTableName<T>(), this);

        #region IDisposable
        /// <summary>
        /// Implemenation of <see cref="IDisposable"/>
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implemenation of <see cref="IDisposable"/> for
        /// derived classes to use.
        /// </summary>
        /// <param name="disposing">
        /// Indicates if being called from the Dispose() method
        /// or the finalizer.
        /// </param>
        [ExcludeFromCodeCoverage]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SyncContext.Dispose();
                HttpClient.Dispose();
            }
        }
        #endregion
    }
}
