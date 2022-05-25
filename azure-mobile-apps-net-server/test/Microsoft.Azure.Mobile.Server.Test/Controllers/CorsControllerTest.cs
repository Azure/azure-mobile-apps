// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    public class CorsControllerTest
    {
        private const string RequestOriginHeader = "origin";
        private const string ResponseOriginHeader = "Access-Control-Allow-Origin";

        [Fact]
        public async Task MethodAttributePolicy_NoHeaderReturns()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            request.Headers.Add(RequestOriginHeader, "http://unknown");
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains(ResponseOriginHeader));
        }

        [Fact]
        public async Task MethodAttributePolicy_HeaderReturns()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/testattribute");
            request.Headers.Add(RequestOriginHeader, "http://unknown");
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains(ResponseOriginHeader));
            Assert.Equal(response.Headers.GetValues(ResponseOriginHeader).FirstOrDefault(), "*");
        }

        [Fact]
        public async Task DefaultCorsPolicy_NoHeaderReturns()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            request.Headers.Add(RequestOriginHeader, "http://unknown");
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(response.Headers.Contains(ResponseOriginHeader));
        }

        [Fact]
        public async Task DefaultCorsPolicy_HeaderReturns()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "api/cors/test");
            request.Headers.Add(RequestOriginHeader, "http://sample");
            HttpResponseMessage response = await this.CreateTestServer().HttpClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.Contains(ResponseOriginHeader));
            Assert.Equal(response.Headers.GetValues(ResponseOriginHeader).FirstOrDefault(), "http://sample");
        }

        private TestServer CreateTestServer()
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            new MobileAppConfiguration()
                .MapApiControllers()
                .ApplyTo(config);

            config.EnableCors(new EnableCorsAttribute("http://sample", "*", "*"));

            return TestServer.Create(appBuilder =>
            {
                appBuilder.UseWebApi(config);
            });
        }
    }
}
