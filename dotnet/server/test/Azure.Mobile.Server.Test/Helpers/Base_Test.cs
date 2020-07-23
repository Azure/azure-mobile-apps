﻿using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.Helpers
{
    /// <summary>
    /// A set of basic helper methods for the tests.
    /// </summary>
    public abstract class Base_Test
    {
        /// <summary>
        /// The base URI for the request
        /// </summary>
        private Uri BaseUri = new Uri("https://localhost");

        /// <summary>
        /// The JSON Serializer options to use
        /// </summary>
        private JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Send a request to the server, receiving the response
        /// </summary>
        /// <typeparam name="T">The type for the content</typeparam>
        /// <param name="server">The server to send the request to</param>
        /// <param name="method">The HTTP Method</param>
        /// <param name="relativePath">The path for the requet</param>
        /// <param name="content">The typed content</param>
        /// <param name="additionalHeaders">Any additional headers</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SendRequestToServer<T>(
            TestServer server, 
            HttpMethod method,
            string relativePath,
            T content,
            Dictionary<string, string> additionalHeaders = null) where T : class
        {
            var client = server.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(BaseUri, relativePath)
            };

            if (content != null)
            {
                var json = JsonSerializer.Serialize<T>(content, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (additionalHeaders != null)
            {
                foreach (var kv in additionalHeaders)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }

            return client.SendAsync(request); 
        }

        public async Task<T> GetValueFromResponse<T>(HttpResponseMessage response) where T : class
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return result;
        }
    }
}
