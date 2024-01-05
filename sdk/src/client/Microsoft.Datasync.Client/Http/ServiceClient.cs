// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Json;
using System.Text.Json;

namespace Microsoft.Datasync.Client.Http;

public class ServiceClient
{
    public ServiceClient(Uri serviceUri) : this(serviceUri, DatasyncServiceOptions.GetJsonSerializerOptions())
    {
    }

    public ServiceClient(Uri serviceUri, JsonSerializerOptions serializerOptions) : this(serviceUri, new ServiceClientOptions(), serializerOptions)
    {
    }

    public ServiceClient(Uri serviceUri, ServiceClientOptions options) : this(serviceUri, options, DatasyncServiceOptions.GetJsonSerializerOptions())
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
}
