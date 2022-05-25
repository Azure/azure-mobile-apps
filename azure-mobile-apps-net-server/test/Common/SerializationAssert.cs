// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile
{
    public static class SerializationAssert
    {
        public static void VerifySerialization<T>(T instance, string expected, IEnumerable<string> excludeProperties = null)
            where T : class
        {
            // Arrange
            // The default config does not set up a global formatter. Instead, we use the
            // MobileAppControllerConfigAttribute to set up per-controller formatter settings.
            var config = new HttpConfiguration();
            var controllerSettings = new HttpControllerSettings(config);
            var mobileAppControllerConfig = new MobileAppControllerAttribute();
            var controllerDescriptor = new HttpControllerDescriptor()
            {
                Configuration = config
            };
            mobileAppControllerConfig.Initialize(controllerSettings, controllerDescriptor);

            JsonMediaTypeFormatter jsonFormatter = controllerSettings.Formatters.JsonFormatter;
            JsonSerializerSettings settings = jsonFormatter.SerializerSettings;

            // Act/Assert
            VerifySerialization(instance, settings, expected, excludeProperties);
        }

        public static void VerifySerialization<T>(T instance, JsonSerializerSettings settings, string expected, IEnumerable<string> excludeProperties = null)
            where T : class
        {
            // Act
            string actual = JsonConvert.SerializeObject(instance, settings);

            // Act/Assert
            PropertyAssert.PublicPropertiesAreSet(instance, excludeProperties);
            Assert.Equal(expected, actual);
        }
    }
}