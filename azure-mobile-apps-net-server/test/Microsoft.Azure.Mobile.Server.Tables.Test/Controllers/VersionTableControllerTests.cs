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
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    public class TableVersionTests
    {
        private const string ZumoAPIVersion = "ZUMO-API-VERSION";
        private const string CurrentZumoAPIVersion = "2.0.0";

        [Fact]
        public async Task NoVersion_Errors()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "tables/testtable");
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task NoVersion_SkipIsSet_Success()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "tables/testtable");
            HttpResponseMessage response = await this.CreateTestServer(true).HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CorrectVersion_Header_Success()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "tables/testtable");
            request.Headers.Add(ZumoAPIVersion, CurrentZumoAPIVersion);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CorrectVersion_QueryString_Success()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "tables/testtable?" + ZumoAPIVersion + "=" + CurrentZumoAPIVersion);
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private TestServer CreateTestServer(bool skipVersionCheck = false)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            new MobileAppConfiguration()
                .AddTables()
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
