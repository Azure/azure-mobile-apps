using Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Controllers;
using Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Models;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Tests;

[ExcludeFromCodeCoverage]
public class Swashbuckle_Tests : ServiceTest, IClassFixture<ServiceApplicationFactory>
{
    public Swashbuckle_Tests(ServiceApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Swashbuckle_GeneratesSwagger()
    {
        string swaggerDoc = await client.GetStringAsync("/swagger/v1/swagger.json");
        swaggerDoc.Should().NotBeNullOrWhiteSpace();
        string normalizedContent = swaggerDoc.Replace("\r\n", "\n").TrimEnd();
        string expectedContent = ReadEmbeddedResource();
        if (!expectedContent.Equals(normalizedContent))
        {
            WriteExternalFile("swashbuckle.json.out", normalizedContent);
        }
        normalizedContent.Should().Be(expectedContent);
    }

    [Theory]
    [InlineData(typeof(KitchenSink))]
    [InlineData(typeof(Object))]
    public void IsTableController_NonController_ReturnsFalse(Type type)
    {
        DatasyncDocumentFilter.IsTableController(type).Should().BeFalse();
    }

    [Fact]
    public void IsTableController_TableController_ReturnsTrue()
    {
        Type sut = typeof(KitchenSinkController);
        DatasyncDocumentFilter.IsTableController(sut).Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(KitchenSink))]
    [InlineData(typeof(Object))]
    public void GetEntityType_NotTableController_ThrowsArgumentException(Type type)
    {
        Action act = () => DatasyncDocumentFilter.GetEntityType(type);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetEntityType_TableController_ReturnsEntityType()
    {
        Type sut = typeof(KitchenSinkController);
        DatasyncDocumentFilter.GetEntityType(sut).Should().Be(typeof(KitchenSink));
    }

    [Fact]
    public void IsNotNull_Throws_OnNullArg()
    {
        Action act = () => DatasyncDocumentFilter.IsNotNull(null, "foo");
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("get", true)]
    [InlineData("Get", true)]
    [InlineData("GET", true)]
    [InlineData("delete", false)]
    [InlineData("post", false)]
    [InlineData("put", false)]
    public void IsGetOperation_Works(string method, bool expected)
    {
        DatasyncDocumentFilter.IsGetOperation(method).Should().Be(expected);
    }
}