// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.Models;

internal class DatasyncServiceOptions : IDatasyncServiceOptions
{
    private readonly Lazy<JsonSerializerOptions> _options;

    public DatasyncServiceOptions()
    {
        _options = new Lazy<JsonSerializerOptions>(() => GetJsonSerializerOptions());
    }

    /// <inheritdoc />
    public JsonSerializerOptions JsonSerializerOptions => _options.Value;

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
        {
            AllowTrailingCommas = true,
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
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new DateTimeOffsetConverter());
        options.Converters.Add(new DateTimeConverter());
        options.Converters.Add(new TimeOnlyConverter());
        return options;
    }
}
