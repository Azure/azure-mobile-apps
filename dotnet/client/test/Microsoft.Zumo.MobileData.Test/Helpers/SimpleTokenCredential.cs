// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    /// <summary>
    /// A test token provider that implements a token store that the test facility can use.
    /// </summary>
    public class SimpleTokenCredential : TokenCredential
    {
        public SimpleTokenCredential() { }

        public SimpleTokenCredential(string accessToken) 
        { 
            AccessToken = accessToken; 
        }

        public string AccessToken { get; set; }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => new AccessToken(AccessToken, DateTimeOffset.UtcNow.AddMinutes(5));

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => await Task.FromResult(GetToken(requestContext, cancellationToken));
    }
}
