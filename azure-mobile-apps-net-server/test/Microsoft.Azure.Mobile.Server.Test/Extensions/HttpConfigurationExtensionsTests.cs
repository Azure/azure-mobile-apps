// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http.Dispatcher;
using Microsoft.Azure.Mobile.Server.Cache;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.TestControllers;
using Moq;
using Xunit;

namespace System.Web.Http
{
    public class HttpConfigurationExtensionsTests
    {
        [Fact]
        public void GetMobileAppConfiguration_ReturnsNullByDefault()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            MobileAppConfiguration actual = config.GetMobileAppConfiguration();

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void SetMobileAppConfiguration_Roundtrips()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            MobileAppConfiguration options = new MobileAppConfiguration();

            // Act
            config.SetMobileAppConfiguration(options);
            MobileAppConfiguration actual = config.GetMobileAppConfiguration();

            // Assert
            Assert.Same(options, actual);
        }

        [Fact]
        public void SetMobileAppConfiguration_ReturnsNull_IfSetToNull()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetMobileAppConfiguration(null);
            MobileAppConfiguration actual = config.GetMobileAppConfiguration();

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetMobileAppSettingsProvider_ReturnsDefaultInstance()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            IMobileAppSettingsProvider actual = config.GetMobileAppSettingsProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<MobileAppSettingsProvider>(actual);
        }

        [Fact]
        public void SetMobileAppSettingsProvider_Roundtrips()
        {
            // Arrange
            MobileAppSettingsProvider provider = new MobileAppSettingsProvider();
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetMobileAppSettingsProvider(provider);
            IMobileAppSettingsProvider actual = config.GetMobileAppSettingsProvider();

            // Assert
            Assert.Same(provider, actual);
        }

        [Fact]
        public void SetMobileAppSettingsProvider_ReturnsDefault_IfSetToNull()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetMobileAppSettingsProvider(null);
            IMobileAppSettingsProvider actual = config.GetMobileAppSettingsProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<MobileAppSettingsProvider>(actual);
        }

        [Fact]
        public void GetCachePolicyProvider_ReturnsDefaultInstance()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            ICachePolicyProvider actual = config.GetCachePolicyProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<CachePolicyProvider>(actual);
        }

        [Fact]
        public void SetCachePolicyProvider_Roundtrips()
        {
            // Arrange
            CachePolicyProvider provider = new CachePolicyProvider();
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetCachePolicyProvider(provider);
            ICachePolicyProvider actual = config.GetCachePolicyProvider();

            // Assert
            Assert.Same(provider, actual);
        }

        [Fact]
        public void SetCachePolicyProvider_ReturnsDefault_IfSetToNull()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetCachePolicyProvider(null);
            ICachePolicyProvider actual = config.GetCachePolicyProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<CachePolicyProvider>(actual);
        }

        [Fact]
        public void GetMobileAppControllerConfigProvider_ReturnsDefaultInstance()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            IMobileAppControllerConfigProvider actual = config.GetMobileAppControllerConfigProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<MobileAppControllerConfigProvider>(actual);
        }

        [Fact]
        public void SetMobileAppControllerConfigProvider_Roundtrips()
        {
            // Arrange
            MobileAppControllerConfigProvider provider = new MobileAppControllerConfigProvider();
            HttpConfiguration config = new HttpConfiguration();

            // Act
            config.SetMobileAppControllerConfigProvider(provider);
            IMobileAppControllerConfigProvider actual = config.GetMobileAppControllerConfigProvider();

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
            IMobileAppControllerConfigProvider actual = config.GetMobileAppControllerConfigProvider();

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<MobileAppControllerConfigProvider>(actual);
        }

        [Fact]
        public void GetMobileAppControllerNames_ReturnsCorrectControllers()
        {
            // Arrange
            var typeList = new[]
            {
                typeof(MobileAppController),
                typeof(ApiController),
                typeof(DerivedMobileAppControllerController),
                typeof(NormalApiController)
            };

            HttpConfiguration config = new HttpConfiguration();
            IAssembliesResolver assemblyResolver = config.Services.GetAssembliesResolver();
            Mock<IHttpControllerTypeResolver> typeResolverMock = new Mock<IHttpControllerTypeResolver>();
            typeResolverMock.Setup(m => m.GetControllerTypes(assemblyResolver)).Returns(typeList);

            config.Services.Replace(typeof(IHttpControllerTypeResolver), typeResolverMock.Object);

            // Act
            var names = config.GetMobileAppControllerNames();

            // Assert
            Assert.Equal(new[] { "MobileApp", "DerivedMobileAppController" }, names);
        }

        [MobileAppController]
        private class MobileAppController : ApiController
        {
        }

        private class DerivedMobileAppControllerAttribute : MobileAppControllerAttribute
        {
        }

        [DerivedMobileAppController]
        private class DerivedMobileAppControllerController : ApiController
        {
        }

        private class NormalApiController : ApiController
        {
        }
    }
}