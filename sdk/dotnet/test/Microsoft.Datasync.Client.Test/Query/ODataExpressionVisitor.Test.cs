// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Query.OData;

namespace Microsoft.Datasync.Client.Test.Query;

/// <summary>
/// Corner case tests for things that should never happen.
/// </summary>
[ExcludeFromCodeCoverage]
public class ODataExpressionVisitor_Tests
{
    [Fact]
    public void ToODataString_Null_ReturnsEmpty()
    {
        Assert.Equal("", ODataExpressionVisitor.ToODataString(null));
    }

    [Fact]
    public void Visitor_ConvertNode_Throws()
    {
        var visitor = new TestODataExpressionVisitor();
        var node = new ConvertNode(new ConstantNode(1), typeof(int));
        Assert.ThrowsAny<Exception>(() => visitor.Visit(node));
    }

    [Fact]
    public void Visitor_UnaryNode_Throws()
    {
        var visitor = new TestODataExpressionVisitor();
        var node = new UnaryOperatorNode(UnaryOperatorKind.Negate, new ConstantNode(1));
        Assert.ThrowsAny<Exception>(() => visitor.Visit(node));
    }

    [Fact]
    public void Accept_Throws_OnIncompleteTypes()
    {
        var visitor = new TestODataExpressionVisitor();
        var node = new ConstantNode(1);
        Assert.ThrowsAny<Exception>(() => visitor.TestAccept(node, null));
    }

    private class TestODataExpressionVisitor : ODataExpressionVisitor
    {
        public TestODataExpressionVisitor() : base()
        {
        }

        public void TestAccept(QueryNode parent, QueryNode node)
            => base.Accept(parent, node);
    }
}
