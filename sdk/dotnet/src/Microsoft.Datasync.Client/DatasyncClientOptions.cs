// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The options used to configure the <see cref="DatasyncClient"/>.
    /// </summary>
    public class DatasyncClientOptions
    {
        /// <summary>
        /// The HTTP Pipeline to use.  This can be null.  If set, it must
        /// be an ordered set of <see cref="DelegatingHandler"/> objects,
        /// potentially followed by a <see cref="HttpClientHandler"/> for
        /// a transport.
        /// </summary>
        public IEnumerable<HttpMessageHandler> HttpPipeline { get; set; }

        /// <summary>
        /// If set, use this as the installation ID.  The installation ID
        /// is sent to the remote server in the <c>ZUMO-INSTALLATION-ID</c>
        /// header.
        /// </summary>
        public string InstallationId { get; set; }

        /// <summary>
        /// The serializer settings to use for this connection.
        /// </summary>
        public DatasyncSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// The value used for the <c>User-Agent</c> header.  By default, this includes enough information
        /// to do telemetry easily without being too obtrusive.  We'd prefer it if you didn't change this.
        /// </summary>
        public string UserAgent { get; set; } = $"Datasync/{Platform.AssemblyVersion} ({Platform.UserAgentDetails})";
    }
}
