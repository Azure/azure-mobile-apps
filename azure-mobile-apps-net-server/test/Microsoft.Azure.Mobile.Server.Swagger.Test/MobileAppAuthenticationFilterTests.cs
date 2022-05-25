// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using Moq;
using Swashbuckle.Swagger;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test
{
    public class MobileAppAuthenticationFilterTests
    {
        [Fact]
        public void MobileAppAuthFilter_AddsSecurity_WhenAuthorizeSpecified()
        {
            // Arrange
            var filter = new MobileAppAuthenticationFilter("facebook");

            var operation = new Operation();
            var apiDescription = new ApiDescription();
            var actionDescMock = new Mock<HttpActionDescriptor>();
            Collection<FilterInfo> pipeline = new Collection<FilterInfo>()
            {
                new FilterInfo(new AuthorizeAttribute(), FilterScope.Action)
            };
            actionDescMock.Setup(a => a.GetFilterPipeline()).Returns(pipeline);
            apiDescription.ActionDescriptor = actionDescMock.Object;

            // Act
            filter.Apply(operation, null, apiDescription);

            // Assert
            Assert.NotNull(operation.security);
            Assert.Equal(1, operation.security.Count);
            Assert.Equal(1, operation.security[0].Keys.Count);
            Assert.Equal("facebook", operation.security[0].Keys.First());
            Assert.Equal(0, operation.security[0]["facebook"].Count());
        }

        [Fact]
        public void MobileAppAuthFilter_DoesNotAddSecurity_WithoutAuthorizeSpecified()
        {
            // Arrange
            var filter = new MobileAppAuthenticationFilter("facebook");
            var operation = new Operation();
            var apiDescription = new ApiDescription();
            var actionDescMock = new Mock<HttpActionDescriptor>();
            actionDescMock.Setup(a => a.GetFilterPipeline()).Returns(new Collection<FilterInfo>());
            apiDescription.ActionDescriptor = actionDescMock.Object;

            // Act
            filter.Apply(operation, null, apiDescription);

            // Assert
            Assert.Null(operation.security);
        }
    }
}