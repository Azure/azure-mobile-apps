// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// JSON serializer settings to use with a <see cref="MobileServiceClient"/>.
    /// </summary>
    public class MobileServiceJsonSerializerSettings : JsonSerializerSettings
    {
        /// <summary>
        /// Initializes a new instance of the MobileServiceJsonSerializerSettings
        /// class.
        /// </summary>
        public MobileServiceJsonSerializerSettings()
        {
            this.NullValueHandling = NullValueHandling.Include;
            this.ContractResolver = new MobileServiceContractResolver();
            this.ObjectCreationHandling = ObjectCreationHandling.Replace;

            this.Converters.Add(new MobileServiceIsoDateTimeConverter());
            this.Converters.Add(new MobileServicePrecisionCheckConverter());
            this.Converters.Add(new StringEnumConverter());
        }

        /// <summary>
        /// Indicates if the property names should be camel-cased when serialized
        /// out into JSON.
        /// </summary>
        public bool CamelCasePropertyNames
        {
            get
            {
                return ContractResolver.CamelCasePropertyNames;
            }

            set
            {
                ContractResolver.CamelCasePropertyNames = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MobileServiceContractResolver"/> instance.  
        /// </summary>
        public new MobileServiceContractResolver ContractResolver
        {
            get
            {
                // Because we are hiding the base member (which is of type 
                // IContractResolver) it is possible that the base has been 
                // set to an instance of IContractResolver that is not a 
                // MobileServiceContractResolver. Therefore, we must check for 
                // this condition and throw an exception as needed.
                if (!(base.ContractResolver is MobileServiceContractResolver contractResolver))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The member '{0}.ContractResolver' must be set to an instance of the '{1}' class or a class that inherits from the '{1}' class.",
                            this.GetType().FullName,
                            typeof(MobileServiceContractResolver).FullName));
                }

                return contractResolver;
            }

            set
            {
                base.ContractResolver = value ?? throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The member '{0}.ContractResolver' must be set to an instance of the '{1}' class or a class that inherits from the '{1}' class.",
                            this.GetType().FullName,
                            typeof(MobileServiceContractResolver).FullName));
            }
        }

        /// <summary>
        /// Returns a <see cref="JsonSerializer"/> with the equivalent settings
        /// as this <see cref="MobileServiceJsonSerializerSettings"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/> with the equivalent settings
        /// as this <see cref="MobileServiceJsonSerializerSettings"/>.
        /// </returns>
        internal JsonSerializer GetSerializerFromSettings()
        {
            return JsonSerializer.Create(this);
        }
    }
}
