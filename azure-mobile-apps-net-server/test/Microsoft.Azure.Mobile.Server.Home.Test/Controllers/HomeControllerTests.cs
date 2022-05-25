// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    public class HomeControllerTests : IClassFixture<HomeControllerTests.TestContext>
    {
        private TestContext context;

        public HomeControllerTests(HomeControllerTests.TestContext data)
        {
            this.context = data;
        }

        [Fact]
        public async Task Index_Returns_Html()
        {
            // Act
            HttpResponseMessage response = await this.context.Client.GetAsync(string.Empty);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void Index_Allows_AnonymousAccess()
        {
            // Arrange
            MethodInfo methodInfo = typeof(HomeController).GetMethod("Index");
            object[] actualAttrs = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true);

            // Act
            AllowAnonymousAttribute actualAttr = actualAttrs.Single() as AllowAnonymousAttribute;

            // Assert
            Assert.NotNull(actualAttr);
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("/some/unknown/path")]
        [InlineData("unknown?query")]
        public async Task Unknown_ReturnsNotFound(string path)
        {
            // Act
            HttpResponseMessage response = await this.context.Client.GetAsync(path);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public class TestContext
        {
            public TestContext()
            {
                var config = new HttpConfiguration();
                new MobileAppConfiguration()
                    .AddMobileAppHomeController()
                    .ApplyTo(config);

                TestServer server = TestServer.Create(app =>
                    {
                        app.UseWebApi(config);
                    });

                this.Client = server.HttpClient;
            }

            public HttpClient Client { get; private set; }
        }
    }
}
