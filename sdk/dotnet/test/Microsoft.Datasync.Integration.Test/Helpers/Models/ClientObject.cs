// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Integration.Test.Helpers
{
    /// <summary>
    /// Deserialize everything into the Data object.
    /// </summary>
    public class ClientObject
    {
        [JsonExtensionData]
        public Dictionary<string, object>? Data { get; set; }
    }
}
