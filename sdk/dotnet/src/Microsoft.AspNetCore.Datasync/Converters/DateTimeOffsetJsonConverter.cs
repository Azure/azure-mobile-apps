// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync.Converters
{
    /// <summary>
    /// A converter for the DateTimeOffset type.
    /// </summary>
    public class DateTimeOffsetJsonConverter : JsonConverter
    {
        /// <summary>
        /// This is the interchange format for the DateTimeOffset.
        /// </summary>
        private const string format = "yyyy-MM-dd'T'HH:mm:ss.fffK";

        /// <summary>
        /// Can this class convert the specified type?
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns>True if the type is handled by this class</returns>
        public override bool CanConvert(Type objectType)
            => objectType.IsAssignableFrom(typeof(DateTimeOffset));

        /// <summary>
        /// Can this class handle reading JSON values?
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// Can this class handle writing JSON values?
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Read a value from JSON and convert it to the <see cref="DateTimeOffset"/> value
        /// </summary>
        [SuppressMessage("General", "RCS1079:Throwing of new NotImplementedException.", Justification = "Only valid in write scenario.")]
        [ExcludeFromCodeCoverage]
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write the provided value to JSON format.
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTimeOffset)value).ToUniversalTime().ToString(format));
        }
    }
}
