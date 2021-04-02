﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.AzureMobile.Common.Test.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// The client uses System.Text.Json to deserialize content from the server.  This
        /// is the required <see cref="JsonSerializerOptions"/> for deserialization.
        /// </summary>
        private static JsonSerializerOptions SerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Deserialize the content asynchronously.
        /// </summary>
        /// <typeparam name="T">The expected type of the response</typeparam>
        /// <param name="response">The response object</param>
        /// <returns>The object</returns>
        public static async Task<T> DeserializeContentAsync<T>(this HttpResponseMessage response) where T : class
        {
            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<T>(payload, SerializerOptions);
            return result;
        }

        /// <summary>
        /// Deserialize the content synchronously.
        /// </summary>
        /// <typeparam name="T">The expected type of the response</typeparam>
        /// <param name="response">The response object</param>
        /// <returns>The object</returns>
        public static T DeserializeContent<T>(this HttpResponseMessage response) where T : class
        {
            var payload = response.Content.ReadAsStringAsync().Result;
            var result = JsonSerializer.Deserialize<T>(payload, SerializerOptions);
            return result;
        }
    }
}
