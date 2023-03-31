// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;

namespace Microsoft.Datasync.Client.Test.Query;

[ExcludeFromCodeCoverage]
public class QueryDescription_Tests : BaseQueryTest
{
    [Fact]
    public void Parse_NullQuery()
    {
        var sut = QueryDescription.Parse("test");
        Assert.Equal("test", sut.TableName);
        Assert.Null(sut.Filter);
        Assert.False(sut.IncludeTotalCount);
        Assert.Empty(sut.Ordering);
        Assert.Empty(sut.Projections);
        Assert.Null(sut.ProjectionArgumentType);
        Assert.Empty(sut.Selection);
        Assert.Null(sut.Skip);
        Assert.Null(sut.Top);
    }

    [Fact]
    public void Parse_EmptyQuery()
    {
        var sut = QueryDescription.Parse("test", "");
        Assert.Equal("test", sut.TableName);
        Assert.Null(sut.Filter);
        Assert.False(sut.IncludeTotalCount);
        Assert.Empty(sut.Ordering);
        Assert.Empty(sut.Projections);
        Assert.Null(sut.ProjectionArgumentType);
        Assert.Empty(sut.Selection);
        Assert.Null(sut.Skip);
        Assert.Null(sut.Top);
    }

    [Fact]
    public void Parse_BasicFilter()
    {
        var sut = QueryDescription.Parse("test", "$filter=(field eq 'test')");
        Assert.Equal("test", sut.TableName);
        Assert.False(sut.IncludeTotalCount);
        Assert.Empty(sut.Ordering);
        Assert.Empty(sut.Projections);
        Assert.Null(sut.ProjectionArgumentType);
        Assert.Empty(sut.Selection);
        Assert.Null(sut.Skip);
        Assert.Null(sut.Top);

        Assert.IsAssignableFrom<BinaryOperatorNode>(sut.Filter);
        var node = sut.Filter as BinaryOperatorNode;
        Assert.Equal(BinaryOperatorKind.Equal, node.OperatorKind);
        Assert.IsAssignableFrom<MemberAccessNode>(node.LeftOperand);
        Assert.Equal("field", ((MemberAccessNode)node.LeftOperand).MemberName);
        Assert.IsAssignableFrom<ConstantNode>(node.RightOperand);
        Assert.Equal("test", ((ConstantNode)node.RightOperand).Value as string);
    }

    [Fact]
    public void Parse_BasicOrderBy()
    {
        var sut = QueryDescription.Parse("test", "$orderby=field desc");
        Assert.Equal("test", sut.TableName);
        Assert.Null(sut.Filter);
        Assert.False(sut.IncludeTotalCount);
        Assert.Empty(sut.Projections);
        Assert.Null(sut.ProjectionArgumentType);
        Assert.Empty(sut.Selection);
        Assert.Null(sut.Skip);
        Assert.Null(sut.Top);

        Assert.Single(sut.Ordering);
        var ordering = sut.Ordering[0];
        Assert.IsAssignableFrom<MemberAccessNode>(ordering.Expression);
        Assert.Equal("field", ((MemberAccessNode)ordering.Expression).MemberName);
        Assert.Equal(OrderByDirection.Descending, ordering.Direction);
    }

    [Fact]
    public void Parse_SkipTop()
    {
        var sut = QueryDescription.Parse("test", "$skip=42&$top=10");
        Assert.Equal("test", sut.TableName);
        Assert.Null(sut.Filter);
        Assert.False(sut.IncludeTotalCount);
        Assert.Empty(sut.Ordering);
        Assert.Empty(sut.Projections);
        Assert.Null(sut.ProjectionArgumentType);
        Assert.Empty(sut.Selection);
        Assert.Equal(42, sut.Skip);
        Assert.Equal(10, sut.Top);
    }

    [Fact]
    public void Parse_Selection()
    {
        var sut = QueryDescription.Parse("test", "$select=field,value");
        Assert.Equal("test", sut.TableName);
        Assert.Null(sut.Filter);
        Assert.False(sut.IncludeTotalCount);
        Assert.Empty(sut.Ordering);
        Assert.Empty(sut.Projections);
        Assert.Null(sut.ProjectionArgumentType);
        Assert.Null(sut.Skip);
        Assert.Null(sut.Top);

        Assert.Equal(2, sut.Selection.Count);
        Assert.Equal("field", sut.Selection[0]);
        Assert.Equal("value", sut.Selection[1]);
    }

    [Fact]
    public void Parse_Parameters()
    {
        var sut = QueryDescription.Parse("test", "__deleted=false");
        Assert.Equal("test", sut.TableName);
        Assert.Null(sut.Filter);
        Assert.False(sut.IncludeTotalCount);
        Assert.Empty(sut.Ordering);
        Assert.Empty(sut.Projections);
        Assert.Null(sut.ProjectionArgumentType);
        Assert.Empty(sut.Selection);
        Assert.Null(sut.Skip);
        Assert.Null(sut.Top);
    }

    [Fact]
    public void Clone_Complete()
    {
        var sut = QueryDescription.Parse("test", "$filter=(field eq 'test')&$select=field,value&$orderby=field desc&$skip=42&$top=10");

        // Filter
        Assert.IsAssignableFrom<BinaryOperatorNode>(sut.Filter);
        var node = sut.Filter as BinaryOperatorNode;
        Assert.Equal(BinaryOperatorKind.Equal, node.OperatorKind);
        Assert.IsAssignableFrom<MemberAccessNode>(node.LeftOperand);
        Assert.Equal("field", ((MemberAccessNode)node.LeftOperand).MemberName);
        Assert.IsAssignableFrom<ConstantNode>(node.RightOperand);
        Assert.Equal("test", ((ConstantNode)node.RightOperand).Value as string);

        // OrderBy
        Assert.Single(sut.Ordering);
        var ordering = sut.Ordering[0];
        Assert.IsAssignableFrom<MemberAccessNode>(ordering.Expression);
        Assert.Equal("field", ((MemberAccessNode)ordering.Expression).MemberName);
        Assert.Equal(OrderByDirection.Descending, ordering.Direction);

        // Skip, Top
        Assert.Equal(42, sut.Skip);
        Assert.Equal(10, sut.Top);

        // Select
        Assert.Equal(2, sut.Selection.Count);
        Assert.Equal("field", sut.Selection[0]);
        Assert.Equal("value", sut.Selection[1]);

        var clone = sut.Clone();

        // Filter
        Assert.IsAssignableFrom<BinaryOperatorNode>(clone.Filter);
        node = clone.Filter as BinaryOperatorNode;
        Assert.Equal(BinaryOperatorKind.Equal, node.OperatorKind);
        Assert.IsAssignableFrom<MemberAccessNode>(node.LeftOperand);
        Assert.Equal("field", ((MemberAccessNode)node.LeftOperand).MemberName);
        Assert.IsAssignableFrom<ConstantNode>(node.RightOperand);
        Assert.Equal("test", ((ConstantNode)node.RightOperand).Value as string);

        // OrderBy
        Assert.Single(clone.Ordering);
        ordering = clone.Ordering[0];
        Assert.IsAssignableFrom<MemberAccessNode>(ordering.Expression);
        Assert.Equal("field", ((MemberAccessNode)ordering.Expression).MemberName);
        Assert.Equal(OrderByDirection.Descending, ordering.Direction);

        // Skip, Top
        Assert.Equal(42, clone.Skip);
        Assert.Equal(10, clone.Top);

        // Select
        Assert.Equal(2, clone.Selection.Count);
        Assert.Equal("field", clone.Selection[0]);
        Assert.Equal("value", clone.Selection[1]);
    }
}
