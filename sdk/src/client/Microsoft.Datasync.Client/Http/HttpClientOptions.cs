// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// The model to use for the options used in configuring the <see cref="DefaultHttpClientFactory"/>.
/// </summary>
internal class HttpClientOptions : IHttpClientOptions
{
    /// <summary>
    /// The URI for the Datasync service.  This is the URI that is used to create the <see cref="HttpClient"/>.
    /// </summary>
    public Uri Endpoint { get; set; } = new Uri("null:");

    /// <summary>
    /// The HTTP pipeline to use.  This must be an ordered list of <see cref="DelegatingHandler"/>
    /// objects, potentially followed by a <see cref="HttpClientHandler"/> for a transport.
    /// </summary>
    public IEnumerable<HttpMessageHandler> HttpPipeline { get; set; } = Array.Empty<HttpMessageHandler>();

    /// <summary>
    /// The timeout to use when configuring the <see cref="HttpClient"/> instances.
    /// </summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// If set, use this as the installation ID.  The installation ID is sent to the remote server in the
    /// <c>ZUMO-INSTALLATION-ID</c> header.
    /// </summary>
    public string? InstallationId { get; set; }

    /// <summary>
    /// If set, the ZUMO-API-VERSION header will be set to this value.  This is used to indicate the version
    /// of the protocol that the client is using.
    /// </summary>
    public string? ProtocolVersion { get; set; } = "3.0.0";

    /// <summary>
    /// The value used for the <c>User-Agent</c> header.  By default, this includes enough information
    /// to do telemetry easily without being too obtrusive.  We'd prefer it if you didn't change this.
    /// </summary>
    public string? UserAgent { get; set; } = $"Datasync/{AssemblyVersion} ({UserAgentDetails})";

    /// <summary>
    /// Obtains the assembly version for the current assembly.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Runtime data - cannot test")]
    private static string AssemblyVersion
        => typeof(HttpClientOptions).Assembly.GetName().Version?.ToString() ?? "err";

    /// <summary>
    /// Obtains the default value for the User-Agent details.
    /// </summary>
    private static string UserAgentDetails
        => $"lang=dotnet;os={RuntimeInformation.OSDescription};arch={RuntimeInformation.OSArchitecture}";
}
