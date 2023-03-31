// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Query.Linq.Nodes;

namespace Microsoft.Datasync.Client.Test.Query;

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

    [Fact]
    [Trait("Method", "SetChildren")]
    public void BinaryOperatorNode_NeedsTwoArguments()
    {
        BinaryOperatorNode node = new(BinaryOperatorKind.Add);
        QueryNode[] children = new QueryNode[] { new ConstantNode(1), new ConstantNode(2) };
        node.SetChildren(children);

        children = new QueryNode[] { new ConstantNode(1) };
        Assert.ThrowsAny<Exception>(() => node.SetChildren(children));
    }

    [Fact]
    [Trait("Method", "SetChildren")]
    public void UnaryOperatorNode_NeedsOneArgument()
    {
        UnaryOperatorNode node = new(UnaryOperatorKind.Not, new ConstantNode(1));
        QueryNode[] children = new QueryNode[] { new ConstantNode(1), new ConstantNode(2) };
        Assert.ThrowsAny<Exception>(() => node.SetChildren(children));

        children = new QueryNode[] { new ConstantNode(1) };
        node.SetChildren(children);
    }

    [Fact]
    [Trait("Method", "get_Instance")]
    public void MemberAccessNode_GetInstance()
    {
        BinaryOperatorNode instance = new(BinaryOperatorKind.Add);
        MemberAccessNode node = new(instance, "field");
        Assert.Same(instance, node.Instance);
    }

    [Fact]
    [Trait("Method", "SetChildren")]
    public void ConstantNode_SetChildren_Throws()
    {
        ConstantNode node = new(true);
        Assert.ThrowsAny<Exception>(() => node.SetChildren(new QueryNode[] { node }));
    }

    [Fact]
    [Trait("Method", "SetChildren")]
    public void ConvertNode_SetChildren_Works()
    {
        ConvertNode instance = new ConvertNode(null, typeof(string));
        List<QueryNode> children = new QueryNode[] { new ConstantNode("abc123") }.ToList();
        instance.SetChildren(children);
        Assert.Same(children[0], instance.Source);
        Assert.Equal(typeof(string), instance.TargetType);
    }

    [Fact]
    [Trait("Method", "SetChildren")]
    public void ConvertNode_SetChildren_ThrowsException()
    {
        ConvertNode instance = new ConvertNode(null, typeof(string));
        Assert.ThrowsAny<Exception>(() => instance.SetChildren(Array.Empty<QueryNode>()));
    }
}
