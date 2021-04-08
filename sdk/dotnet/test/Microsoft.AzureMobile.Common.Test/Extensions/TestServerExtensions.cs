// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Server;
using Microsoft.AzureMobile.Server.InMemory;

namespace Microsoft.AzureMobile.Common.Test.Extensions
{
    /// <summary>
    /// A set of extension methods that make it easier to send unit tests to the test server.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Part of the test suite")]
    public static class TestServerExtensions
    {
        /// <summary>
        /// The URI to assume for the <see cref="TestServer"/> instance.
        /// </summary>
        private static Uri ServerUri { get; } = new Uri("https://localhost");

        /// <summary>
        /// The client uses System.Text.Json to deserialize content from the server.  This
        /// is the required <see cref="JsonSerializerOptions"/> for deserialization.
        /// </summary>
        private static JsonSerializerOptions SerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Gets a reference to the underlying <see cref="IRepository{TEntity}"/> registered within the test service.
        /// </summary>
        /// <typeparam name="T">The type of entity stored in the repository</typeparam>
        /// <param name="server"></param>
        /// <returns>The <see cref="IRepository{TEntity}"/> for the provided entity type</returns>
        /// <exception cref="InvalidOperationException">if no repository can be found for the provided type.</exception>
        public static InMemoryRepository<T> GetRepository<T>(this TestServer server) where T : InMemoryTableData
        {
            object service = server.Services.GetService(typeof(IRepository<T>));
            if (service == null)
            {
                throw new InvalidOperationException($"Service for type IRepository<{typeof(T).Name}> not found");
            }
            return (InMemoryRepository<T>)service;
        }

        /// <summary>
        /// Sends a request to the remote server with no body content.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="method">The <see cref="HttpMethod"/> to use</param>
        /// <param name="relativeUri">The relative Uri of the request</param>
        /// <param name="headers">Any additional headers to send</param>
        /// <returns>The response from the server</returns>
        public static Task<HttpResponseMessage> SendRequest(this TestServer server, HttpMethod method, string relativeUri, Dictionary<string, string> headers = null)
        {
            var client = server.CreateClient();
            var request = new HttpRequestMessage { Method = method, RequestUri = new Uri(ServerUri, relativeUri) };
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }

            // Auto-add the X-ZUMO-Version header if we don't already have one.
            if (!request.Headers.Contains("X-ZUMO-Version"))
            {
                request.Headers.Add("X-ZUMO-Version", "3.0");
            }
            return client.SendAsync(request);
        }

        /// <summary>
        /// Sends a request to the remote server with no body content.
        /// </summary>
        /// <typeparam name="T">The type of the content</typeparam>
        /// <param name="server"></param>
        /// <param name="method">The <see cref="HttpMethod"/> to use</param>
        /// <param name="relativeUri">The relative Uri of the request</param>
        /// <param name="content">The payload of the request</param>
        /// <param name="headers">Any additional headers to send</param>
        /// <returns>The response from the server</returns>
        public static Task<HttpResponseMessage> SendRequest<T>(this TestServer server, HttpMethod method, string relativeUri, T content, Dictionary<string, string> headers = null) where T : class
            => SendRequest<T>(server, method, relativeUri, content, "application/json", headers);

        /// <summary>
        /// Sends a request to the remote server with no body content.
        /// </summary>
        /// <typeparam name="T">The type of the content</typeparam>
        /// <param name="server"></param>
        /// <param name="method">The <see cref="HttpMethod"/> to use</param>
        /// <param name="relativeUri">The relative Uri of the request</param>
        /// <param name="content">The payload of the request</param>
        /// <param name="contentType">The MIME content type of the request</param>
        /// <param name="headers">Any additional headers to send</param>
        /// <returns>The response from the server</returns>
        public static Task<HttpResponseMessage> SendRequest<T>(this TestServer server, HttpMethod method, string relativeUri, T content, string contentType, Dictionary<string, string> headers = null) where T : class
        {
            var client = server.CreateClient();
            var request = new HttpRequestMessage { Method = method, RequestUri = new Uri(ServerUri, relativeUri) };
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }

            // Auto-add the X-ZUMO-Version header if we don't already have one.
            if (!request.Headers.Contains("X-ZUMO-Version"))
            {
                request.Headers.Add("X-ZUMO-Version", "3.0");
            }

            var payload = JsonSerializer.Serialize(content, SerializerOptions);
            request.Content = new StringContent(payload, Encoding.UTF8, contentType);
            return client.SendAsync(request);
        }

        /// <summary>
        /// Send a patch request to the remote server.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="relativeUri"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> SendPatch(this TestServer server, string relativeUri, IEnumerable<PatchOperation> content, string contentType = "application/json-patch+json", Dictionary<string, string> headers = null)
        {
            var client = server.CreateClient();
            var request = new HttpRequestMessage { Method = HttpMethod.Patch, RequestUri = new Uri(ServerUri, relativeUri) };
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }

            // Auto-add the X-ZUMO-Version header if we don't already have one.
            if (!request.Headers.Contains("X-ZUMO-Version"))
            {
                request.Headers.Add("X-ZUMO-Version", "3.0");
            }

            var payload = JsonSerializer.Serialize(content, SerializerOptions);
            request.Content = new StringContent(payload, Encoding.UTF8, contentType);
            return client.SendAsync(request);
        }

        /// <summary>
        /// Alternate form of the patch request with headers and no content type.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="relativeUri"></param>
        /// <param name="content"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> SendPatch(this TestServer server, string relativeUri, IEnumerable<PatchOperation> content, Dictionary<string, string> headers = null)
            => SendPatch(server, relativeUri, content, "application/json-patch+json", headers);

        /// <summary>
        /// Alternate form of the patch request with no headers or content type.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="relativeUri"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> SendPatch(this TestServer server, string relativeUri, IEnumerable<PatchOperation> content)
            => SendPatch(server, relativeUri, content, "application/json-patch+json", null);
    }
}
