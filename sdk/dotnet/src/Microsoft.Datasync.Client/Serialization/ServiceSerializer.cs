// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// Provides serialization and deserialization for a <see cref="DatasyncClient"/>.
    /// </summary>
    internal class ServiceSerializer
    {
        /// <summary>
        /// The JSON serializer settings to use with this serializer.
        /// </summary>
        public DatasyncSerializerSettings SerializerSettings { get; set; } = new();
    }
}
