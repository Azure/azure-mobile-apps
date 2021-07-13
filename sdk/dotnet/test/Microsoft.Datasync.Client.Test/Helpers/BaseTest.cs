﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Extensions;
using Datasync.Common.Test.Models;
using Datasync.Webservice;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace Microsoft.Datasync.Client.Test
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseTest
    {
        /// <summary>
        /// Required entities for a number of different tests
        /// </summary>
        protected const string sEndpoint = "http://localhost/tables/movies/";
        protected readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };
        protected readonly Dictionary<string, object> changes = new() { { "stringValue", "test" } };
        protected const string sJsonPayload = "{\"id\":\"db0ec08d-46a9-465d-9f5e-0066a3ee5b5f\",\"stringValue\":\"test\"}";
        protected const string sBadJson = "{this-is-bad-json";
        protected const string sId = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f";

        /// <summary>
        /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
        /// object and then adjust.
        /// </summary>
        protected readonly ClientMovie blackPantherMovie = new()
        {
            BestPictureWinner = true,
            Duration = 134,
            Rating = "PG-13",
            ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
            Title = "Black Panther",
            Year = 2018
        };

        /// <summary>
        /// The test server - lazily assigned.
        /// </summary>
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

        /// <summary>
        /// Creates a paging response.
        /// </summary>
        /// <param name="count">The count of elements to return</param>
        /// <param name="totalCount">The total count</param>
        /// <param name="nextLink">The next link</param>
        /// <returns></returns>
        protected Page<IdEntity> CreatePageOfItems(int count, long? totalCount = null, Uri nextLink = null)
        {
            List<IdEntity> items = new();

            for (int i = 0; i < count; i++)
            {
                items.Add(new IdEntity { Id = Guid.NewGuid().ToString("N") });
            }
            var page = new Page<IdEntity> { Items = items, Count = totalCount, NextLink = nextLink };
            MockHandler.AddResponse(HttpStatusCode.OK, page);
            return page;
        }
    }
}
