// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Tables;
using Moq;
using Swashbuckle.Swagger;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test
{
    public class MobileAppHeaderFilterTests
    {
        public static TheoryDataCollection<Collection<IFilter>> FilterDataSet
        {
            get
            {
                return new TheoryDataCollection<Collection<IFilter>>
                {
                    CreateCollection(new MobileAppControllerAttribute()),
                    CreateCollection(new TableControllerConfigAttribute()),
                    CreateCollection(new MobileAppControllerAttribute(), new TableControllerConfigAttribute()),
                    CreateCollection(new MobileAppControllerAttribute(), new AuthorizeAttribute())
                };
            }
        }

        [Theory]
        [MemberData("FilterDataSet")]
        public void HeaderFilter_AddsHeaderRequirement(Collection<IFilter> filters)
        {
            // Arrange
            var swashbuckleFilter = new MobileAppHeaderFilter();
            var operation = new Operation();

            var controllerDescMock = new Mock<HttpControllerDescriptor>();
            controllerDescMock.Setup(c => c.GetFilters()).Returns(filters);

            var description = new ApiDescription();
            description.ActionDescriptor = new ReflectedHttpActionDescriptor();
            description.ActionDescriptor.ControllerDescriptor = controllerDescMock.Object;

            // Act
            swashbuckleFilter.Apply(operation, null, description);

            // Assert
            Assert.NotNull(operation.parameters);
            Assert.Equal(1, operation.parameters.Count);
            Parameter parameter = operation.parameters[0];
            Assert.Equal("ZUMO-API-VERSION", parameter.name);
            Assert.Equal("header", parameter.@in);
            Assert.Equal("string", parameter.type);
            Assert.Equal("2.0.0", parameter.@default);
            Assert.True(parameter.required);
        }

        private static Collection<IFilter> CreateCollection(params IFilter[] filters)
        {
            return new Collection<IFilter>(filters);
        }
    }
}