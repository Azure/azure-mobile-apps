// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Linq.Query.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Linq.Query
{
    /// <summary>
    /// Corner cases only, as most normal paths are tested through the <see cref="LinqFunctionality"/> tests.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class QueryNode_Tests
    {
        [Fact]
        [Trait("Method", "BinaryOperatorNode.GetKind")]
        public void Binary_Kind_HasCorrectValue()
        {
            var sut = new BinaryOperatorNode(BinaryOperatorKind.Or);
            Assert.Equal(QueryNodeKind.BinaryOperator, sut.Kind);
        }

        [Fact]
        [Trait("Method", "BinaryOperatorNode.SetChildren")]
        public void Binary_SetChildren_ThrowsOnNull()
        {
            var node = new BinaryOperatorNode(BinaryOperatorKind.And);
            Assert.Throws<ArgumentNullException>(() => node.SetChildren(null));
        }

        [Theory, CombinatorialData]
        [Trait("Method", "BinaryOperatorNode.SetChildren")]
        public void Binary_SetChildren_ThrowsOnInvalid([CombinatorialValues(0, 1, 3)] int count)
        {
            List<QueryNode> children = new();
            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    children.Add(new ConstantNode(5));
                }
            }

            var node = new BinaryOperatorNode(BinaryOperatorKind.And);
            Assert.Throws<ArgumentException>(() => node.SetChildren(children));
        }

        [Fact]
        [Trait("Method", "ConstantNode.GetKind")]
        public void Constant_Kind_HasCorrectValue()
        {
            var sut = new ConstantNode(5);
            Assert.Equal(QueryNodeKind.Constant, sut.Kind);
        }

        [Fact]
        [Trait("Method", "ConstantNode.SetChildren")]
        public void Constant_SetChildren_Throws()
        {
            var node = new ConstantNode(5);
            Assert.Throws<NotSupportedException>(() => node.SetChildren(new List<QueryNode>()));
        }

        [Fact]
        [Trait("Method", "ConvertNode.GetKind")]
        public void Convert_Kind_HasCorrectValue()
        {
            var node = new ConstantNode(5);
            var sut = new ConvertNode(node, typeof(int));
            Assert.Equal(QueryNodeKind.Convert, sut.Kind);
            Assert.Same(node, sut.Source);
            Assert.Equal(typeof(int), sut.TargetType);
        }

        [Fact]
        [Trait("Method", "FunctionCallNode.GetKind")]
        public void Function_Kind_HasCorrectValue()
        {
            var sut = new FunctionCallNode("Concat");
            Assert.Equal(QueryNodeKind.FunctionCall, sut.Kind);
        }

        [Fact]
        [Trait("Method", "MemberAccessNode.GetKind")]
        public void Member_Kind_HasCorrectValue()
        {
            var sut = new MemberAccessNode(null, "Day");
            Assert.Null(sut.Instance);
            Assert.Equal(QueryNodeKind.MemberAccess, sut.Kind);
        }

        [Fact]
        [Trait("Method", "MemberAccessNode.GetInstance")]
        public void Member_Instance_CanRoundTrip()
        {
            var node = new ConstantNode(5);
            var sut = new MemberAccessNode(node, "Day");
            Assert.Same(node, sut.Instance);
        }
        [Fact]
        [Trait("Method", "UnaryOperatorNode.GetKind")]
        public void Unary_Kind_HasCorrectValue()
        {
            var sut = new UnaryOperatorNode(UnaryOperatorKind.Negate, null);
            Assert.Equal(QueryNodeKind.UnaryOperator, sut.Kind);
        }

        [Fact]
        [Trait("Method", "UnaryOperatorNode.SetChildren")]
        public void Unary_SetChildren_ThrowsOnNull()
        {
            var node = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            Assert.Throws<ArgumentNullException>(() => node.SetChildren(null));
        }

        [Theory, CombinatorialData]
        [Trait("Method", "UnaryOperatorNode.SetChildren")]
        public void Unary_SetChildren_ThrowsOnInvalid([CombinatorialValues(0, 2, 3)] int count)
        {
            List<QueryNode> children = new();
            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    children.Add(new ConstantNode(5));
                }
            }

            var node = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            Assert.Throws<ArgumentException>(() => node.SetChildren(children));
        }

        [Fact]
        [Trait("Method", "UnaryOperatorNode.SetChildren")]
        public void Unary_SetChildren_SetsOperand()
        {
            var constant = new ConstantNode(5);
            List<QueryNode> children = new() { constant };
            var node = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            node.SetChildren(children);
            Assert.Same(constant, node.Operand);
        }
    }
}
