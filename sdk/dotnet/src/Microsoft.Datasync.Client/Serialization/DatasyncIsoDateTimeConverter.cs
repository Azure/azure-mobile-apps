// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// Converts <see cref="DateTime"/> and <see cref="DateTimeOffset"/> objects into
    /// a UTC ISO string represetnation with millisecond accuracy.
    /// </summary>
    public class DatasyncIsoDateTimeConverter : IsoDateTimeConverter
    {
        internal const string IsoDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK";

        /// <summary>
        /// Creates a new <see cref="DatasyncIsoDateTimeConverter"/> instance.
        /// </summary>
        public DatasyncIsoDateTimeConverter()
        {
            Culture = CultureInfo.InvariantCulture;
            DateTimeFormat = IsoDateTimeFormat;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object datetimeObject = base.ReadJson(reader, objectType, existingValue, serializer);

            if (datetimeObject != null)
            {
                if (datetimeObject is DateTime dt)
                {
                    return dt.ToLocalTime();
                }
                else if (datetimeObject is DateTimeOffset dto)
                {
                    return dto.ToLocalTime();
                }
            }

            return datetimeObject;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dateTime;
            if (value is DateTime time)
            {
                dateTime = time.ToUniversalTime();
            }
            else
            {
                dateTime = ((DateTimeOffset)value).UtcDateTime;
            }

            base.WriteJson(writer, dateTime, serializer);
        }
    }
}