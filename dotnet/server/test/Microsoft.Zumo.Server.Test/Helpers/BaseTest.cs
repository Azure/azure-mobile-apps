// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Zumo.Server.Test.TestServer;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server.Test.Helpers
{
    public abstract class BaseTest
    {
        protected readonly AspNetCore.TestHost.TestServer server = GetTestServer();
        private readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public Task<HttpResponseMessage> SendRequestToServer<T>(HttpMethod method, string relativePath, T content = null, Dictionary<string, string> headers = null) where T : class
        {
            var client = server.CreateClient();
            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(new Uri("https://localhost"), relativePath)
            };
            if (content != null)
            {
                if (typeof(T) == typeof(String))
                {
                    request.Content = new StringContent(content as string, Encoding.UTF8, "application/json-patch+json");
                } 
                else
                {
                    var json = JsonSerializer.Serialize<T>(content, SerializerOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }
            return client.SendAsync(request);
        }

        public async Task<T> GetValueFromResponse<T>(HttpResponseMessage response) where T : class
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(json, SerializerOptions);
            return result;
        }

        /// <summary>
        /// Test implementation of the service
        /// </summary>
        /// <returns>A test server</returns>
        public static AspNetCore.TestHost.TestServer GetTestServer()
        {
            var applicationBasePath = System.AppContext.BaseDirectory;
            var builder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseContentRoot(applicationBasePath)
                .UseStartup<Startup>();

            var server = new AspNetCore.TestHost.TestServer(builder);

            return server;
        }
    }
}
