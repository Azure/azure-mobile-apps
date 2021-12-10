// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
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
        [Fact]
        [Trait("Method", "Compile")]
        public void Compile_Null_ReturnsNull()
        {
            var result = FilterBuildingExpressionVisitor.Compile(null, new DatasyncClientOptions());
            Assert.Null(result);
        }

        [Fact]
        [Trait("Method", "GetTableMemberName")]
        public void GetTableMemberName_NullExpression_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => FilterBuildingExpressionVisitor.GetTableMemberName(null, new DatasyncClientOptions()));
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
            Assert.Null(FilterBuildingExpressionVisitor.GetTableMemberName(sut, new DatasyncClientOptions()));
        }

        [Fact]
        [Trait("Method", "GetTableMemberName")]
        public void GetTableMemberName_NotParameter_ReturnsNull()
        {
            IdEntity obj = new();
            var sut = Expression.Field(Expression.Constant(obj), "StringField");
            Assert.Null(FilterBuildingExpressionVisitor.GetTableMemberName(sut, new DatasyncClientOptions()));
        }

        [Fact]
        [Trait("Method", "ResolvePropertyName")]
        public void ResolvePropertyName_ResolvesToJsonPropertyName()
        {
            var info = typeof(RenamedEntity).GetProperty("MpaaRating");
            var name = FilterBuildingExpressionVisitor.ResolvePropertyName(info, new DatasyncClientOptions());
            Assert.Equal("rating", name);
        }

        [Fact]
        [Trait("Method", "ResolvePropertyName")]
        public void ResolvePropertyName_ResolvesUsingNamingPolicy()
        {
            var info = typeof(ClientMovie).GetProperty("Title");
            var name = FilterBuildingExpressionVisitor.ResolvePropertyName(info, new DatasyncClientOptions());
            Assert.Equal("title", name);
        }

        [Fact]
        [Trait("Method", "ResolvePropertyName")]
        public void ResolvePropertyName_ResolvesUsingName()
        {
            var info = typeof(ClientMovie).GetProperty("Title");
            var serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            var options = new DatasyncClientOptions { SerializerOptions = serializerOptions };
            var name = FilterBuildingExpressionVisitor.ResolvePropertyName(info, options);
            Assert.Equal("Title", name);
        }
    }
}
