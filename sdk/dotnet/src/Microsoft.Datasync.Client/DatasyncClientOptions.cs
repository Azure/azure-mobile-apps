// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The options used to configure the <see cref="DatasyncClient"/>.
    /// </summary>
    public class DatasyncClientOptions
    {
        private int _parallelOperations = 1;

        /// <summary>
        /// The maximum number of threads that can be requested.
        /// </summary>
        public const int MAX_PARALLEL_OPERATIONS = 8;

        /// <summary>
        /// The HTTP Pipeline to use.  This can be null.  If set, it must
        /// be an ordered set of <see cref="DelegatingHandler"/> objects,
        /// potentially followed by a <see cref="HttpClientHandler"/> for
        /// a transport.
        /// </summary>
        public IEnumerable<HttpMessageHandler> HttpPipeline { get; set; }

        /// <summary>
        /// If set, the timeout to use with <see cref="HttpClient"/> connections.
        /// If not set, the default of 100,000ms (100 seconds) will be used.
        /// </summary>
        public TimeSpan? HttpTimeout { get; set; }

        /// <summary>
        /// The id generator to use for customize id. By default, if id is empty is used Guid.NewGuid().ToString("N").
        /// </summary>
        public Func<string, string> IdGenerator { get; set; }

        /// <summary>
        /// If set, use this as the installation ID.  The installation ID
        /// is sent to the remote server in the <c>ZUMO-INSTALLATION-ID</c>
        /// header.
        /// </summary>
        public string InstallationId { get; set; }

        /// <summary>
        /// The offline store to use for offline table storage.
        /// </summary>
        public IOfflineStore OfflineStore { get; set; }

        /// <summary>
        /// The number of parallel operations that can occur when running the 
        /// push operation queue.  By default, we do this serially (i.e. one
        /// thread).
        /// </summary>
        public int ParallelOperations
        {
            get => _parallelOperations;
            set
            {
                if (value <= 0 || value > MAX_PARALLEL_OPERATIONS)
                {
                    throw new ArgumentOutOfRangeException(nameof(ParallelOperations));
                }
                _parallelOperations = value;
            }
        }

        /// <summary>
        /// The serializer settings to use for this connection.
        /// </summary>
        public DatasyncSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// An optional resolver for the table endpoint.  If set, the func must return a path, like
        /// <c>/tables/tableName</c>.
        /// </summary>
        public Func<string, string> TableEndpointResolver { get; set; }

        /// <summary>
        /// The value used for the <c>User-Agent</c> header.  By default, this includes enough information
        /// to do telemetry easily without being too obtrusive.  We'd prefer it if you didn't change this.
        /// </summary>
        public string UserAgent { get; set; } = $"Datasync/{Platform.AssemblyVersion} ({Platform.UserAgentDetails})";
    }
}
