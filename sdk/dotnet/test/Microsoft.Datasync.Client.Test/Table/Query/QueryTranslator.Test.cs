// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Table.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Query
{
    /// <summary>
    /// Tests for some corner cases that *SHOULD* never happen
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class QueryTranslator_Tests : BaseTest
    {
        #region Test Setup
        private DatasyncClient MockClient { get; }
        private DatasyncTable<IdEntity> Table { get; }

        public QueryTranslator_Tests()
        {
            MockClient = CreateClientForMocking();
            Table = MockClient.GetTable<IdEntity>("movies") as DatasyncTable<IdEntity>;
        }
        #endregion

        /// <summary>
        /// A duplicate of the QueryTranslator that allows access to internal protected methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
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
            Assert.False(ExpressionExtensions.IsValidLambdaExpression(null, out LambdaExpression lambda));
            Assert.Null(lambda);
        }

        [Fact]
        [Trait("Method", "StripQuote")]
        public void StripQuote_ReturnsUnquoted()
        {
            Expression<Func<IdEntity, bool>> expression = m => m.Id != null;
            var actual = expression.StripQuote();
            Assert.Same(expression, actual);
        }

        [Fact]
        [Trait("Method", "StripQuote")]
        public void StripQuote_StripsQuoted()
        {
            Expression<Func<IdEntity, bool>> expression = m => m.Id != null;
            var quoted = Expression.Quote(expression);
            var actual = expression.StripQuote();
            Assert.Same(expression, actual);
        }
    }
}
