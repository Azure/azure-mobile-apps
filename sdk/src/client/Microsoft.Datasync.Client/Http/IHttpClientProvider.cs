// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// When the service client needs to communicate with the service, it will request a <see cref="HttpClient"/>
/// from this provider.  The default provider caches the client and reuses it for all requests.
/// </summary>
public interface IHttpClientProvider
{
    /// <summary>
    /// Retrieves a <see cref="HttpClient"/> for use by the service client.
    /// </summary>
    /// <param name="providerName">The name of the provider; used to distinguish between clients.</param>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    HttpClient GetHttpClient(string providerName = "");
}
