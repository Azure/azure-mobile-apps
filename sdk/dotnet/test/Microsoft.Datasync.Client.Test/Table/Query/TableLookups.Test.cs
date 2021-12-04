// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query;
using Microsoft.Datasync.Client.Table.Query.Nodes;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Query
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class TableLookups_Tests
    {
        [Fact]
        [Trait("Method", "BinaryOperatorKind.ToODataString")]
        public void BinaryOperatorKind_ToODataString_ThrowsOnOutOfRange()
        {
            const BinaryOperatorKind kind = (BinaryOperatorKind)1000;
            Assert.Throws<NotSupportedException>(() => kind.ToODataString());
        }

        [Theory]
        [InlineData(null, "null")]
        [InlineData(true, "true")]
        [InlineData(false, "false")]
        [InlineData((byte)0, "00")]
        [InlineData((byte)255, "FF")]
        [InlineData('a', "'a'")]
        [InlineData('\'', "''''")]
        [InlineData('\\', "'\\'")]
        [InlineData((double)2.0, "2.0")]
        [InlineData((float)2.0, "2f")]
        [InlineData((long)2, "2L")]

        [Trait("Method", "ConstantNode.ToODataString")]
        public void ConstantNode_ToODataString_ReturnsExpected(object source, string expected)
        {
            var node = new ConstantNode(source);
            Assert.Equal(expected, node.ToODataString());
        }

        [Fact]
        [Trait("Method", "ConstantNode.ToODataString")]
        public void ConstantNode_ToODataString_ReturnsExpected_ForNonTheory()
        {
            var guidNode = new ConstantNode(Guid.Parse("775db25a-754a-455d-9ade-68d327e41c43"));
            var dtNode = new ConstantNode(new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc));
            var dtoNode = new ConstantNode(new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero));
            var decNode = new ConstantNode((decimal)2.0);

            Assert.Equal("cast(775db25a-754a-455d-9ade-68d327e41c43,Edm.Guid)", guidNode.ToODataString());
            Assert.Equal("cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", dtNode.ToODataString());
            Assert.Equal("cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)", dtoNode.ToODataString());
            Assert.Equal("2M", decNode.ToODataString());
        }
    }
}
