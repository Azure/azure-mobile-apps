// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Json;
using NetTopologySuite.IO.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http;

public class ServiceClient
{
    public ServiceClient(Uri serviceUri) : this(serviceUri, DefaultJsonSerializerOptions())
    {
    }

    public ServiceClient(Uri serviceUri, JsonSerializerOptions serializerOptions) : this(serviceUri, new ServiceClientOptions(), serializerOptions)
    {
    }

    public ServiceClient(Uri serviceUri, ServiceClientOptions options) : this(serviceUri, options, DefaultJsonSerializerOptions())
    {
    }

    public ServiceClient(Uri serviceUri, ServiceClientOptions options, JsonSerializerOptions serializerOptions)
    {
        ServiceUri = serviceUri;
        ClientOptions = options;
        SerializerOptions = serializerOptions;
    }

    /// <summary>
    /// The client options to use for communicating with the service.
    /// </summary>
    public ServiceClientOptions ClientOptions { get; }

    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> to use for serializing and deserializing content.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    /// The base URI address for the service;
    /// </summary>
    public Uri ServiceUri { get; }

    /// <summary>
    /// Provides the default <see cref="JsonSerializerOptions"/> for a datasync service.
    /// </summary>
    /// <returns>The default <see cref="JsonSerializerOptions"/>.</returns>
    internal static JsonSerializerOptions DefaultJsonSerializerOptions() => new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        Converters =
            {
                new JsonStringEnumConverter(),
                new DateTimeOffsetConverter(),
                new DateTimeConverter(),
                new TimeOnlyConverter(),
                new GeoJsonConverterFactory()
            },
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}
