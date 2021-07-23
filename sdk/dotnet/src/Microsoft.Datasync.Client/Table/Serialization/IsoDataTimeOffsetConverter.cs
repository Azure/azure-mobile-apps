// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client.Table.Serialization
{
    /// <summary>
    /// Serializer conversion for date/time entities.
    /// </summary>
    public class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffZ";

        /// <summary>
        /// Read a <see cref="DateTimeOffset"/> from the JSON stream.
        /// </summary>
        /// <param name="reader">The JSON Reader</param>
        /// <param name="typeToConvert">The type to convert</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/></param>
        /// <returns>The value of the field</returns>
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateTimeOffset.Parse(reader.GetString(), CultureInfo.InvariantCulture).ToLocalTime();

        /// <summary>
        /// Write a <see cref="DateTimeOffset"/> to the JSON stream
        /// </summary>
        /// <param name="writer">The JSON Writer</param>
        /// <param name="value">The value to write</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/></param>
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToUniversalTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture));
    }
}
