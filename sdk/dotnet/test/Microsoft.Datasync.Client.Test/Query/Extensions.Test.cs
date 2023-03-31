// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Query.OData;
using System.Linq.Expressions;

namespace Microsoft.Datasync.Client.Test.Query;

/// <summary>
/// These tests are corner cases that are not normally encountered
/// with the Datasync library, but ensure that the right errors occur
/// if they should be encountered.
/// </summary>
[ExcludeFromCodeCoverage]
public class Extensions_Tests : BaseQueryTest
{
    [Fact]
    public void StripQuote_Strips_WhenQuoted()
    {
        ParameterExpression paramExpr = Expression.Parameter(typeof(int), "arg");
        LambdaExpression expression = Expression.Lambda(Expression.Add(paramExpr, Expression.Constant(1)), new List<ParameterExpression>() { paramExpr });
        var quoted = Expression.Quote(expression);
        var actual = quoted.StripQuote();
        Assert.Same(expression, actual);
    }

    [Fact]
    public void StripQuote_Returns_WhenNotQuoted()
    {
        ParameterExpression paramExpr = Expression.Parameter(typeof(int), "arg");
        LambdaExpression expression = Expression.Lambda(Expression.Add(paramExpr, Expression.Constant(1)), new List<ParameterExpression>() { paramExpr });
        var actual = expression.StripQuote();
        Assert.Same(expression, actual);
    }

    [Fact]
    public void IsValidLambdaExpression_False_WhenNull()
    {
        var actual = ExpressionExtensions.IsValidLambdaExpression(null, out LambdaExpression lambda);
        Assert.False(actual);
        Assert.Null(lambda);
    }

    [Fact]
    public void ToODataString_InvalidBinaryOperatorKind_Throws()
    {
        BinaryOperatorKind sut = (BinaryOperatorKind)99;
        Assert.ThrowsAny<Exception>(() => sut.ToODataString());
    }

    [Theory]
    [InlineData(null, "null")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData((byte)42, "2A")]
    [InlineData('a', "'a'")]
    [InlineData('\'', "''''")]
    [InlineData(42.2f, "42.2f")]
    [InlineData(4.2E14, "420000000000000.0")]
    [InlineData((double)42.4, "42.4")]
    [InlineData((double)42, "42.0")]
    [InlineData((long)4000, "4000L")]
    public void ToODataString_ConstantNode(object sut, string expected)
    {
        var node = new ConstantNode(sut);
        Assert.Equal(expected, node.ToODataString());
    }

    [Fact]
    public void ToODataString_ConstantNode_NonTheory()
    {
        var node = new ConstantNode(5.8M);
        Assert.Equal("5.8M", node.ToODataString());
    }
}
