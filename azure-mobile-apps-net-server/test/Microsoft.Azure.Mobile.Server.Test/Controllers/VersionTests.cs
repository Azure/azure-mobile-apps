// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin.Testing;
using Owin;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    public class VersionTests
    {
        private const string ZumoAPIVersion = "ZUMO-API-VERSION";
        private const string CurrentZumoAPIVersion = "2.0.0";

        public static TheoryDataCollection<string> InvalidVersions
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    "1.0.0",
                    "2.1.0",                
                    "3.0.0",
                    "22.0.0",                    
                    "apples",
                    "2.0.0.0",
                    "2.0",
                    "1",
                    "2"
                };
            }
        }

        public static TheoryDataCollection<string> ValidVersions
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    "2.0.0",
                    "2.0.1",
                    "2.0.77"
                };
            }
        }

        public static TheoryDataCollection<string> ApiVersionCasings
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    "zumo-api-version",
                    "Zumo-API-Version",
                    "zUmO-ApI-VeRsIoN"
                };
            }
        }

        [Fact]
        public async Task NoVersion_Errors()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task NoVersion_SkipIsSet_Success()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            HttpResponseMessage response = await this.CreateTestServer(true).HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData("ValidVersions")]
        public async Task CorrectVersion_Header_Success(string version)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            request.Headers.Add(ZumoAPIVersion, version);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData("ValidVersions")]
        public async Task CorrectVersion_QueryString_Success(string version)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test?" + ZumoAPIVersion + "=" + version);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task QueryStringTrumpsHeader_Success()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test?" + ZumoAPIVersion + "=" + CurrentZumoAPIVersion);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);
            request.Headers.Add(ZumoAPIVersion, "1.0.0");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData("ApiVersionCasings")]
        public async Task CorrectVersionAnyCase_Header_Success(string headerName)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            request.Headers.Add(headerName, CurrentZumoAPIVersion);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData("ApiVersionCasings")]
        public async Task CorrectVersionAnyCase_QueryString_Success(string queryStringName)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test?" + queryStringName + "=" + CurrentZumoAPIVersion);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData("InvalidVersions")]
        public async Task IncorrectVersion_Header_Errors(string version)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            request.Headers.Add(ZumoAPIVersion, version);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [MemberData("InvalidVersions")]
        public async Task IncorrectVersion_QueryString_Errors(string version)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            request.Headers.Add(ZumoAPIVersion, version);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private TestServer CreateTestServer(bool skipVersionCheck = false)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            new MobileAppConfiguration()
                .MapApiControllers()
                .ApplyTo(config);

            IMobileAppSettingsProvider settingsProvider = config.GetMobileAppSettingsProvider();
            var settings = settingsProvider.GetMobileAppSettings();
            settings.SkipVersionCheck = skipVersionCheck;

            return TestServer.Create(appBuilder =>
            {
                appBuilder.UseWebApi(config);
            });
        }
    }
}
