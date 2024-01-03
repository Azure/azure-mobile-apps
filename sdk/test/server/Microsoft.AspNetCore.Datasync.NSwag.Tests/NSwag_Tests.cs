using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Controllers;
using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Models;
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
        normalizedContent.Should().Be(expectedContent);
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