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
    public class MobileAppControllerCacheTests
    {
        private HttpClient client;

        public MobileAppControllerCacheTests()
        {
            TestServer server = this.CreateTestServer();
            this.client = server.HttpClient;
        }

        [Fact]
        public async Task ApiController_AttributeRoute_AddsCacheHeaders()
        {
            HttpResponseMessage response = await this.client.GetAsync("api/attribute/test");
            VerifyResponse(response);
        }

        [Fact]
        public async Task ApiController_RouteTable_AddsCacheHeaders()
        {
            HttpResponseMessage response = await this.client.GetAsync("api/testapi");
            VerifyResponse(response);

            // make sure the formatters are correctly returning json by default            
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal("\"hello world\"", await response.Content.ReadAsStringAsync());
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
                .MapApiControllers()
                .ApplyTo(config);

            return TestServer.Create(appBuilder =>
            {
                appBuilder.UseWebApi(config);               
            });
        }
    }
}