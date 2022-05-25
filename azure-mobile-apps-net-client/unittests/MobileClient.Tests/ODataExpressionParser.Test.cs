// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Globalization;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Query;
using Xunit;

namespace MobileClient.Tests
{
    public class ODataExpressionParser_Test
    {
        class CurrentCultureHelper : IDisposable
        {
            private CultureInfo previous;

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
            using (var _ = new CurrentCultureHelper("ru-RU"))
            {
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
        }

        [Fact]
        public void ParseFilter_Guid()
        {
            Guid filterGuid = Guid.NewGuid();

            QueryNode queryNode = ODataExpressionParser.ParseFilter(string.Format("Field eq guid'{0}'", filterGuid));

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
            => Assert.Throws<MobileServiceODataException>(() => ODataExpressionParser.ParseFilter(string.Format("Field eq guid'this is not a guid'")));
    }
}
