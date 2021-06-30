// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Webservice;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Integration.Helpers
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public abstract class BaseTest
    {
        /// <summary>
        /// The endpoint to use as a base address for the TestService requests
        /// </summary>
        protected const string ServiceEndpoint = "https://localhost";

        /// <summary>
        /// Lazy initialization of the test server.
        /// </summary>
        protected readonly Lazy<TestServer> TestServer = new(() => Program.CreateTestServer());

        /// <summary>
        /// Get a reference to the <see cref="DatasyncClient"/> that is set up to communicate
        /// with the TestService
        /// </summary>
        /// <returns>A valid <see cref="DatasyncClient"/> reference</returns>
        public DatasyncClient GetClient(HttpMessageHandler[] handlers = null)
        {
            List<HttpMessageHandler> _handlers = new(handlers ?? Array.Empty<HttpMessageHandler>());
            _handlers.Add(TestServer.Value.CreateHandler());
            var options = new DatasyncClientOptions() { HttpPipeline = _handlers.ToArray() };
            return new DatasyncClient(ServiceEndpoint, options);
        }
    }
}
