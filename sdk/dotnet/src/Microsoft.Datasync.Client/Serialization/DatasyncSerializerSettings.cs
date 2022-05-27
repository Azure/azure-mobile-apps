// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Serialization
{
    /// <summary>
    /// JSON Serializer Settings to use with a <see cref="ServiceSerializer"/>.
    /// </summary>
    public class DatasyncSerializerSettings : JsonSerializerSettings
    {
        /// <summary>
        /// Creates a new <see cref="DatasyncSerializerSettings"/> instance
        /// with the default properties necessary for a datasync service.
        /// </summary>
        public DatasyncSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Include;
            ObjectCreationHandling = ObjectCreationHandling.Replace;

            ContractResolver = new DatasyncContractResolver();

            Converters.Add(new DatasyncIsoDateTimeConverter());
            Converters.Add(new StringEnumConverter());
        }

        /// <summary>
        /// Indicates if the property names should be camel-cased when
        /// serialized into JSON.
        /// </summary>
        public bool CamelCasePropertyNames
        {
            get => ContractResolver.CamelCasePropertyNames;
            set => ContractResolver.CamelCasePropertyNames = value;
        }

        /// <summary>
        /// The contract resolver to use.
        /// </summary>
        /// <remarks>
        /// The normal contract resolver requires that the object implement <c>IContractResolver</c>,
        /// but we require a <see cref="DatasyncContractResolver"/>, so we must check for this.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        public new DatasyncContractResolver ContractResolver
        {
            get
            {
                if (base.ContractResolver is not DatasyncContractResolver resolver)
                {
                    throw new InvalidOperationException($"The '{GetType().FullName}.ContractResolver' must be set to a '{typeof(DatasyncContractResolver).FullName}' instance.");
                }
                return resolver;
            }

            set
            {
                base.ContractResolver = value ?? throw new InvalidOperationException($"The '{GetType().FullName}.ContractResolver' must be set to a '{typeof(DatasyncContractResolver).FullName}' instance.");
            }
        }

        /// <summary>
        /// Returns a <see cref="JsonSerializer"/> with the equivalent settings as this object.
        /// </summary>
        /// <returns>A <see cref="JsonSerializer"/> that is configured with these settings.</returns>
        internal JsonSerializer GetSerializerFromSettings() => JsonSerializer.Create(this);
    }
}