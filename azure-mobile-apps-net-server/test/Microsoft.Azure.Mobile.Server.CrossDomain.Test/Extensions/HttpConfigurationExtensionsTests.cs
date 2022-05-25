// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using Xunit;

namespace System.Web.Http
{
    public class HttpConfigurationExtensionsTests
    {
        private HttpConfiguration config = new HttpConfiguration();

        [Fact]
        public void GetCrossDomainOrigins_ReturnsNull_IfNotSet()
        {
            // Arrange
            IEnumerable<string> result = this.config.GetCrossDomainOrigins();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SetAndGetGetCrossDomainOrigins_Roundtrips()
        {
            // Arrange
            IEnumerable<string> expected = new List<string> { "a", "b" };

            // Act
            this.config.SetCrossDomainOrigins(expected);
            IEnumerable<string> result = this.config.GetCrossDomainOrigins();

            // Assert
            Assert.Same(expected, result);
        }

        [Fact]
        public void SetCrossDomainOrigins_AllowsNull()
        {
            // Act
            this.config.SetCrossDomainOrigins(null);

            // Assert
            Assert.Null(this.config.Properties["MS_CrossDomainOrigins"]);
        }

        [Fact]
        public void GetCrossDomainOrigins_ReturnsNullAfterSetToNull()
        {
            // Arrange
            this.config.SetCrossDomainOrigins(null);

            // Act
            IEnumerable<string> result = this.config.GetCrossDomainOrigins();

            // Assert
            Assert.Null(result);
        }
    }
}
