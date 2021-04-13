﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AzureMobile.Server.Extensions;
using Microsoft.AzureMobile.Server.InMemory;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class JsonPatchDocument_Tests
    {
        private class Entity : InMemoryTableData { }

        [Theory]
        [InlineData("replace", "/id", "changed", true)]
        [InlineData("replace", "/id", "test", false)]
        [InlineData("replace", "/updatedAt", "2018-12-31T05:00:00.000Z", true)]
        [InlineData("replace", "/updatedAt", "2021-12-31T05:30:00.000Z", false)]
        [InlineData("replace", "/updatedAt", "some-string", false)]
        [InlineData("replace", "/version", "AQBCIkeP", false)]
        [InlineData("replace", "/version", "dGVzdA==", true)]
        [InlineData("test", "/id", "changed", false)]
        [InlineData("test", "/updatedAt", "2018-12-31T05:00:00.000Z", false)]
        [InlineData("test", "/version", "dGVzdA==", false)]
        public void ModifiesSystemProperties_BasicTests(string op, string path, string value, bool expected)
        {
            // Arrange
            var patchDoc = new JsonPatchDocument<Entity>();
            if (path.Equals("/updatedAt", StringComparison.OrdinalIgnoreCase) && value.EndsWith(".000Z"))
                patchDoc.Operations.Add(new Operation<Entity>(op, path, null, DateTime.Parse(value)));
            else
                patchDoc.Operations.Add(new Operation<Entity>(op, path, null, value));

            var entity = new Entity
            {
                Id = "test",
                UpdatedAt = DateTimeOffset.Parse("2021-12-31T05:30:00.000Z"),
                Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F }
            };

            // Act
            var actual = patchDoc.ModifiesSystemProperties(entity);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
