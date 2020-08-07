// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using System.Text.Json;

namespace Microsoft.Zumo.MobileData
{
    public class MobileTableClientOptions : ClientOptions
    {
        /// <summary>
        /// The SerializerOptions used by the serialization library.  The default is
        /// normally good, unless you adjust on the service too.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }
}
