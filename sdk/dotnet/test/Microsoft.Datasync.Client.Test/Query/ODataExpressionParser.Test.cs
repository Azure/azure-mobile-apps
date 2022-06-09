// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Query.OData;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Query
{
    [ExcludeFromCodeCoverage]
    public class ODataExpressionParser_Tests
    {
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

        [Theory]
        [InlineData("")]
        [InlineData("$count=true")]
		[InlineData("$orderby=bestPictureWinner")]
		[InlineData("$orderby=bestPictureWinner desc")]
		[InlineData("$orderby=rating desc,title desc")]
		[InlineData("$skip=100")]
		[InlineData("$top=100")]
		[InlineData("$skip=100&$top=50")]
		[InlineData("$filter=bestPictureWinner")]
		[InlineData("$filter=not(bestPictureWinner)")]
		[InlineData("$filter=(bestPictureWinner eq true)")]
		[InlineData("$filter=(bestPictureWinner eq false)")]
		[InlineData("$filter=(bestPictureWinner ne true)")]
		[InlineData("$filter=(bestPictureWinner ne false)")]
		[InlineData("$filter=not((bestPictureWinner eq true))")]
		[InlineData("$filter=not((bestPictureWinner ne true))")]
		[InlineData("$filter=not((bestPictureWinner eq false))")]
		[InlineData("$filter=not((bestPictureWinner ne false))")]
		[InlineData("$filter=(duration eq 100)")]
		[InlineData("$filter=(duration lt 100)")]
		[InlineData("$filter=(duration le 100)")]
		[InlineData("$filter=(duration gt 90)")]
		[InlineData("$filter=(duration ge 90)")]
		[InlineData("$filter=(duration ne 100)")]
		[InlineData("$filter=not((duration eq 100))")]
		[InlineData("$filter=not((duration lt 100))")]
		[InlineData("$filter=not((duration le 100))")]
		[InlineData("$filter=not((duration gt 90))")]
		[InlineData("$filter=not((duration ge 90))")]
		[InlineData("$filter=not((duration ne 100))")]
		[InlineData($"$filter=(releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset))")]
		[InlineData($"$filter=(releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset))")]
		[InlineData($"$filter=(releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset))")]
		[InlineData($"$filter=(releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset))")]
		[InlineData($"$filter=(releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset))")]
		[InlineData("$filter=(title eq 'The Godfather')")]
		[InlineData("$filter=(title ne 'The Godfather')")]
		[InlineData("$filter=(rating ne null)")]
		[InlineData("$filter=(rating eq null)")]
		[InlineData("$filter=((year gt 1929) and (year lt 1940))")]
		[InlineData("$filter=((year ge 1930) and (year le 1939))")]
		[InlineData("$filter=((year gt 2000) or (year lt 1940))")]
		[InlineData("$filter=((year gt 2000) or not(bestPictureWinner))")]
		[InlineData("$filter=(((year ge 1930) and (year le 1940)) or ((year ge 1950) and (year le 1960)))")]
		[InlineData("$filter=((year sub 1900) gt 80)")]
		[InlineData("$filter=((year add duration) lt 2100)")]
		[InlineData("$filter=((year sub 1900) lt duration)")]
		[InlineData("$filter=((duration mul 2) lt 180)")]
		[InlineData("$filter=((year div 1000.5) eq 2.0)")]
		[InlineData("$filter=((duration mod 2) eq 1)")]
		[InlineData("$filter=((((year sub 1900) ge 80) and ((year add 10) le 2000)) and (duration le 120))")]
		[InlineData("$filter=(day(releaseDate) eq 1)")]
		[InlineData("$filter=(month(releaseDate) eq 11)")]
		[InlineData("$filter=(year(releaseDate) ne year)")]
		[InlineData("$filter=endswith(title,'er')")]
		[InlineData("$filter=endswith(tolower(title),'er')")]
		[InlineData("$filter=endswith(toupper(title),'ER')")]
		[InlineData("$filter=startswith(rating,'PG')")]
		[InlineData("$filter=startswith(tolower(rating),'pg')")]
		[InlineData("$filter=startswith(toupper(rating),'PG')")]
		[InlineData("$filter=(indexof(rating,'-') gt 0)")]
		[InlineData("$filter=contains(rating,'PG')")]
		[InlineData("$filter=(substring(rating,0,2) eq 'PG')")]
		[InlineData("$filter=(length(trim(title)) gt 10)")]
		[InlineData("$filter=(concat(title,rating) eq 'Fight ClubR')")]
		[InlineData("$filter=(round((duration div 60.0)) eq 2.0)")]
		[InlineData("$filter=(ceiling((duration div 60.0)) eq 2.0)")]
		[InlineData("$filter=(floor((duration div 60.0)) eq 2.0)")]
		[InlineData("$filter=(not(bestPictureWinner) and (round((duration div 60.0)) eq 2.0))")]
		[InlineData("$filter=(not(bestPictureWinner) and (ceiling((duration div 60.0)) eq 2.0))")]
		[InlineData("$filter=(not(bestPictureWinner) and (floor((duration div 60.0)) eq 2.0))")]
		[InlineData("$filter=(year lt 1990.5f)")]
        public void ODataExpressionParser_Roundtrips(string original)
        {
            var query = QueryDescription.Parse("movies", original);
            Assert.NotNull(query);

            // Numbers are converted to LONG during the conversion, which isn't an issue.  We translate [0-9]+L to [0-9]+
            string odataString = Regex.Replace(query.ToODataString(), "([0-9]+)L", "$1");

            Assert.Equal(original, odataString);
        }
    }
}
