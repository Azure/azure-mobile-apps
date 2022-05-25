// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
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
    public class TableControllerCacheTests
    {
        private HttpClient client;

        public TableControllerCacheTests()
        {
            TestServer server = this.CreateTestServer();
            this.client = server.HttpClient;
        }

        [Fact]
        public async Task TableController_AttributeRoute_AddsCacheHeaders()
        {
            HttpResponseMessage response = await this.client.GetAsync("tables/attribute/mapped");
            VerifyResponse(response);
        }

        [Fact]
        public async Task TableController_RouteTable_AddsCacheHeaders()
        {
            HttpResponseMessage response = await this.client.GetAsync("tables/testtable");
            VerifyResponse(response);
        }

        private static void VerifyResponse(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.CacheControl.NoCache);
            var pragma = response.Headers.Pragma.Single();
            Assert.Equal("no-cache", pragma.Name);
            Assert.Null(pragma.Value);
        }

        private TestServer CreateTestServer()
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            new MobileAppConfiguration()
                .AddTables()
                .ApplyTo(config);

            return TestServer.Create(appBuilder =>
            {
                appBuilder.UseWebApi(config);
            });
        }
    }
}