// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query;
using Microsoft.Datasync.Client.Table.Query.Nodes;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Query
{
    /// <summary>
    /// The main flows are tested by the <see cref="DatasyncTableQuery_Tests"/> test suite.  However, there
    /// are some test cases for things that **should** never happen.  If they do happen, we check
    /// to ensure the right exceptions are thrown and/or the right results are returned.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class ODataExpressionVisitor_Tests
    {
        [Fact]
        [Trait("Method", "ToODataString")]
        public void ToODataString_Null_ReturnsEmptyString()
        {
            Assert.Equal("", ODataExpressionVisitor.ToODataString(null));
        }

        [Fact]
        [Trait("Method", "ToODataString")]
        public void Accept_InvalidBinaryNode_Throws()
        {
            var node = new BinaryOperatorNode(BinaryOperatorKind.And);
            Assert.Throws<ArgumentException>(() => ODataExpressionVisitor.ToODataString(node));
        }

        [Fact]
        [Trait("Method", "ToODataString")]
        public void Accept_InvalidUnaryNode_Throws()
        {
            var node = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            Assert.Throws<ArgumentException>(() => ODataExpressionVisitor.ToODataString(node));
        }

        [Fact]
        [Trait("Method", "ToODataString")]
        public void Accept_NegateNode_Throws()
        {
            var node = new UnaryOperatorNode(UnaryOperatorKind.Negate, null);
            Assert.Throws<NotSupportedException>(() => ODataExpressionVisitor.ToODataString(node));
        }

        [Fact]
        [Trait("Method", "ToODataString")]
        public void Accept_ConvertNode_Throws()
        {
            var constant = new ConstantNode(5);
            var node = new ConvertNode(constant, typeof(string));
            Assert.Throws<NotSupportedException>(() => ODataExpressionVisitor.ToODataString(node));
        }
    }
}
