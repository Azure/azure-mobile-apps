// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

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
        /// <param name="clientOptions">The client options used to modify any request/response that is sent.</param>
        /// <exception cref="UriFormatException">if the endpoint is not a valid Uri.</exception>
        public DatasyncClient(string endpoint, DatasyncClientOptions clientOptions = null)
            : this(new Uri(endpoint, UriKind.Absolute), clientOptions)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DatasyncClient"/> that connects to the specified endpoint for information transfer.
        /// </summary>
        /// <param name="endpoint">The endpoint of the datasync service.</param>
        /// <param name="clientOptions">The client options used to modify any request/response that is sent.</param>
        /// <exception cref="ArgumentNullException">if the endpoint is null</exception>
        /// <exception cref="UriFormatException">if the endpoint is malformed</exception>
        public DatasyncClient(Uri endpoint, DatasyncClientOptions clientOptions = null)
        {
            Validate.IsValidEndpoint(endpoint, nameof(endpoint));

            Endpoint = endpoint.NormalizeEndpoint();
            ClientOptions = clientOptions ?? new DatasyncClientOptions();
        }

        /// <summary>
        /// The base <see cref="Uri"/> for the datasync service this client is communicating with.
        /// </summary>
        public Uri Endpoint { get; }

        /// <summary>
        /// The client options used to communicate with the remote datasync service.
        /// </summary>
        public DatasyncClientOptions ClientOptions { get; }

        #region IDisposable
        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> pattern for derived classes to use.
        /// </summary>
        /// <param name="disposing">Indicates if being called from the <see cref="Dispose"/> method or the finalizer.</param>
        [ExcludeFromCodeCoverage]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }
        }

        /// <summary>
        /// Implementation of the <see cref="IDisposable"/> pattern.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
