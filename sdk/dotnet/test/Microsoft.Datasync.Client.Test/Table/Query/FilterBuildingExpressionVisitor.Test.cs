// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query;
using System.Diagnostics.CodeAnalysis;
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
            var result = FilterBuildingExpressionVisitor.Compile(null, ClientOptions);
            Assert.Null(result);
        }

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
    }
}
