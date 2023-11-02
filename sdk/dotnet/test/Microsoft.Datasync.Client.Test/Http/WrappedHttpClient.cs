// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Http;

namespace Microsoft.Datasync.Client.Test.Http;

/// <summary>
/// Wrapped version of the <see cref="ServiceHttpClient"/> that exposes protected elements.
/// </summary>
[ExcludeFromCodeCoverage]
internal class WrappedHttpClient : ServiceHttpClient
{
    internal WrappedHttpClient(Uri endpoint, DatasyncClientOptions options) : base(endpoint, options)
    {
    }

    internal WrappedHttpClient(Uri endpoint, AuthenticationProvider provider, DatasyncClientOptions options) : base(endpoint, provider, options)
    {
    }

    internal string WrappedEndpoint { get => Endpoint.ToString(); }

    internal HttpMessageHandler HttpHandler { get => roothandler; }

    internal void WrappedDispose(bool dispose)
        => base.Dispose(dispose);

    internal void WrappedDispose()
        => base.Dispose();

    internal Task<HttpResponseMessage> WrappedSendAsync(HttpRequestMessage request, CancellationToken token = default)
        => base.SendAsync(request, token);

    internal Task<ServiceResponse> WrappedSendAsync(ServiceRequest request, CancellationToken token = default)
        => base.SendAsync(request, token);
}
