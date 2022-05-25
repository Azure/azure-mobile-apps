// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// A default implementation of <see cref="IMobileAppControllerConfigProvider" /> that configures the JSON
    /// serializer settings for use with Mobile App clients.
    /// </summary>
    public class MobileAppControllerConfigProvider : IMobileAppControllerConfigProvider
    {
        /// <inheritdoc />
        public virtual void Configure(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerSettings == null)
            {
                throw new ArgumentNullException("controllerSettings");
            }

            JsonMediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            JsonSerializerSettings serializerSettings = jsonFormatter.SerializerSettings;

            // Set up date/time format to be ISO 8601 but with 3 digits and "Z" as UTC time indicator. This format
            // is the JS-valid format accepted by most JS clients.
            IsoDateTimeConverter dateTimeConverter = new IsoDateTimeConverter()
            {
                Culture = CultureInfo.InvariantCulture,
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFZ",
                DateTimeStyles = DateTimeStyles.AdjustToUniversal
            };

            // Ignoring default values while serializing was affecting offline scenarios as client sdk looks at first object in a batch for the properties.
            // If first row in the server response did not include columns with default values, client sdk ignores these columns for the rest of the rows
            serializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
            serializerSettings.NullValueHandling = NullValueHandling.Include;
            serializerSettings.Converters.Add(new StringEnumConverter());
            serializerSettings.Converters.Add(dateTimeConverter);
            serializerSettings.MissingMemberHandling = MissingMemberHandling.Error;
            serializerSettings.CheckAdditionalContent = true;
            serializerSettings.ContractResolver = new ServiceContractResolver(jsonFormatter);
            controllerSettings.Formatters.Remove(controllerSettings.Formatters.JsonFormatter);
            controllerSettings.Formatters.Insert(0, jsonFormatter);
        }
    }
}