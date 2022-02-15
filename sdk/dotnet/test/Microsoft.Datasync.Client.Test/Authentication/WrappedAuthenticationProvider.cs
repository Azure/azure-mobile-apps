// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Test.Authentication
{
    /// <summary>
    /// Wrap of the <see cref="GenericAuthenticationProvider"/> that provides public access to `SendAsync()`
    /// </summary
    [ExcludeFromCodeCoverage]
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
