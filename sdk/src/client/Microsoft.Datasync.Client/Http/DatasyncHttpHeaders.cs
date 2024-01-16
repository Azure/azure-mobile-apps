namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// A list of HTTP headers that the datasync service may expect.
/// </summary>
public static class DatasyncHttpHeaders
{
    /// <summary>
    /// The unique ID of the installation.
    /// </summary>
    public const string InstallationId = "X-ZUMO-INSTALLATION-ID";

    /// <summary>
    /// Some clients can't adjust <c>User-Agent</c>, so we provide a secondary user-agent
    /// </summary>
    public const string InternalUserAgent = "X-ZUMO-VERSION";

    /// <summary>
    /// The version of the protocol being used.
    /// </summary>
    public const string ProtocolVersion = "ZUMO-API-VERSION";

    /// <summary>
    /// The <c>User-Agent</c> header name (so we don't need to bring in a NuGet for it)
    /// </summary>
    public const string UserAgent = "User-Agent";
}
