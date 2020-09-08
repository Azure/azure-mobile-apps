// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.Zumo.MobileData;
using System;

namespace Microsoft.Zumo.E2ETest.Helpers
{
    /// <summary>
    /// Base class for the E2E tests - used primarily for establishing a context
    /// for the client that routes through the test server.
    /// </summary>
    public abstract class BaseTest
    {
        private readonly Uri TestServerUri = new Uri("https://localhost:5001");

        public MobileTableClient GetClient()
        {
            var server = E2EServer.Program.GetTestServer();
            var options = new MobileTableClientOptions()
            {
                Transport = new HttpClientTransport(server.CreateClient())
            };
            return new MobileTableClient(TestServerUri, options);
        }

        public MobileTableClient GetClient(TokenCredential credential)
        {
            var server = E2EServer.Program.GetTestServer();
            var options = new MobileTableClientOptions()
            {
                Transport = new HttpClientTransport(server.CreateClient())
            };
            return new MobileTableClient(TestServerUri, credential, options);
        }

        public MobileTableClient GetClient(MobileTableClientOptions options)
        {
            var server = E2EServer.Program.GetTestServer();
            options.Transport = new HttpClientTransport(server.CreateClient());
            return new MobileTableClient(TestServerUri, options);
        }

        public MobileTableClient GetClient(TokenCredential credential, MobileTableClientOptions options)
        {
            var server = E2EServer.Program.GetTestServer();
            options.Transport = new HttpClientTransport(server.CreateClient());
            return new MobileTableClient(TestServerUri, credential, options);
        }
    }
}
