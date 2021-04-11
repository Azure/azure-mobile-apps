// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Common.Test.TestData;
using Microsoft.AzureMobile.Server.Tables;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Tables
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class PagedResult_Tests
    {
        [Fact]
        public void Ctor_CanCreateEmptyModel()
        {
            // Act
            var actual = new PagedResult();

            // Assert
            Assert.NotNull(actual);
            Assert.NotNull(actual.Items);
            Assert.Empty(actual.Items);
            Assert.Null(actual.Count);
            Assert.Null(actual.NextLink);
        }

        [Fact]
        public void Ctor_CanCreateFilledModel()
        {
            // Arrange
            var objects = Movies.OfType<InMemoryMovie>();

            // Act
            var actual = new PagedResult(objects);

            // Assert
            Assert.NotNull(actual);
            Assert.NotNull(actual.Items);
            Assert.Equal(248, actual.Items.Count());
            Assert.Null(actual.Count);
            Assert.Null(actual.NextLink);
        }
    }
}
