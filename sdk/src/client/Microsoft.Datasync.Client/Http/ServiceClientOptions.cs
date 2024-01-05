// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// The settings for adjusting the behavior of the <see cref="ServiceClient"/>.
/// </summary>
public class ServiceClientOptions
{
    /// <summary>
    /// Creates a new <see cref="ServiceClientOptions"/> with the default <see cref="IHttpClientProvider"/>.
    /// </summary>
    public ServiceClientOptions()
    {
        HttpClientProvider = new DefaultHttpClientProvider(this);
    }

    /// <summary>
    /// Creates a new <see cref="ServiceClientOptions"/> with a specified <see cref="IHttpClientProvider"/>.
    /// </summary>
    /// <param name="clientProvider">The <see cref="IHttpClientProvider"/> to use.</param>
    public ServiceClientOptions(IHttpClientProvider clientProvider)
    {
        HttpClientProvider = clientProvider;
    }

    /// <summary>
    /// The object that provides a fully formed <see cref="HttpClient"/> for use by the service client.
    /// </summary>
    public IHttpClientProvider HttpClientProvider { get; set; }

    /// <summary>
    /// If using the defult <see cref="IHttpClientProvider"/>, this provides the pipeline to use.  If set,
    /// it must be an ordered set of <see cref="DelegatingHandler"/> objects, potentially followed by a
    /// <see cref="HttpClientHandler"/> for a transport.
    /// </summary>
    public IEnumerable<HttpMessageHandler> HttpPipeline { get; set; } = Array.Empty<HttpMessageHandler>();

    /// <summary>
    /// The timeout to use with the default <see cref="IHttpClientProvider"/>.
    /// </summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// If set, use this as the installation ID.  The installation ID
    /// is sent to the remote server in the <c>ZUMO-INSTALLATION-ID</c>
    /// header.
    /// </summary>
    public string InstallationId { get; set; } = string.Empty;

    /// <summary>
    /// The value used for the <c>User-Agent</c> header when using the default <see cref="IHttpClientProvider"/>.
    /// By default, this includes enough information to do telemetry easily without being too obtrusive.  We'd prefer
    /// it if you didn't change this.
    /// </summary>
    public string UserAgent { get; set; } = $"Datasync/{typeof(ServiceClientOptions).Assembly.GetName().Version} (lang=dotnet;os={RuntimeInformation.OSDescription}/{Environment.OSVersion.VersionString};arch={RuntimeInformation.OSArchitecture})";
}
