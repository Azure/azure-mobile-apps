// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Authentication;

namespace Datasync.Common.Test.Mocks;

/// <summary>
/// A test authentication provider that just takes an AuthenticationToken and
/// adds it to the request.
/// </summary>
[ExcludeFromCodeCoverage]
public class MockAuthenticationProvider : AuthenticationProvider
{
    public MockAuthenticationProvider(AuthenticationToken token)
    {
        Token = token;
    }

    public AuthenticationToken Token { get; set; }

    public string HeaderName { get; set; } = "X-ZUMO-AUTH";

    public override Task LoginAsync() => Task.CompletedTask;

    /// <summary>
    /// The delegating handler for this request - injects the authorization header into the request.
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>The response (asynchronously)</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        if (request.Headers.Contains(HeaderName))
        {
            request.Headers.Remove(HeaderName);
        }
        request.Headers.Add(HeaderName, Token.Token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
