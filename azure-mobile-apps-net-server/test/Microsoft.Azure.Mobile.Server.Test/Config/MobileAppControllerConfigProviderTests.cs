// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class MobileAppControllerConfigProviderTests
    {
        [Fact]
        public void ConfigProvider_SettingsAreCorrect()
        {
            // Arrange
            var config = new HttpConfiguration();
            var configProvider = new MobileAppControllerConfigProvider();
            var settings = new HttpControllerSettings(config);
            var descriptor = new HttpControllerDescriptor()
            {
                Configuration = config
            };

            // Act
            configProvider.Configure(settings, descriptor);

            // Assert
            // Verify SerializerSettings are set up as we expect
            var serializerSettings = settings.Formatters.JsonFormatter.SerializerSettings;
            Assert.Equal(typeof(ServiceContractResolver), serializerSettings.ContractResolver.GetType());
            Assert.Equal(DefaultValueHandling.Include, serializerSettings.DefaultValueHandling);
            Assert.Equal(NullValueHandling.Include, serializerSettings.NullValueHandling);
            Assert.Equal(MissingMemberHandling.Error, serializerSettings.MissingMemberHandling);
            Assert.True(serializerSettings.CheckAdditionalContent);

            // Verify Converters
            var stringEnumConverter = serializerSettings.Converters.Single(c => c.GetType() == typeof(StringEnumConverter)) as StringEnumConverter;
            Assert.False(stringEnumConverter.CamelCaseText);

            var isoDateTimeConverter = serializerSettings.Converters.Single(c => c.GetType() == typeof(IsoDateTimeConverter)) as IsoDateTimeConverter;
            Assert.Equal(DateTimeStyles.AdjustToUniversal, isoDateTimeConverter.DateTimeStyles);
            Assert.Equal("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFZ", isoDateTimeConverter.DateTimeFormat);
            Assert.Equal(CultureInfo.InvariantCulture, isoDateTimeConverter.Culture);

            Assert.NotSame(config.Formatters.JsonFormatter.SerializerSettings.ContractResolver, settings.Formatters.JsonFormatter.SerializerSettings.ContractResolver);
            Assert.Same(settings.Formatters.JsonFormatter, settings.Formatters[0]);
        }
    }
}