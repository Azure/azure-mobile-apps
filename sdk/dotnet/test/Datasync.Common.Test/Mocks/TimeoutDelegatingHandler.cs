// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Datasync.Common.Test.Mocks;

/// <summary>
/// A test delegating handler that simulates timeout.
/// </summary>
[ExcludeFromCodeCoverage]
public class TimeoutDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
    {
        throw new OperationCanceledException();
    }
}
