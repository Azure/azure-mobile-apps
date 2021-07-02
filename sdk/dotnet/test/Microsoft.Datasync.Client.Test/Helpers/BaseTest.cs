// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Webservice;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseTest
    {
        private Lazy<TestServer> _testServer = new(() => Program.CreateTestServer());

        /// <summary>
        /// Default client options.
        /// </summary>
        protected DatasyncClientOptions ClientOptions { get; } = new DatasyncClientOptions();

        /// <summary>
        /// The endpoint for any HttpClient that needs to communicate with the test service.
        /// </summary>
        protected Uri Endpoint { get => _testServer.Value.BaseAddress; }
    }
}
