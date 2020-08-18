// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    public abstract class BaseTest
    {
        public HttpResponseMessage CreateResponse<T>(HttpStatusCode code, T obj)
        {
            var json = JsonSerializer.Serialize<T>(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var response = new HttpResponseMessage
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                StatusCode = code
            };
            return response;
        }
    }
}
