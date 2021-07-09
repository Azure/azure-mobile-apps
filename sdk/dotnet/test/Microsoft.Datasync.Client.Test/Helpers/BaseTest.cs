// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Extensions;
using Datasync.Webservice;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseTest
    {
        private readonly Lazy<TestServer> _testServer = new(() => Program.CreateTestServer());

        /// <summary>
        /// Default client options.
        /// </summary>
        protected DatasyncClientOptions ClientOptions { get; } = new DatasyncClientOptions();

        /// <summary>
        /// The endpoint for any HttpClient that needs to communicate with the test service.
        /// </summary>
        protected Uri Endpoint { get => _testServer.Value.BaseAddress; }

        /// <summary>
        /// The mock handler that allows us to set responses and see requests.
        /// </summary>
        protected TestDelegatingHandler MockHandler { get; } = new TestDelegatingHandler();

        /// <summary>
        /// Gets a client reference for the test server.
        /// </summary>
        /// <returns></returns>
        protected DatasyncClient CreateClientForTestServer()
        {
            var handler = _testServer.Value.CreateHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
            return new DatasyncClient(Endpoint, options);
        }

        /// <summary>
        /// Gets a client reference for mocking
        /// </summary>
        /// <returns></returns>
        protected DatasyncClient CreateClientForMocking()
        {
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { MockHandler } };
            return new DatasyncClient(Endpoint, options);
        }

        /// <summary>
        /// Gets a reference to the repository named.
        /// </summary>
        /// <typeparam name="T">The type of data stored in the repository</typeparam>
        /// <returns></returns>
        protected InMemoryRepository<T> GetRepository<T>() where T : InMemoryTableData
            => _testServer.Value.GetRepository<T>();

        /// <summary>
        /// Converts an index into an ID for the Movies controller.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetMovieId(int index) => string.Format("id-{0:000}", index);
    }
}
