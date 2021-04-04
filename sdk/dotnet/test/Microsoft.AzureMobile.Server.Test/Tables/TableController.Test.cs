// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Server.InMemory;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Tables
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class TableController_Tests
    {
        [Fact]
        public void Repository_Throws_WhenSetNull()
        {
            // Arrange
            var controller = new TableController<InMemoryMovie>();

            // Act
            Assert.Throws<ArgumentNullException>(() => controller.Repository = null);
        }

        [Fact]
        public void Repository_Throws_WhenGetNull()
        {
            // Arrange
            var controller = new TableController<InMemoryMovie>();

            // Act
            Assert.Throws<InvalidOperationException>(() => controller.Repository);
        }

        [Fact]
        [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Proper split of arrange/act/assert")]
        public void Repository_CanBeStored()
        {
            // Arrange
            var repository = new InMemoryRepository<InMemoryMovie>();
            var controller = new TableController<InMemoryMovie>();

            // Act
            controller.Repository = repository;

            // Assert
            Assert.NotNull(controller.Repository);
            Assert.Equal(repository, controller.Repository);
        }
    }
}
