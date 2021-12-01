// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Datasync.Common.Test.Wraps
{
    /// <summary>
    /// Wrap of the <see cref="GenericAuthenticationProvider"/> that provides public access to `SendAsync()`
    /// </summary>
    public class WrappedAuthenticationProvider : GenericAuthenticationProvider
    {
        public WrappedAuthenticationProvider(Func<Task<AuthenticationToken>> requestor, string header = "Authorization", string authType = null)
            : base(requestor, header, authType)
        {
        }

        public Task<HttpResponseMessage> WrappedSendAsync(HttpRequestMessage request, CancellationToken token = default)
            => base.SendAsync(request, token);
    }
}
