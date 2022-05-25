// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin.Testing;
using Owin;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    public class CrossDomainControllerTests
    {
        private TestServer corsServer;
        private IEnumerable<string> allowedOrigins = new List<string> { "http://testhost", "http://sample" };

        public CrossDomainControllerTests()
        {
            CrossDomainController.Reset();
            this.corsServer = this.CreateTestServer(this.allowedOrigins);
        }

        public static TheoryDataCollection<string[]> OriginsData
        {
            get
            {
                return new TheoryDataCollection<string[]>
                {
                    { new string[] { "http://A", "https://B", "http://C" } },
                    { new string[] { "http://你好.com", "http://世界.com" } },
                    { new string[] { "http://testhost", "http://sample" } }
                };
            }
        }

        [Fact]
        public async Task LoginReceiver_ReturnsUnauthorized_IfNoOriginsSet()
        {
            // Arrange
            TestServer server = this.CreateTestServer(null);

            // Act
            HttpResponseMessage response = await server.HttpClient.GetAsync("crossdomain/loginreceiver?completion_origin=http://sample");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task LoginReceiver_ReturnsOk_IfKnown()
        {
            // Act
            HttpResponseMessage response = await this.corsServer.HttpClient.GetAsync("crossdomain/loginreceiver?completion_origin=http://sample");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task LoginReceiver_ReturnsUnauthorized_IfUnknown()
        {
            // Act
            HttpResponseMessage response = await this.corsServer.HttpClient.GetAsync("crossdomain/loginreceiver?completion_origin=http://unknown");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Bridge_ReturnsOk_IfKnown()
        {
            // Act
            HttpResponseMessage response = await this.corsServer.HttpClient.GetAsync("crossdomain/bridge?origin=http://sample");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Bridge_ReturnsUnauthorized_IfUnknown()
        {
            // Act
            HttpResponseMessage response = await this.corsServer.HttpClient.GetAsync("crossdomain/bridge?origin=http://unknown");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Bridge_ReturnsUnauthorized_IfNoOriginsSet()
        {
            // Arrange
            TestServer server = this.CreateTestServer(null);

            // Act
            HttpResponseMessage response = await this.corsServer.HttpClient.GetAsync("crossdomain/bridge?origin=http://unknown");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [MemberData("OriginsData")]
        public async Task InitializeOrigins_CheckValues_IfEnabledGlobally(IEnumerable<string> origins)
        {
            // Arrange
            HttpConfiguration corsConfig = new HttpConfiguration();
            corsConfig.EnableCors(new EnableCorsAttribute(string.Join(",", origins), "*", "*"));
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(corsConfig);

            // Act
            IEnumerable<string> actual = await CrossDomainController.InitializeOrigins(corsConfig, request);

            // Assert
            Assert.Equal(origins, actual);
        }

        [Fact]
        public async Task InitializeOrigins_SetsCacheToNull_IfEnabledGloballyWithoutOrigins()
        {
            // Arrange
            HttpConfiguration corsConfig = new HttpConfiguration();
            corsConfig.EnableCors();
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(corsConfig);

            // Act
            IEnumerable<string> actual = await CrossDomainController.InitializeOrigins(corsConfig, request);

            // Assert
            //Assert.Equal(origins, actual);
        }

        [Theory]
        [MemberData("OriginsData")]
        public async Task InitializeOrigins_CheckValues_IfPassedDirectly(IEnumerable<string> origins)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            CrossDomainController.Reset();
            new MobileAppConfiguration()
                .MapLegacyCrossDomainController(origins)
                .ApplyTo(config);

            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);

            // Act
            IEnumerable<string> actual = await CrossDomainController.InitializeOrigins(config, request);

            // Assert
            Assert.Equal(origins, actual);
        }

        [Fact]
        public async Task DirectOrigins_Wins_IfGlobalAlsoSet()
        {
            // Arrange
            CrossDomainController.Reset();
            HttpConfiguration config = new HttpConfiguration();

            new MobileAppConfiguration()
                .MapLegacyCrossDomainController(this.allowedOrigins)
                .ApplyTo(config);

            config.EnableCors(new EnableCorsAttribute("http://notused", "*", "*"));

            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);

            // Act
            IEnumerable<string> actual = await CrossDomainController.InitializeOrigins(config, request);

            // Assert
            Assert.Equal(this.allowedOrigins, actual);
        }

        [Fact]
        public async Task OriginsCache_Null_IfNoOriginsSet()
        {
            // Arrange
            CrossDomainController.Reset();
            HttpConfiguration config = new HttpConfiguration();
            new MobileAppConfiguration()
                .MapLegacyCrossDomainController()
                .ApplyTo(config);

            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);

            // Act
            IEnumerable<string> actual = await CrossDomainController.InitializeOrigins(config, request);

            // Assert
            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        [Fact]
        public async Task InitializeOrigins_ReturnsCachedList()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            config.EnableCors(new EnableCorsAttribute("http://test", "*", "*"));
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);

            // Act
            IEnumerable<string> actual1 = await CrossDomainController.InitializeOrigins(config, new HttpRequestMessage());
            IEnumerable<string> actual2 = await CrossDomainController.InitializeOrigins(config, new HttpRequestMessage());

            // Assert
            Assert.Same(actual1, actual2);
        }

        private TestServer CreateTestServer(IEnumerable<string> origins)
        {
            HttpConfiguration config = new HttpConfiguration();
            MobileAppConfiguration mobileConfig = new MobileAppConfiguration();
            CrossDomainController.Reset();

            if (origins == null)
            {
                mobileConfig = mobileConfig.MapLegacyCrossDomainController();
            }
            else
            {
                mobileConfig = mobileConfig.MapLegacyCrossDomainController(origins);
            }

            mobileConfig.ApplyTo(config);

            return TestServer.Create(appBuilder =>
            {
                appBuilder.UseWebApi(config);
            });
        }
    }
}