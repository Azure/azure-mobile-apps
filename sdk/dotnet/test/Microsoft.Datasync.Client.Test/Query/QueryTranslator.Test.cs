// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Query;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Query
{
    /// <summary>
    /// Tests for some corner cases that *SHOULD* never happen
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class QueryTranslator_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "AddFilter")]
        public void AddFilter_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            var query = new TableQuery<IdEntity>(table);
            var translator = new TestQueryTranslator<IdEntity>(query, new DatasyncClientOptions());
            Assert.Throws<NotSupportedException>(() => translator.AddFilter(null));
        }

        [Fact]
        [Trait("Method", "AddOrdering")]
        public void AddOrdering_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            var query = new TableQuery<IdEntity>(table);
            var translator = new TestQueryTranslator<IdEntity>(query, new DatasyncClientOptions());
            Assert.Throws<NotSupportedException>(() => translator.AddOrdering(null));
        }

        [Fact]
        [Trait("Method", "AddOrderByNode")]
        public void AddOrderByNode_Null_Returns()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            var query = new TableQuery<IdEntity>(table);
            var translator = new TestQueryTranslator<IdEntity>(query, new DatasyncClientOptions());
            translator.AddOrderByNode(null);
            Assert.Equal(0, translator.CountOrderings());
        }

        [Fact]
        [Trait("Method", "AddProjection")]
        public void AddProjection_Null_Throws()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<IdEntity>();
            var query = new TableQuery<IdEntity>(table);
            var translator = new TestQueryTranslator<IdEntity>(query, new DatasyncClientOptions());
            Assert.Throws<NotSupportedException>(() => translator.AddProjection(null));
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
