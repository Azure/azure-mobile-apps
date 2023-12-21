// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.Converters;

/// <summary>
/// A specialized converter for <see cref="DateTimeOffset"/> that handles the specific requirements of the
/// Datasync protocol.
/// </summary>
internal class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    private const string format = "yyyy-MM-dd'T'HH:mm:ss.fffZ";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTimeOffset.Parse(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString(format));
    }
}
