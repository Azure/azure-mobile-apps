// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Moq;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test
{
    public class MobileAppApiExplorerTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldExploreController_MatchesRouteConstraint(bool match)
        {
            // Arrange
            var config = new HttpConfiguration();
            var ctrlDesc = new HttpControllerDescriptor(config, "sample", typeof(SampleController));
            var explorer = new MobileAppApiExplorer(config);

            var constraintMock = new Mock<IHttpRouteConstraint>();
            var constraint = constraintMock.Object;

            var routeMock = new Mock<IHttpRoute>();
            routeMock.Setup(r => r.Constraints);
            var route = routeMock.Object;

            constraintMock.Setup(c => c.Match(null, route, "controller", It.IsAny<IDictionary<string, object>>(), HttpRouteDirection.UriResolution))
                .Returns(match)
                .Callback<HttpRequestMessage, IHttpRoute, string, IDictionary<string, object>, HttpRouteDirection>((req, rt, p, cnts, rd) =>
                {
                    Assert.Equal("sample", cnts["controller"]);
                });
            routeMock.Setup(r => r.Constraints)
                .Returns(new Dictionary<string, object> { { "controller", constraint } });

            // Act
            bool actual = explorer.ShouldExploreController("sample", ctrlDesc, route);

            // Assert
            Assert.Equal(match, actual);
        }

        public class SampleController : ApiController
        {
            public string Get()
            {
                return "Hello";
            }
        }
    }
}