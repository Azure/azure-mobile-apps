// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    /// <summary>
    /// Common test artifacts
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public abstract class BaseTest
    {
        protected readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        protected readonly JsonSerializerOptions DeserializerOptions = new()
        {
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.Strict,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        protected readonly Uri Endpoint;
        protected readonly MockDelegatingHandler ClientHandler;
        protected readonly DatasyncClientOptions ClientOptions;
        internal readonly InternalHttpClient HttpClient;
        protected readonly DatasyncTable<IdEntity> Table;

        protected BaseTest()
        {
            Endpoint = new("https://foo.azurewebsites.net/tables/movies");
            ClientHandler = new();
            ClientOptions = new() { HttpPipeline = new HttpMessageHandler[] { ClientHandler } };
            HttpClient = new InternalHttpClient(Endpoint, ClientOptions);
            Table = new DatasyncTable<IdEntity>(Endpoint, HttpClient, ClientOptions);
        }

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
            ClientHandler.AddResponse(HttpStatusCode.OK, page);
            return page;
        }
    }
}
