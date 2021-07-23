// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Query
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class FilterBuildingExpressionVisitor_Tests : BaseTest
    {
        #region Compile
        [Fact]
        [Trait("Method", "Compile")]
        public void Compile_Null_ReturnsNull()
        {
            var result = FilterBuildingExpressionVisitor.Compile(null, ClientOptions);
            Assert.Null(result);
        }
        #endregion

        #region GetTableMemberName
        [Fact]
        [Trait("Method", "GetTableMemberName")]
        public void GetTableMemberName_NullExpression_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => FilterBuildingExpressionVisitor.GetTableMemberName(null, ClientOptions));
        }

        [Fact]
        [Trait("Method", "GetTableMemberName")]
        public void GetTableMemberName_NullOptions_Throws()
        {
            var sut = Expression.Constant(1, typeof(int));
            Assert.Throws<ArgumentNullException>(() => FilterBuildingExpressionVisitor.GetTableMemberName(sut, null));
        }

        [Fact]
        [Trait("Method", "GetTableMemberName")]
        public void GetTableMemberName_NonMember_ReturnsNull()
        {
            var sut = Expression.Constant(1, typeof(int));
            Assert.Null(FilterBuildingExpressionVisitor.GetTableMemberName(sut, ClientOptions));
        }

        [Fact]
        [Trait("Method", "GetTableMemberName")]
        public void GetTableMemberName_NotParameter_ReturnsNull()
        {
            IdEntity obj = new();
            var sut = Expression.Field(Expression.Constant(obj), "StringField");
            Assert.Null(FilterBuildingExpressionVisitor.GetTableMemberName(sut, ClientOptions));
        }
        #endregion

        #region ResolvePropertyName
        [Fact]
        [Trait("Method", "ResolvePropertyName")]
        public void ResolvePropertyName_ResolvesToJsonPropertyName()
        {
            var info = typeof(Movie).GetProperty("MpaaRating");
            var name = FilterBuildingExpressionVisitor.ResolvePropertyName(info, ClientOptions);
            Assert.Equal("rating", name);
        }

        [Fact]
        [Trait("Method", "ResolvePropertyName")]
        public void ResolvePropertyName_ResolvesUsingNamingPolicy()
        {
            var info = typeof(Movie).GetProperty("Title");
            var name = FilterBuildingExpressionVisitor.ResolvePropertyName(info, ClientOptions);
            Assert.Equal("title", name);
        }

        [Fact]
        [Trait("Method", "ResolvePropertyName")]
        public void ResolvePropertyName_ResolvesUsingName()
        {
            var info = typeof(Movie).GetProperty("Title");
            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            var options = new DatasyncClientOptions { SerializerOptions = serializerOptions };
            var name = FilterBuildingExpressionVisitor.ResolvePropertyName(info, options);
            Assert.Equal("Title", name);
        }
        #endregion
    }
}
