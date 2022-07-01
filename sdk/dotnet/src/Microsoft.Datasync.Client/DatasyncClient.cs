// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                SyncContext = new SyncContext(this, ClientOptions.OfflineStore);
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
        /// Gets the <see cref="ServiceHttpClient"/> associated with the Azure Mobile App.
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
        internal SyncContext SyncContext { get; }

        /// <summary>
        /// Returns a reference to an offline table, providing untyped (JSON) data
        /// operations for that table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>A reference to the table.</returns>
        public virtual IOfflineTable GetOfflineTable(string tableName)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            if (SyncContext == null)
            {
                throw new InvalidOperationException("An offline store must be specified before using offline tables.");
            }
            return new OfflineTable(tableName, this);
        }

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
        {
            tableName ??= Serializer.ResolveTableName<T>();
            Arguments.IsValidTableName(tableName, nameof(tableName));
            if (SyncContext == null)
            {
                throw new InvalidOperationException("An offline store must be specified before using offline tables.");
            }
            if (SyncContext.OfflineStore is AbstractOfflineStore store && !store.TableIsDefined(tableName))
            {
                store.DefineTable<T>(tableName);
            }
            return new OfflineTable<T>(tableName, this);
        }

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

        /// <summary>
        /// Initializes the offline store.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the offline store is initialized.</returns>
        /// <exception cref="InvalidOperationException">if the offline store was not provided.</exception>
        public virtual async Task InitializeOfflineStoreAsync(CancellationToken cancellationToken = default)
        {
            if (SyncContext == null)
            {
                throw new InvalidOperationException("An offline store must be specified before initialization.");
            }
            await SyncContext.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Pushes the pending operations for all tables to the remote service.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pending operations have been pushed.</returns>
        /// <exception cref="InvalidOperationException">if the offline store is not available.</exception>
        public virtual Task PushTablesAsync(CancellationToken cancellationToken = default)
            => PushTablesAsync(Array.Empty<string>(), null, cancellationToken);

        /// <summary>
        /// Pushes the pending operations for all tables to the remote service.
        /// </summary>
        /// <param name="tables">The list of tables to be pushed.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pending operations have been pushed.</returns>
        /// <exception cref="InvalidOperationException">if the offline store is not available.</exception>
        public virtual Task PushTablesAsync(IEnumerable<string> tables, CancellationToken cancellationToken = default)
            => PushTablesAsync(tables, null, cancellationToken);

        /// <summary>
        /// Pushes the pending operations for all tables to the remote service.
        /// </summary>
        /// <param name="options">The push operation options.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pending operations have been pushed.</returns>
        /// <exception cref="InvalidOperationException">if the offline store is not available.</exception>
        public virtual Task PushTablesAsync(PushOptions options, CancellationToken cancellationToken = default)
            => PushTablesAsync(Array.Empty<string>(), options, cancellationToken);

        /// <summary>
        /// Pushes the pending operations for a list of tables to the remote service.  You must name the tables
        /// to be pushed.
        /// </summary>
        /// <param name="tables">The list of tables to be pushed.</param>
        /// <param name="options">The push operation options.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the pending operations have been pushed.</returns>
        /// <exception cref="InvalidOperationException">if the offline store is not available.</exception>
        public virtual async Task PushTablesAsync(IEnumerable<string> tables, PushOptions options, CancellationToken cancellationToken = default)
        {
            if (SyncContext == null)
            {
                throw new InvalidOperationException("An offline store must be specified before doing offline operations.");
            }
            await SyncContext.PushItemsAsync(tables.ToArray(), options, cancellationToken).ConfigureAwait(false);
        }

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
