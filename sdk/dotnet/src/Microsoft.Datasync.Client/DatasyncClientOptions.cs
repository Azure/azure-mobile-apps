// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The options for configuring the datasync client.
    /// </summary>
    public class DatasyncClientOptions
    {
        private JsonSerializerOptions _deserializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.Strict,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        private IEnumerable<HttpMessageHandler> _httpPipeline = Array.Empty<HttpMessageHandler>();
        private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            NumberHandling = JsonNumberHandling.Strict,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private string _tablesUri = "tables";

        /// <summary>
        /// The serializer options to use when deserializing content from a datasync service.
        /// </summary>
        public JsonSerializerOptions DeserializerOptions
        {
            get => _deserializerOptions;
            set
            {
                _deserializerOptions = value ?? throw new ArgumentNullException(nameof(DeserializerOptions));
            }
        }

        /// <summary>
        /// A list of <see cref="DelegatingHandler"/> objects to be used as a pipeline for HTTP requests.
        /// The last entry can optionally be a <see cref="HttpClientHandler"/>
        /// </summary>
        public IEnumerable<HttpMessageHandler> HttpPipeline
        {
            get => _httpPipeline;
            set
            {
                if (value.Count() > 1 && value.Reverse().Skip(1).Any(m => !(m is DelegatingHandler)))
                {
                    throw new ArgumentException($"Invalid {nameof(HttpPipeline)} - all elements except the last one must be a DelegatingHandler", nameof(HttpPipeline));
                }
                _httpPipeline = value;
            }
        }
        /// <summary>
        /// The serializer options to use when serializing content for the datasync service.
        /// </summary>
        public JsonSerializerOptions SerializerOptions
        {
            get => _serializerOptions;
            set
            {
                _serializerOptions = value ?? throw new ArgumentNullException(nameof(SerializerOptions));
            }
        }

        /// <summary>
        /// The default relative Uri for the tables in the datasync service.  This is used to construct
        /// the full Uri to a specific table when the relative Uri is not explicitly specified.
        /// </summary>
        public string DefaultTablesUri
        {
            get => _tablesUri;
            set
            {
                _tablesUri = value ?? throw new ArgumentNullException(nameof(DefaultTablesUri));
            }
        }

        /// <summary>
        /// The protocol version string to use in the <c>ZUMO-API-VERSION</c> header.
        /// </summary>
        internal virtual string ProtocolVersion { get; } = "3.0.0";

    }
}
