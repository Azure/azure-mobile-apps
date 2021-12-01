// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Datasync.Common.Test.Mocks
{
    /// <summary>
    /// A test delegating handler that simulates timeout.
    /// </summary>
    public class TimeoutDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token = default)
        {
            throw new OperationCanceledException();
        }
    }
}
