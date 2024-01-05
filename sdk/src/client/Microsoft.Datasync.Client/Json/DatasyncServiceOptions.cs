// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Azure.Core.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client.Json;
public static class DatasyncServiceOptions
{
    public static JsonSerializerOptions GetJsonSerializerOptions() => new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        Converters =
            {
                new JsonStringEnumConverter(),
                new DateTimeOffsetConverter(),
                new DateTimeConverter(),
                new TimeOnlyConverter(),
                new MicrosoftSpatialGeoJsonConverter()
            },
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
}
