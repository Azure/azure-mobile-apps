// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions.Converters;
using NetTopologySuite.IO.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.Abstractions;

public class DatasyncServiceOptions : IDatasyncServiceOptions
{
    private readonly Lazy<JsonSerializerOptions> _options;

    public DatasyncServiceOptions()
    {
        _options = new Lazy<JsonSerializerOptions>(() => GetJsonSerializerOptions());
    }

    /// <inheritdoc />
    public JsonSerializerOptions JsonSerializerOptions => _options.Value;

    private static JsonSerializerOptions GetJsonSerializerOptions() => new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        Converters =
            {
                new JsonStringEnumConverter(),
                new DateTimeOffsetConverter(),
                new DateTimeConverter(),
                new TimeOnlyConverter(),
                new GeoJsonConverterFactory()
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
