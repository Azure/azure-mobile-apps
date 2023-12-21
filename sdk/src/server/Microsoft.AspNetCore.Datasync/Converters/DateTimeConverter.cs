// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.Converters;

/// <summary>
/// A specialized converter for <see cref="DateTime"/> that handles the specific requirements of the
/// Datasync protocol.
/// </summary>
internal class DateTimeConverter : JsonConverter<DateTime>
{
    private const string format = "yyyy-MM-dd'T'HH:mm:ss.fffK";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString(format));
    }
}
