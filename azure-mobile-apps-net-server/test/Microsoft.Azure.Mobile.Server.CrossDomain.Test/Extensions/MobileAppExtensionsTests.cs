// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Controllers;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.CrossDomain.Test.Extensions
{
    public class MobileAppExtensionsTests
    {
        [Fact]
        public void MapLegacyCrossDomainController_SetsCrossDomainOrigins()
        {
            // Arrange
            CrossDomainController.Reset();

            var origins = new[] { "a", "b" };
            HttpConfiguration config = new HttpConfiguration();

            // Act
            new MobileAppConfiguration()
                .MapLegacyCrossDomainController(origins)
                .ApplyTo(config);

            // Assert
            var actual = config.GetCrossDomainOrigins();
            Assert.Same(origins, actual);
        }

        [Fact]
        public async Task MapLegacyCrossDomainController_MapsRoutesCorrectly()
        {
            // Arrange
            CrossDomainController.Reset();

            TestServer server = TestServer.Create(app =>
            {
                HttpConfiguration config = new HttpConfiguration();
                new MobileAppConfiguration()
                    .MapApiControllers()
                    .MapLegacyCrossDomainController(new[] { "testorigin" })
                    .ApplyTo(config);

                app.UseWebApi(config);
            });

            HttpClient client = server.HttpClient;

            // Act
            var getBridge = await client.GetAsync("crossdomain/bridge?origin=testorigin");
            var getLoginReceiver = await client.GetAsync("crossdomain/loginreceiver?completion_origin=testorigin");

            var getBridgeApi = await client.GetAsync("api/crossdomain/bridge?origin=testorigin");
            var getLoginReceiverApi = await client.GetAsync("api/crossdomain/loginreceiver?completion_origin=testorigin");

            // Assert
            Assert.Equal(HttpStatusCode.OK, getBridge.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getLoginReceiver.StatusCode);

            // api routes should not be found
            Assert.Equal(HttpStatusCode.NotFound, getBridgeApi.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getLoginReceiverApi.StatusCode);
        }
    }
}