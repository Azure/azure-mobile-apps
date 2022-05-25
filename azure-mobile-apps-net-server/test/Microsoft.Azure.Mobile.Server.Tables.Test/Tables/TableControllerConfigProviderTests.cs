// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.Serialization;
using Microsoft.Azure.Mobile.Server.Tables;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class TableControllerConfigProviderTests
    {
        [Fact]
        public void TableConfigProvider_SettingsAreCorrect()
        {
            // Arrange
            var config = new HttpConfiguration();
            var configProvider = new TableControllerConfigProvider();
            var settings = new HttpControllerSettings(config);
            var descriptor = new HttpControllerDescriptor()
            {
                Configuration = config
            };

            // Act
            configProvider.Configure(settings, descriptor);

            // Assert
            // Verify SerializerSettings are set up as we expect
            Assert.Null(settings.Formatters.XmlFormatter);
            var filterProvider = config.Services.GetFilterProviders().OfType<TableFilterProvider>();
            Assert.NotNull(filterProvider);
            Assert.IsType<TableContractResolver>(settings.Formatters.JsonFormatter.SerializerSettings.ContractResolver);
        }
    }
}