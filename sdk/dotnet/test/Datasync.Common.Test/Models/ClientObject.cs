// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Datasync.Common.Test.Models
{
    /// <summary>
    /// Deserialize everything into the Data object.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ClientObject
    {
        [JsonExtensionData]
        public Dictionary<string, object> Data { get; set; }
    }
}
