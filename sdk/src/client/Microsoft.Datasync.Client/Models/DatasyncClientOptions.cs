// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using System.Text.Json;

namespace Microsoft.Datasync.Client;

/// <summary>
/// The options used to configure the datasync client.
/// </summary>
public class DatasyncClientOptions
{
    /// <summary>
    /// The mechanism by which we get a pre-configured HTTP client for communicating with the service.
    /// </summary>
    public IHttpClientFactory HttpClientFactory { get; set; } = new DefaultHttpClientFactory();

    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> to use when serializing and deserializing entities.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new DatasyncClientOptions().JsonSerializerOptions;

    /// <summary>
    /// The function used to resolve a table name to the path of the table on the server.
    /// </summary>
    public Func<string, string> TableEndpointResolver { get; set; } = name => $"/tables/{name}";
}
