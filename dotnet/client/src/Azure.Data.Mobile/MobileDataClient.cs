﻿using Azure.Core;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Azure.Data.Mobile
{
    /// <summary>
    /// Provides basic access to a Microsoft Azure Mobile Apps service.
    /// </summary>
    public class MobileDataClient
    {
        #region Constructors
        /// <summary>
        /// Initialize a new instance of <see cref="MobileDataClient"/> for mocking.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected MobileDataClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileDataClient"/>
        /// </summary>
        /// <param name="endpoint">The <see cref="Uri"/> to the backend service</param>
        public MobileDataClient(Uri endpoint)
            : this(endpoint, null, new MobileDataClientOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileDataClient"/>
        /// </summary>
        /// <param name="endpoint">The <see cref="Uri"/> to the backend service</param>
        /// <param name="credential">The <see cref="TokenCredential"/> for authentication</param>
        public MobileDataClient(Uri endpoint, TokenCredential credential)
            : this(endpoint, credential, new MobileDataClientOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileDataClient"/>
        /// </summary>
        /// <param name="endpoint">The <see cref="Uri"/> to the backend service</param>
        /// <param name="options">The client options for this connection</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="endpoint"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="endpoint"/> is not an absolute Uri.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="options"/> is <c>null</c>.
        /// </exception>
        public MobileDataClient(Uri endpoint, MobileDataClientOptions options)
            : this(endpoint, null, options)
        {
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="MobileDataClient"/>
        /// </summary>
        /// <param name="endpoint">The <see cref="Uri"/> to the backend service</param>
        /// <param name="credential">The <see cref="TokenCredential"/> for authentication</param>
        /// <param name="options">The client options for this connection</param>
        public MobileDataClient(Uri endpoint, TokenCredential credential, MobileDataClientOptions options)
        {
            Arguments.IsNotNull(endpoint, nameof(endpoint));
            Arguments.IsNotNull(options, nameof(options));

            if (!endpoint.IsAbsoluteUri)
            {
                throw new ArgumentException("Endpoint must be an absolute Uri", nameof(endpoint));
            }

            Endpoint = endpoint;
            Credential = credential;
            ClientOptions = options;
        }

        /// <summary>
        /// The base <see cref="Uri"/> for the backend service.
        /// </summary>
        internal Uri Endpoint { get; }

        /// <summary>
        /// The credential to use for authorization.
        /// </summary>
        internal TokenCredential Credential { get; }

        /// <summary>
        /// The client options for this connection.
        /// </summary>
        internal MobileDataClientOptions ClientOptions { get; }

        /// <summary>
        /// Obtain a reference to a <see cref="MobileDataTable{T}"/> for a typed entity, using
        /// a specific relative path to the table controller.  If the relative path to the 
        /// endpoint is null, then the default path is used.
        /// </summary>
        /// <typeparam name="T">The type of the entity managed by the table</typeparam>
        /// <param name="relativePath">The relative path (to the Endpoint) to the table controller</param>
        /// <returns>A <see cref="MobileDataTable{T}"/> reference</returns>
        /// <exception cref="UriFormatException">
        ///     <paramref name="relativePath"/> does not produce a valid Uri.
        /// </exception>
        public MobileDataTable<T> GetTable<T>(string relativePath = null) where T : TableData
        {
            var tableEndpoint = relativePath == null
                ? new Uri(Endpoint, $"tables/{typeof(T).Name.ToLowerInvariant()}s")
                : new Uri(Endpoint, relativePath);
            return new MobileDataTable<T>(this, tableEndpoint);
        }
    }
}
