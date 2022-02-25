// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// An implementation of <see cref="JsonConverter"/> to be used with value type
    /// properties to ensure that null values in a JSON payload are deserialized as
    /// the default value for the value type.
    /// </summary>
    internal class NullHandlingConverter : JsonConverter
    {
        private static readonly Lazy<NullHandlingConverter> _instance = new(() => new NullHandlingConverter());
        private readonly JsonConverter _inner;

        /// <summary>
        /// A singleton instance of the <see cref="NullHandlingConverter"/>.
        /// </summary>
        public static NullHandlingConverter Instance { get => _instance.Value; }

        /// <summary>
        /// Creates a new <see cref="JsonConverter"/> that handles nulls as default values.
        /// </summary>
        /// <param name="inner">The inner converter (if any).</param>
        public NullHandlingConverter(JsonConverter inner = null)
        {
            _inner = inner;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
            => objectType.GetTypeInfo().IsValueType || (_inner != null && _inner.CanConvert(objectType));

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (_inner?.CanConvert(objectType) == true && _inner?.CanRead == true)
            {
                return _inner.ReadJson(reader, objectType, existingValue, serializer);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                return objectType.GetTypeInfo().IsValueType ? Activator.CreateInstance(objectType) : null;
            }
            else
            {
                return serializer.Deserialize(reader, objectType);
            }
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (_inner?.CanWrite == true)
            {
                _inner.WriteJson(writer, value, serializer);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }
    }
}
