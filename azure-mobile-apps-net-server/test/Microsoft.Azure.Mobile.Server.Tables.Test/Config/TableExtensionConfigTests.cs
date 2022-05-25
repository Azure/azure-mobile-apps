// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.Serialization;
using Microsoft.Azure.Mobile.Server.Tables;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class TableExtensionConfigTests
    {
        private HttpConfiguration config;
        private TableMobileAppExtensionConfig tableExtension;
        private ITableControllerConfigProvider configProvider;

        public TableExtensionConfigTests()
        {
            this.config = new HttpConfiguration();
            this.configProvider = new TableControllerConfigProvider();
            this.tableExtension = new TableMobileAppExtensionConfig(this.configProvider);
        }

        [Fact]
        public void Initialize_RegistersTableControllerConfigProvider()
        {
            // Act
            this.tableExtension.Initialize(this.config);

            // Assert
            Assert.Same(this.configProvider, this.config.GetTableControllerConfigProvider());
        }

        [Fact]
        public void Initialize_ModifiesExistingJsonSerializerSettingsRatherThanReplacingThem()
        {
            // Arrange
            JsonSerializerSettings expected = this.config.Formatters.JsonFormatter.SerializerSettings;

            // Act
            this.tableExtension.Initialize(this.config);

            // Assert
            Assert.Same(expected, this.config.Formatters.JsonFormatter.SerializerSettings);
        }

        [Fact]
        public void Initialize_SetsContractResolver()
        {
            // Arrange
            this.tableExtension.Initialize(this.config);
            var controllerSettings = new HttpControllerSettings(this.config);
            var tableControllerConfig = new TableControllerConfigAttribute();

            // Act
            tableControllerConfig.Initialize(controllerSettings, new HttpControllerDescriptor(this.config, "Dummy", typeof(TableController)));

            // Assert
            Assert.IsType<TableContractResolver>(controllerSettings.Formatters.JsonFormatter.SerializerSettings.ContractResolver);
        }
    }
}
