// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Query
{
    [ExcludeFromCodeCoverage]
    public class Nodes_Test : OldBaseTest
    {
        [Fact]
        public void UnaryOperatorNode_SetChildren_NullThrows()
        {
            var sut = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            Assert.Throws<ArgumentNullException>(() => sut.SetChildren(null));
        }

        [Fact]
        public void UnaryOperatorNode_SetChildren_ThrowsEmpty()
        {
            var sut = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            var list = new List<QueryNode>();
            Assert.Throws<ArgumentException>(() => sut.SetChildren(list));
        }

        [Fact]
        public void UnaryOperatorNode_SetChildren_ThrowsMoreThanOne()
        {
            var sut = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            var constant = new ConstantNode(5);
            var list = new List<QueryNode>();
            list.AddRange(new QueryNode[] { constant, constant });
            Assert.Throws<ArgumentException>(() => sut.SetChildren(list));
        }

        [Fact]
        public void UnaryOperatorNode_SetChildren_SetsForSingle()
        {
            var sut = new UnaryOperatorNode(UnaryOperatorKind.Not, null);
            var constant = new ConstantNode(5);
            var list = new List<QueryNode>();
            list.AddRange(new QueryNode[] { constant });
            sut.SetChildren(list);
            Assert.Same(constant, sut.Operand);
        }

        [Fact]
        public void ConstantNode_SetChildren_Throws()
        {
            var sut = new ConstantNode(5);
            Assert.Throws<NotSupportedException>(() => sut.SetChildren(new List<QueryNode>()));
        }

        [Fact]
        public void ConstantNode_Kind_IsSet()
        {
            var sut = new ConstantNode(5);
            Assert.Equal(QueryNodeKind.Constant, sut.Kind);
        }

        [Fact]
        public void MemberAccessNode_Kind_IsSet()
        {
            var constant = new ConstantNode(5);
            var sut = new MemberAccessNode(constant, "member");
            Assert.Same(constant, sut.Instance);
            Assert.Equal("member", sut.MemberName);
            Assert.Equal(QueryNodeKind.MemberAccess, sut.Kind);
        }

        [Fact]
        public void FunctionCallNode_Kind_IsSet()
        {
            var sut = new FunctionCallNode("test");
            Assert.Equal(QueryNodeKind.FunctionCall, sut.Kind);
            Assert.Equal("test", sut.Name);
        }

        [Fact]
        public void ConvertNode_Kind_IsSet()
        {
            var from = new ConstantNode(5);
            var to = typeof(int);
            var sut = new ConvertNode(from, to);
            Assert.Same(from, sut.Source);
            Assert.Same(to, sut.TargetType);
            Assert.Equal(QueryNodeKind.Convert, sut.Kind);
        }

        [Fact]
        public void BinaryOperatorNode_SetChildren_NullThrows()
        {
            var sut = new BinaryOperatorNode(BinaryOperatorKind.And);
            Assert.Throws<ArgumentNullException>(() => sut.SetChildren(null));
        }

        [Fact]
        public void BinaryOperatorNode_SetChildren_ZeroThrows()
        {
            var sut = new BinaryOperatorNode(BinaryOperatorKind.And);
            var list = new List<QueryNode>();
            Assert.Throws<ArgumentException>(() => sut.SetChildren(list));
        }

        [Fact]
        public void BinaryOperatorNode_SetChildren_OneThrows()
        {
            var sut = new BinaryOperatorNode(BinaryOperatorKind.And);
            var list = new List<QueryNode>();
            list.AddRange(new QueryNode[] { new ConstantNode(1) });
            Assert.Throws<ArgumentException>(() => sut.SetChildren(list));
        }

        [Fact]
        public void BinaryOperatorNode_SetChildren_TwoWorks()
        {
            var sut = new BinaryOperatorNode(BinaryOperatorKind.And);
            var list = new List<QueryNode>();
            list.AddRange(new QueryNode[] { new ConstantNode(1), new ConstantNode(2) });
            sut.SetChildren(list);

            Assert.Same(list[0], sut.LeftOperand);
            Assert.Same(list[1], sut.RightOperand);
        }

        [Fact]
        public void BinaryOperatorNode_SetChildren_ThreeThrows()
        {
            var sut = new BinaryOperatorNode(BinaryOperatorKind.And);
            var list = new List<QueryNode>();
            list.AddRange(new QueryNode[] { new ConstantNode(1), new ConstantNode(2), new ConstantNode(3) });
            Assert.Throws<ArgumentException>(() => sut.SetChildren(list));
        }
    }
}
