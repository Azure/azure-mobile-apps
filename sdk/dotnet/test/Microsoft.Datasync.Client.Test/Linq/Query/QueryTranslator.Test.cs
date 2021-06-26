// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Linq;
using Microsoft.Datasync.Client.Linq.Query;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Linq.Query
{
    /// <summary>
    /// Tests for some corner cases that *SHOULD* never happen
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class QueryTranslator_Tests : BaseTest
    {
        /// <summary>
        /// A duplicate of the QueryTranslator that allows access to protected methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [SuppressMessage("Design", "RCS1158:Static member in generic type should use a type parameter.", Justification = "Test suite")]
        internal class TestQueryTranslator<T> : QueryTranslator<T>
        {
            internal TestQueryTranslator(DatasyncTableQuery<T> query, DatasyncClientOptions options) : base(query, options)
            {
            }

            internal int CountOrderings() => QueryDescription.Ordering.Count;

            internal void TestAddFilter(MethodCallExpression expression)
                => AddFilter(expression);

            internal void TestAddOrdering(MethodCallExpression expression)
                => AddOrdering(expression, true, false);

            internal void TestAddOrderByNode(string memberName)
                => AddOrderByNode(memberName, true, false);

            internal void TestAddProjection(MethodCallExpression expression)
                => AddProjection(expression);

            internal static bool TestValidLambdaExpression(MethodCallExpression expression, out LambdaExpression lambda)
                => IsValidLambdaExpression(expression, out lambda);

            internal static Expression TestStripQuote(Expression expression)
                => StripQuote(expression);
        }

        [Fact]
        [Trait("Method", "AddFilter")]
        public void AddFilter_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            var translator = new TestQueryTranslator<IdEntity>(query, ClientOptions);
            Assert.Throws<NotSupportedException>(() => translator.TestAddFilter(null));
        }

        [Fact]
        [Trait("Method", "AddOrdering")]
        public void AddOrdering_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            var translator = new TestQueryTranslator<IdEntity>(query, ClientOptions);
            Assert.Throws<NotSupportedException>(() => translator.TestAddOrdering(null));
        }

        [Fact]
        [Trait("Method", "AddOrderByNode")]
        public void AddOrderByNode_Null_Returns()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            var translator = new TestQueryTranslator<IdEntity>(query, ClientOptions);
            translator.TestAddOrderByNode(null);
            Assert.Equal(0, translator.CountOrderings());
        }

        [Fact]
        [Trait("Method", "AddProjection")]
        public void AddProjection_Null_Throws()
        {
            var query = new DatasyncTableQuery<IdEntity>(Table);
            var translator = new TestQueryTranslator<IdEntity>(query, ClientOptions);
            Assert.Throws<NotSupportedException>(() => translator.TestAddProjection(null));
        }

        [Fact]
        [Trait("Method", "IsValidLambdaExpression")]
        public void IsValidLambdaExpression_Null_ReturnsFalse()
        {
            Assert.False(TestQueryTranslator<IdEntity>.TestValidLambdaExpression(null, out LambdaExpression lambda));
            Assert.Null(lambda);
        }

        [Fact]
        [Trait("Method", "StripQuote")]
        public void StripQuote_ReturnsUnquoted()
        {
            Expression<Func<IdEntity, bool>> expression = m => m.Id != null;
            var actual = TestQueryTranslator<IdEntity>.TestStripQuote(expression);
            Assert.Same(expression, actual);
        }

        [Fact]
        [Trait("Method", "StripQuote")]
        public void StripQuote_StripsQuoted()
        {
            Expression<Func<IdEntity, bool>> expression = m => m.Id != null;
            var quoted = Expression.Quote(expression);
            var actual = TestQueryTranslator<IdEntity>.TestStripQuote(quoted);
            Assert.Same(expression, actual);
        }
    }
}
