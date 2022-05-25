// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Testing;
using Swashbuckle.Application;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test
{
    public class SwaggerUiConfigExtensionsTests
    {
        [Fact]
        public async Task SwaggerUiDocs()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            TestServer server = SwashbuckleHelper.CreateSwaggerServer(config, null, c =>
            {
                c.MobileAppUi(config);
            });

            await ValidateResource(server, "Microsoft.Azure.Mobile.Server.Swagger.o2c.html", "http://localhost/swagger/ui/o2c-html");
            await ValidateResource(server, "Microsoft.Azure.Mobile.Server.Swagger.swagger-ui.min.js", "http://localhost/swagger/ui/swagger-ui-min-js");

            Assert.Contains(typeof(SwaggerUiSecurityFilter), config.MessageHandlers.Select(h => h.GetType()));
        }

        private static async Task ValidateResource(TestServer server, string resource, string requestUri)
        {
            string expected = GetResourceString(resource);

            // Act
            var response = await server.HttpClient.GetAsync(requestUri);
            string actual = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "This code is resilient to this scenario")]
        private static string GetResourceString(string resourceName)
        {
            string resourceText;

            using (Stream stream = typeof(SwaggerUiConfigExtensions).Assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    resourceText = reader.ReadToEnd();
                }
            }

            return resourceText;
        }
    }
}