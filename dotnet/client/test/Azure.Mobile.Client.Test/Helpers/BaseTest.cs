﻿using Azure.Core.Pipeline;
using Microsoft.AspNetCore.TestHost;
using System;

namespace Azure.Mobile.Client.Test.Helpers
{
    public abstract class BaseTest
    {
        private TestServer server = E2EServer.Program.GetTestServer();

        /// <summary>
        /// Returns a MobileDataClient that hits the test server.
        /// </summary>
        /// <returns>A <see cref="MobileDataClient"/> reference.</returns>
        internal MobileDataClient GetTestClient()
        {
            var httpClient = server.CreateClient();

            var clientOptions = new MobileDataClientOptions()
            {
                Transport = new HttpClientTransport(httpClient)
            };

            return new MobileDataClient(new Uri("https://localhost:5001"), clientOptions);
        }
    }
}
