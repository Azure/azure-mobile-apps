// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Datasync.Common.Models;

[ExcludeFromCodeCoverage]
public class ClientObject
{
    [JsonExtensionData]
    public Dictionary<string, object> Data { get; set; }
}
