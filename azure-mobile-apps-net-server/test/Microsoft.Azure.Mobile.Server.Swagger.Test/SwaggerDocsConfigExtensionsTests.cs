// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Swashbuckle.Application;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test
{
    public class SwaggerDocsConfigExtensionsTests
    {
        [Fact]
        public async Task AppServiceAuthentication_AddsSecurityDefinition()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            TestServer server = SwashbuckleHelper.CreateSwaggerServer(config, c =>
            {
                c.AppServiceAuthentication("http://mysite", "google");
            }, null);

            // Act
            HttpResponseMessage swaggerResponse = await server.HttpClient.GetAsync("http://localhost/swagger/docs/v1");
            var swagger = await swaggerResponse.Content.ReadAsAsync<JObject>();
            var googleDef = swagger["securityDefinitions"]["google"];

            // Assert
            Assert.Equal("oauth2", googleDef["type"]);
            Assert.Equal("OAuth2 Implicit Grant", googleDef["description"]);
            Assert.Equal("implicit", googleDef["flow"]);
            Assert.Equal("http://mysite/.auth/login/google", googleDef["authorizationUrl"]);
            Assert.Equal(string.Empty, googleDef["scopes"]["google"].ToString());
        }

        [Fact]
        public async Task AppServiceAuthentication_AddsAuthToAuthenticatedControllers()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            TestServer server = SwashbuckleHelper.CreateSwaggerServer(config, c =>
            {
                c.AppServiceAuthentication("http://mysite", "google");
            }, null);

            // Act
            HttpResponseMessage swaggerResponse = await server.HttpClient.GetAsync("http://localhost/swagger/docs/v1");
            var swagger = await swaggerResponse.Content.ReadAsAsync<JObject>();

            // Assert
            this.ValidateSecurity(swagger, "/api/Anonymous", "get", false);
            this.ValidateSecurity(swagger, "/api/Anonymous", "post", false);
            this.ValidateSecurity(swagger, "/api/Authenticated", "get", true);
            this.ValidateSecurity(swagger, "/api/Authenticated", "post", true);
            this.ValidateSecurity(swagger, "/api/MixedAuth", "get", false);
            this.ValidateSecurity(swagger, "/api/MixedAuth", "post", true);
        }

        private void ValidateSecurity(JObject swagger, string route, string action, bool expectSecurity)
        {
            var security = swagger["paths"][route][action]["security"];

            if (!expectSecurity)
            {
                Assert.Null(security);
                return;
            }

            Assert.NotNull(security);
            security = security as JArray;
            Assert.Equal(1, security.Count());
            var securityDef = security[0];
            Assert.Equal(1, securityDef.Count());
            Assert.Equal("google", ((JProperty)securityDef.First).Name);
            Assert.Empty(securityDef["google"] as JArray);
        }
    }
}