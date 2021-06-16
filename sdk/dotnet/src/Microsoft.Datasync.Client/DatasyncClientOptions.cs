// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The options used to adjust the HTTP requests and responses for a datasync service.
    /// </summary>
    public class DatasyncClientOptions
    {
        private JsonSerializerOptions _deserializerOptions = new()
        {
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.Strict,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private IEnumerable<HttpMessageHandler> _httpPipeline = Array.Empty<HttpMessageHandler>();

        private JsonSerializerOptions _serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            NumberHandling = JsonNumberHandling.Strict,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private string _tablesUri = "tables";

        /// <summary>
        /// The serializer options (from System.Text.Json) used when deserializing content
        /// from the datasync service.  This must match the serializer options on the service.
        /// </summary>
        public JsonSerializerOptions DeserializerOptions
        {
            get => _deserializerOptions;
            set => _deserializerOptions = value ?? throw new ArgumentNullException(nameof(DeserializerOptions));
        }

        /// <summary>
        /// A list of <see cref="DelegatingHandler"/> objects to be used as a pipeline for
        /// requests.  The last entry can optionally be a <see cref="HttpClientHandler"/>.
        /// </summary>
        public IEnumerable<HttpMessageHandler> HttpPipeline
        {
            get => _httpPipeline;
            set => _httpPipeline = value ?? throw new ArgumentNullException(nameof(HttpPipeline));
        }

        /// <summary>
        /// The serializer options (from System.Text.Json) used when serializing content for
        /// the datasync service.  This must match the deserializer options on the service.
        /// </summary>
        public JsonSerializerOptions SerializerOptions
        {
            get => _serializerOptions;
            set => _serializerOptions = value ?? throw new ArgumentNullException(nameof(SerializerOptions));
        }

        /// <summary>
        /// The relative Uri to the tables enpdoint.  This is used in constructing new tables endpoints.
        /// </summary>
        public string TablesUri
        {
            get => _tablesUri;
            set => _tablesUri = (value ?? throw new ArgumentNullException(nameof(TablesUri))).Trim('/');
        }
    }
}
