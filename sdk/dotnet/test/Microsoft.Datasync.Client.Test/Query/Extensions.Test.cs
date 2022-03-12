// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Query
{
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
    }
}
