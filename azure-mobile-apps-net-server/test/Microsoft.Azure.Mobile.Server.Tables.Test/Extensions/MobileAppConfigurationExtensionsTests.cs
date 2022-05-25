// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Owin.Testing;
using Moq;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Extensions
{
    public class MobileAppConfigurationExtensionsTests
    {
        [Fact]
        public void WithTableControllerConfigProvider_AddsProvider()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            var provider = new Mock<ITableControllerConfigProvider>();

            // Act
            new MobileAppConfiguration()
                .WithTableControllerConfigProvider(provider.Object)
                .ApplyTo(config);

            // Assert
            Assert.Same(provider.Object, config.GetTableControllerConfigProvider());
        }

        [Fact]
        public async Task WithTableControllerConfigProvider_RegistersProviderCorrectly()
        {
            // Arrange
            string output = string.Empty;
            var providerMock = new Mock<IMobileAppControllerConfigProvider>();
            providerMock.Setup(p => p.Configure(It.IsAny<HttpControllerSettings>(), It.IsAny<HttpControllerDescriptor>()))
                .Callback(() => output += "1");
            var tableProviderMock = new Mock<ITableControllerConfigProvider>();
            tableProviderMock.Setup(p => p.Configure(It.IsAny<HttpControllerSettings>(), It.IsAny<HttpControllerDescriptor>()))
                .Callback(() => output += "2");

            HttpConfiguration config = new HttpConfiguration();

            var server = TestServer.Create(app =>
            {
                // mock out the controller discovery to only find these
                var controllerTypesToReturn = new[]
                {
                    typeof(MyTable1Controller),
                    typeof(MyTable2Controller)
                };

                SetupMockControllerResolver(config, controllerTypesToReturn);

                new MobileAppConfiguration()
                    .WithTableControllerConfigProvider(tableProviderMock.Object)
                    .WithMobileAppControllerConfigProvider(providerMock.Object)
                    .AddTables()
                    .ApplyTo(config);

                app.UseWebApi(config);
            });

            var client = server.HttpClient;
            client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");

            // Act
            // issue a request to initialize the config
            var result = await client.GetAsync("tables/mytable1");

            // Assert
            Assert.Same(providerMock.Object, config.GetMobileAppControllerConfigProvider());
            Assert.Same(tableProviderMock.Object, config.GetTableControllerConfigProvider());
            // each provider should be called once for each TableController, in order.
            Assert.Equal("1212", output);
        }

        [Fact]
        public async Task AddTables_MapsRoutesCorrectly()
        {
            // Arrange
            TestServer server = TestServer.Create(app =>
            {
                HttpConfiguration config = new HttpConfiguration();

                // mock out the controller discovery to only find these
                var controllerTypesToReturn = new[]
                {
                    typeof(MyCustomController),
                    typeof(MyTable1Controller),
                    typeof(MyTable2Controller)
                };

                SetupMockControllerResolver(config, controllerTypesToReturn);

                new MobileAppConfiguration()
                    .MapApiControllers()
                    .AddTables()
                    .ApplyTo(config);

                app.UseWebApi(config);
            });

            HttpClient client = server.HttpClient;
            client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");

            // Act
            var getTable1 = await client.GetAsync("tables/mytable1");
            var getTable2 = await client.GetAsync("tables/mytable2");
            var getApi = await client.GetAsync("api/mycustom");

            var getTable1AsApi = await client.GetAsync("api/mytable1");
            var getTable2AsApi = await client.GetAsync("api/mytable2");
            var getApiAsTable = await client.GetAsync("tables/mycustom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, getTable1.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getTable2.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getApi.StatusCode);

            // api routes should not be found
            Assert.Equal(HttpStatusCode.NotFound, getTable1AsApi.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getTable2AsApi.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getApiAsTable.StatusCode);
        }

        private static void SetupMockControllerResolver(HttpConfiguration config, ICollection<Type> controllerTypesToReturn)
        {
            IAssembliesResolver assemblyResolver = config.Services.GetAssembliesResolver();
            Mock<IHttpControllerTypeResolver> typeResolverMock = new Mock<IHttpControllerTypeResolver>();
            typeResolverMock.Setup(m => m.GetControllerTypes(assemblyResolver)).Returns(controllerTypesToReturn);

            config.Services.Replace(typeof(IHttpControllerTypeResolver), typeResolverMock.Object);
        }

        [MobileAppController]
        private class MyCustomController : ApiController
        {
            public IHttpActionResult Get()
            {
                return this.Ok();
            }
        }

        private class MyTable1Controller : TableController
        {
            public IHttpActionResult Get()
            {
                return this.Ok();
            }
        }

        private class MyTable2Controller : TableController
        {
            public IHttpActionResult Get()
            {
                return this.Ok();
            }
        }
    }
}