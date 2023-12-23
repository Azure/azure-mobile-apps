// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions.Converters;
using NetTopologySuite.IO.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.Extensions;

internal static class StdLibExtensions
{
    private static readonly Lazy<JsonSerializerOptions> _options = new(() => GetSerializerOptions());

    /// <summary>
    /// Serializes an object to a JSON string.  This is used in logging, so we capture any
    /// exceptions and return a default string.
    /// </summary>
    /// <param name="object">The object to be serialized.</param>
    /// <returns>The serialized object.</returns>
    internal static string ToJsonString(this object @object)
    {
        try
        {
            if (@object == null)
            {
                return "null";
            }
            return JsonSerializer.Serialize(@object, _options.Value);
        }
        catch (Exception)
        {
            return "unserializable object";
        }
    }

    /// <summary>
    /// Gets an appropriate set of serializer options for the logging JSON content.
    /// </summary>
    /// <returns>The <see cref="JsonSerializerOptions"/> to use for logging.</returns>
    private static JsonSerializerOptions GetSerializerOptions() => new(JsonSerializerDefaults.General)
    {
        Converters =
            {
                new JsonStringEnumConverter(),
                new DateTimeOffsetConverter(),
                new DateTimeConverter(),
                new TimeOnlyConverter(),
                new GeoJsonConverterFactory()
            },
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        ReferenceHandler = ReferenceHandler.Preserve
    };

    /// <summary>
    /// Converts a byte array to an entity tag value.
    /// </summary>
    /// <param name="version">The version to convert.</param>
    /// <returns>The version string.</returns>
    internal static string ToEntityTagValue(this byte[] @version)
        => Convert.ToBase64String(@version);
}
