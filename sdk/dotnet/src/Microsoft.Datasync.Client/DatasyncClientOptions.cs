// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A model class for holding the client options used to alter the communication between
    /// the client and the datasync service.
    /// </summary>
    public class DatasyncClientOptions
    {
        /// <summary>
        /// The serializer options used for deserializing content from the datasync service.
        /// </summary>
        public JsonSerializerOptions DeserializerOptions { get; set; }

        /// <summary>
        /// An ordered sequence of <see cref="HttpMessageHandler"/> objects to apply as a pipeline of
        /// policies to the HTTP request/response.  Each one except an (optional) last one must be a
        /// <see cref="DelegatingHandler"/>.  The last one can either be a <see cref="DelegatingHandler"/>
        /// or a <see cref="HttpClientHandler"/>.
        /// </summary>
        public IEnumerable<HttpMessageHandler> HttpPipeline { get; set; } = Array.Empty<HttpMessageHandler>();

        /// <summary>
        /// The unique installation ID for this application on this device.
        /// </summary>
        public string InstallationId { get; set; } = Platform.InstallationId;

        /// <summary>
        /// The serializer options used for serializing content to be sent to the datasync service.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; set; }

        /// <summary>
        /// The prefix for the tables to generate a relative URI to the table endpoint.
        /// </summary>
        public string TablesPrefix { get; set; } = "/tables/";

        /// <summary>
        /// The value used for the <c>User-Agent</c> header.  By default, this includes enough information
        /// to do telemetry easily without being too obtrusive.  We'd prefer it if you didn't change this.
        /// </summary>
        public string UserAgent { get; set; } = $"Datasync/{Platform.AssemblyVersion} ({Platform.UserAgentDetails})";

        /// <summary>
        /// Establish defaults for the <see cref="DatasyncClientOptions"/>
        /// </summary>
        public DatasyncClientOptions()
        {
            SerializerOptions = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                NumberHandling = JsonNumberHandling.Strict,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            SerializerOptions.Converters.Add(new IsoDateTimeOffsetConverter());
            SerializerOptions.Converters.Add(new IsoDateTimeConverter());

            DeserializerOptions = SerializerOptions;
        }
    }
}
