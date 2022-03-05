// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Query
{
    [ExcludeFromCodeCoverage]
    public class Nodes_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "get_Kind")]
        public void AllNodes_Kind_SetCorrectly()
        {
            QueryNode node = new ConstantNode(true);

            Assert.Equal(QueryNodeKind.BinaryOperator, new BinaryOperatorNode(BinaryOperatorKind.Or).Kind);
            Assert.Equal(QueryNodeKind.Constant, new ConstantNode(true).Kind);
            Assert.Equal(QueryNodeKind.Convert, new ConvertNode(node, typeof(QueryNode)).Kind);
            Assert.Equal(QueryNodeKind.FunctionCall, new FunctionCallNode("name").Kind);
            Assert.Equal(QueryNodeKind.MemberAccess, new MemberAccessNode(null, "updatedAt").Kind);
            Assert.Equal(QueryNodeKind.UnaryOperator, new UnaryOperatorNode(UnaryOperatorKind.Not, node).Kind);
        }
    }
}
