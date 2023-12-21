using Microsoft.AspNetCore.Datasync.Models;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Tests.Converters;

[ExcludeFromCodeCoverage]
public class DateTimeConverter_Tests
{
    private readonly JsonSerializerOptions options = new DatasyncServiceOptions().JsonSerializerOptions;

    [Fact]
    public void Read_ValidString_ReturnsValidResult()
    {
        DateTime expected = DateTime.Parse("2023-12-23T12:23:20.010Z");
        const string json = "{\"dt\":\"2023-12-23T12:23:20.010Z\"}";
        Dictionary<string, DateTime> actual = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json, options);
        actual["dt"].Should().Be(expected);
    }

    [Fact]
    public void Read_NonString_ReturnsDefault()
    {
        const string json = "{\"dt\":null}";
        Action act = () => _ = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json, options);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Read_BadString_Throws()
    {
        const string json = "{\"dt\":\"not-date\"}";
        Action act = () => _ = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json, options);
        act.Should().Throw<FormatException>();
    }
}
