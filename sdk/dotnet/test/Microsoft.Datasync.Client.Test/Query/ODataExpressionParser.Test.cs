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
            string dateStr = "2018-08-15T12:15:22.012Z";
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
            string dateStr = "2018-08-15T12:15:22.012Z";
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
    }
}
