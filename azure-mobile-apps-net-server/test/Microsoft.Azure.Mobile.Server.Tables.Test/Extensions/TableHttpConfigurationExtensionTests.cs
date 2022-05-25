// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Azure.Mobile.Server.TestControllers;
using Moq;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Extensions
{
    public class TableHttpConfigurationExtensionTests
    {
        [Fact]
        public void GetTableControllerConfigProvider_ReturnsDefaultInstance()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            ITableControllerConfigProvider actual = config.GetTableControllerConfigProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<TableControllerConfigProvider>(actual);
        }

        [Fact]
        public void SetMobileAppControllerConfigProvider_Roundtrips()
        {
            // Arrange
            TableControllerConfigProvider provider = new TableControllerConfigProvider();
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetTableControllerConfigProvider(provider);
            ITableControllerConfigProvider actual = config.GetTableControllerConfigProvider();

            // Assert
            Assert.Same(provider, actual);
        }

        [Fact]
        public void SetMobileAppControllerConfigProvider_ReturnsDefault_IfSetToNull()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetMobileAppControllerConfigProvider(null);
            ITableControllerConfigProvider actual = config.GetTableControllerConfigProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<TableControllerConfigProvider>(actual);
        }

        [Fact]
        public void GetTableControllerNames_ReturnsCorrectControllers()
        {
            // Arrange
            var typeList = new[]
            {
                typeof(MoviesController),
                typeof(ZetaController),
                typeof(TestTableController),
                typeof(ApiController),
                typeof(CustomController),
                typeof(DerivedMoviesController)
            };

            HttpConfiguration config = new HttpConfiguration();
            IAssembliesResolver assemblyResolver = config.Services.GetAssembliesResolver();
            Mock<IHttpControllerTypeResolver> typeResolverMock = new Mock<IHttpControllerTypeResolver>();
            typeResolverMock.Setup(m => m.GetControllerTypes(assemblyResolver)).Returns(typeList);

            config.Services.Replace(typeof(IHttpControllerTypeResolver), typeResolverMock.Object);

            // Act
            var names = config.GetTableControllerNames();

            // Assert
            Assert.Equal(new[] { "Movies", "TestTable", "DerivedMovies" }, names);
        }

        private class DerivedMoviesController : MoviesController
        {
        }

        [MobileAppController]
        private class CustomController : ApiController
        {
        }
    }
}