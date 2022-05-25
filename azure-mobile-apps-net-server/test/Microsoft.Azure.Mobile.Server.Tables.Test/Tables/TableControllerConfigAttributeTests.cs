// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.Config;
using Moq;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class TableControllerConfigAttributeTests
    {
        [Fact]
        public void Initialize_Calls_MobileAppControllerConfigProvider_Then_TableControllerConfigProvider()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            HttpControllerSettings settings = new HttpControllerSettings(config);
            HttpControllerDescriptor descriptor = new HttpControllerDescriptor()
            {
                Configuration = config
            };

            string output = string.Empty;

            Mock<IMobileAppControllerConfigProvider> configProviderMock = new Mock<IMobileAppControllerConfigProvider>();
            configProviderMock.Setup(p => p.Configure(settings, descriptor)).Callback(() => output += "1");
            config.SetMobileAppControllerConfigProvider(configProviderMock.Object);

            Mock<ITableControllerConfigProvider> tableConfigProviderMock = new Mock<ITableControllerConfigProvider>();
            tableConfigProviderMock.Setup(p => p.Configure(settings, descriptor)).Callback(() => output += "2");
            config.SetTableControllerConfigProvider(tableConfigProviderMock.Object);

            // Act
            new TableControllerConfigAttribute().Initialize(settings, descriptor);

            // Assert
            configProviderMock.VerifyAll();
            tableConfigProviderMock.VerifyAll();
            Assert.Equal("12", output);
        }
    }
}