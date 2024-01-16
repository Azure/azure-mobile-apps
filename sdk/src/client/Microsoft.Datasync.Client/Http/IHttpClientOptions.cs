namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// A description of the options that can be used in configuring a new <see cref="HttpClientFactory"/>.
/// </summary>
public interface IHttpClientOptions
{
    /// <summary>
    /// The base URI for all HTTP requests.  This is the URI that is used to create the <see cref="HttpClient"/>.
    /// </summary>
    Uri Endpoint { get; }

    /// <summary>
    /// The HTTP pipeline to use.  This must be an ordered list of <see cref="DelegatingHandler"/>
    /// objects, potentially followed by a <see cref="HttpClientHandler"/> for a transport.
    /// </summary>
    IEnumerable<HttpMessageHandler> HttpPipeline { get; }

    /// <summary>
    /// The timeout to use when configuring the <see cref="HttpClient"/> instances.
    /// </summary>
    TimeSpan HttpTimeout { get; }

    /// <summary>
    /// If set, use this as the installation ID.  The installation ID is sent to the remote server in the
    /// <c>ZUMO-INSTALLATION-ID</c> header.
    /// </summary>
    string? InstallationId { get; }

    /// <summary>
    /// If set, the ZUMO-API-VERSION header will be set to this value.  This is used to indicate the version
    /// of the protocol that the client is using.
    /// </summary>
    string? ProtocolVersion { get; }

    /// <summary>
    /// The value used for the <c>User-Agent</c> header.  By default, this includes enough information
    /// to do telemetry easily without being too obtrusive.  We'd prefer it if you didn't change this.
    /// </summary>
    string? UserAgent { get; }
}
