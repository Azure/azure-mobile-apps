// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Query.OData;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Query
{
    [ExcludeFromCodeCoverage]
    public class ODataExpressionParser_Tests
    {
        private const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffZ";

        /// <summary>
        /// A helper class to swap the culture to a temporary culture.
        /// </summary>
        private class CurrentCultureHelper : IDisposable
        {
            private readonly CultureInfo previous;

            public static CultureInfo CurrentCulture
            {
                get => CultureInfo.CurrentCulture;
                set => CultureInfo.CurrentCulture = value;
            }

            public CurrentCultureHelper(string name)
            {
                previous = CurrentCulture;
                CurrentCulture = new CultureInfo(name);
            }

            public void Dispose()
            {
                CurrentCulture = previous;
            }
        }

        [Fact]
        public void ParseFilter_Real_NumberDecimalSeparator()
        {
            // Set some CultureInfo with different decimal separator
            using var _ = new CurrentCultureHelper("ru-RU");
            QueryNode queryNode = ODataExpressionParser.ParseFilter("Field eq 42.42");

            Assert.NotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.NotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.NotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.NotNull(right);

            Assert.Equal("Field", left.MemberName);
            Assert.Equal(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.Equal(42.42, right.Value);
        }

        [Fact]
        public void ParseFilter_Guid()
        {
            Guid filterGuid = Guid.NewGuid();

            QueryNode queryNode = ODataExpressionParser.ParseFilter($"Field eq cast({filterGuid},Edm.Guid)");

            Assert.NotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.NotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.NotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.NotNull(right);

            Assert.Equal("Field", left.MemberName);
            Assert.Equal(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.Equal(filterGuid, right.Value);
        }

        [Fact]
        public void ParseFilter_DateTime()
        {
            const string dateStr = "2018-08-15T12:15:22.012Z";
            DateTime comparison = DateTime.Parse(dateStr);
            QueryNode queryNode = ODataExpressionParser.ParseFilter($"Field eq cast({dateStr},Edm.DateTime)");

            Assert.NotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.NotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.NotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.NotNull(right);

            Assert.Equal("Field", left.MemberName);
            Assert.Equal(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.Equal(comparison, right.Value);
        }

        [Fact]
        public void ParseFilter_DateTimeOffset()
        {
            const string dateStr = "2018-08-15T12:15:22.012Z";
            DateTimeOffset comparison = DateTimeOffset.Parse(dateStr);
            QueryNode queryNode = ODataExpressionParser.ParseFilter($"Field eq cast({dateStr},Edm.DateTimeOffset)");

            Assert.NotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.NotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.NotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.NotNull(right);

            Assert.Equal("Field", left.MemberName);
            Assert.Equal(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.Equal(comparison, right.Value);
        }

        [Fact]
        public void ParseFilter_TrueToken()
        {
            QueryNode queryNode = ODataExpressionParser.ParseFilter("(true eq null) and false");

            Assert.NotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.NotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as BinaryOperatorNode;
            Assert.NotNull(left);

            var trueNode = left.LeftOperand as ConstantNode;
            Assert.NotNull(trueNode);
            Assert.Equal(true, trueNode.Value);

            var nullNode = left.RightOperand as ConstantNode;
            Assert.NotNull(nullNode);
            Assert.Null(nullNode.Value);

            var falseNode = comparisonNode.RightOperand as ConstantNode;
            Assert.NotNull(falseNode);

            Assert.Equal(BinaryOperatorKind.And, comparisonNode.OperatorKind);
            Assert.Equal(false, falseNode.Value);
        }

        [Fact]
        public void ParseFilter_DateTimeMember()
        {
            QueryNode queryNode = ODataExpressionParser.ParseFilter("datetime eq 1");

            Assert.NotNull(queryNode);

            var comparisonNode = queryNode as BinaryOperatorNode;
            Assert.NotNull(comparisonNode);

            var left = comparisonNode.LeftOperand as MemberAccessNode;
            Assert.NotNull(left);

            var right = comparisonNode.RightOperand as ConstantNode;
            Assert.NotNull(right);

            Assert.Equal("datetime", left.MemberName);
            Assert.Equal(BinaryOperatorKind.Equal, comparisonNode.OperatorKind);
            Assert.Equal(1L, right.Value);
        }

        [Fact]
        public void ParseFilter_Guid_InvalidGuidString()
        {
            var ex = Assert.Throws<ODataException>(() => ODataExpressionParser.ParseFilter(string.Format("Field eq cast(this is not a guid,Edm.Guid)")));
            Assert.Equal(9, ex.ErrorPosition);
        }

        [Fact]
        public void ParseFilter_InvalidEdmType()
        {
            var ex = Assert.Throws<ODataException>(() => ODataExpressionParser.ParseFilter(string.Format("Field eq cast(this is not a guid,Edm.Unknown)")));
            Assert.Equal(9, ex.ErrorPosition);
        }

        [Theory]
        [InlineData(QueryTokenKind.Unknown, false)]
        [InlineData(QueryTokenKind.End, false)]
        [InlineData(QueryTokenKind.Identifier, false)]
        [InlineData(QueryTokenKind.StringLiteral, false)]
        [InlineData(QueryTokenKind.IntegerLiteral, true)]
        [InlineData(QueryTokenKind.RealLiteral, true)]
        [InlineData(QueryTokenKind.Not, false)]
        [InlineData(QueryTokenKind.Modulo, false)]
        [InlineData(QueryTokenKind.OpenParen, false)]
        [InlineData(QueryTokenKind.CloseParen, false)]
        [InlineData(QueryTokenKind.Multiply, false)]
        [InlineData(QueryTokenKind.Add, false)]
        [InlineData(QueryTokenKind.Subtract, false)]
        [InlineData(QueryTokenKind.Comma, false)]
        [InlineData(QueryTokenKind.Minus, false)]
        [InlineData(QueryTokenKind.Dot, false)]
        [InlineData(QueryTokenKind.Divide, false)]
        [InlineData(QueryTokenKind.LessThan, false)]
        [InlineData(QueryTokenKind.Equal, false)]
        [InlineData(QueryTokenKind.GreaterThan, false)]
        [InlineData(QueryTokenKind.NotEqual, false)]
        [InlineData(QueryTokenKind.And, false)]
        [InlineData(QueryTokenKind.LessThanEqual, false)]
        [InlineData(QueryTokenKind.GreaterThanEqual, false)]
        [InlineData(QueryTokenKind.Or, false)]
        public void QueryTokenKind_IsNumberLiteral_Works(QueryTokenKind kind, bool expected)
        {
            Assert.Equal(expected, kind.IsNumberLiteral());
        }

        [Theory]
        [InlineData(QueryTokenKind.Add, BinaryOperatorKind.Add)]
        [InlineData(QueryTokenKind.And, BinaryOperatorKind.And)]
        [InlineData(QueryTokenKind.Or, BinaryOperatorKind.Or)]
        [InlineData(QueryTokenKind.Equal, BinaryOperatorKind.Equal)]
        [InlineData(QueryTokenKind.NotEqual, BinaryOperatorKind.NotEqual)]
        [InlineData(QueryTokenKind.LessThan, BinaryOperatorKind.LessThan)]
        [InlineData(QueryTokenKind.LessThanEqual, BinaryOperatorKind.LessThanOrEqual)]
        [InlineData(QueryTokenKind.GreaterThan, BinaryOperatorKind.GreaterThan)]
        [InlineData(QueryTokenKind.GreaterThanEqual, BinaryOperatorKind.GreaterThanOrEqual)]
        [InlineData(QueryTokenKind.Subtract, BinaryOperatorKind.Subtract)]
        [InlineData(QueryTokenKind.Multiply, BinaryOperatorKind.Multiply)]
        [InlineData(QueryTokenKind.Divide, BinaryOperatorKind.Divide)]
        [InlineData(QueryTokenKind.Modulo, BinaryOperatorKind.Modulo)]
        public void QueryTokenKind_ToBinaryOperatorKind_Works(QueryTokenKind kind, BinaryOperatorKind expected)
        {
            Assert.Equal(expected, kind.ToBinaryOperatorKind());
        }

        [Theory]
        [InlineData(QueryTokenKind.Unknown)]
        [InlineData(QueryTokenKind.End)]
        [InlineData(QueryTokenKind.Identifier)]
        [InlineData(QueryTokenKind.StringLiteral)]
        [InlineData(QueryTokenKind.IntegerLiteral)]
        [InlineData(QueryTokenKind.RealLiteral)]
        [InlineData(QueryTokenKind.Not)]
        [InlineData(QueryTokenKind.OpenParen)]
        [InlineData(QueryTokenKind.CloseParen)]
        [InlineData(QueryTokenKind.Comma)]
        [InlineData(QueryTokenKind.Dot)]
        public void QueryTokenKind_ToBinaryOperatorKind_ThrowsOnInvalid(QueryTokenKind kind)
        {
            Assert.ThrowsAny<Exception>(() => kind.ToBinaryOperatorKind());
        }
    }
}
