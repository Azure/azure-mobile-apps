// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using FluentAssertions.Json;
using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Controllers;
using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json.Linq;
using NSwag;

namespace Microsoft.AspNetCore.Datasync.NSwag.Tests;

[ExcludeFromCodeCoverage]
public class NSwag_Tests : ServiceTest, IClassFixture<ServiceApplicationFactory>
{
    public NSwag_Tests(ServiceApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task NSwag_GeneratesSwagger()
    {
        string swaggerDoc = await client.GetStringAsync("/swagger/v1/swagger.json");
        swaggerDoc.Should().NotBeNullOrWhiteSpace();
        string normalizedContent = swaggerDoc.Replace("\r\n", "\n").TrimEnd();
        string expectedContent = ReadEmbeddedResource();
        if (!expectedContent.Equals(normalizedContent))
        {
            WriteExternalFile("nswag.json.out", normalizedContent);
        }

        JToken expectedJToken = JToken.Parse(expectedContent);
        JToken actualJToken = JToken.Parse(normalizedContent);
        actualJToken.Should().BeEquivalentTo(expectedJToken);
    }

    [Fact]
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Not needed")]
    public async Task NSwag_SwaggerIsValid()
    {
        string swaggerDoc = await client.GetStringAsync("/swagger/v1/swagger.json");
        swaggerDoc.Should().NotBeNullOrWhiteSpace();

        var result = new OpenApiStringReader().Read(swaggerDoc, out var diagnostic);
        diagnostic.SpecificationVersion.Should().Be(OpenApi.OpenApiSpecVersion.OpenApi3_0);

        result.Components.Schemas.Should().HaveCount(2).And.ContainKeys("KitchenSink", "TodoItem");

        result.Paths.Should().HaveCount(6);
        result.Paths["/tables/kitchenreader"].Should().HaveOperations(new string[] { "get" });
        result.Paths["/tables/kitchenreader/{id}"].Should().HaveOperations(new string[] { "get" });

        result.Paths["/tables/kitchensink"].Should().HaveOperations(new string[] { "get", "post" });
        result.Paths["/tables/kitchensink/{id}"].Should().HaveOperations(new string[] { "get", "put", "delete" });

        result.Paths["/tables/TodoItem"].Should().HaveOperations(new string[] { "get", "post" });
        result.Paths["/tables/TodoItem/{id}"].Should().HaveOperations(new string[] { "get", "put", "delete" });

        diagnostic.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("ETag", OpenApiParameterKind.Header, false)]
    [InlineData("If-Match", OpenApiParameterKind.Query, false)]
    [InlineData("If-Match", OpenApiParameterKind.Header, true)]
    public void IsHeader_MatchesProperly(string name, OpenApiParameterKind kind, bool expected)
    {
        OpenApiParameter parameter = new()
        {
            Name = name,
            Kind = kind
        };
        DatasyncOperationProcessor.IsHeader(parameter, "If-Match").Should().Be(expected);
    }

    [Theory]
    [InlineData(typeof(KitchenSink))]
    [InlineData(typeof(Object))]
    public void IsTableController_NonController_ReturnsFalse(Type type)
    {
        DatasyncOperationProcessor.IsTableController(type).Should().BeFalse();
    }

    [Fact]
    public void IsTableController_TableController_ReturnsTrue()
    {
        Type sut = typeof(KitchenSinkController);
        DatasyncOperationProcessor.IsTableController(sut).Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(KitchenSink))]
    [InlineData(typeof(Object))]
    public void GetEntityType_NotTableController_ThrowsArgumentException(Type type)
    {
        Action act = () => DatasyncOperationProcessor.GetEntityType(type);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetEntityType_TableController_ReturnsEntityType()
    {
        Type sut = typeof(KitchenSinkController);
        DatasyncOperationProcessor.GetEntityType(sut).Should().Be(typeof(KitchenSink));
    }
}