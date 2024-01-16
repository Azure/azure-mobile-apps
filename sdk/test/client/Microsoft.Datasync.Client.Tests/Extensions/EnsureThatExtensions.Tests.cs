using EnsureThat;
using Microsoft.Datasync.Client.Extensions;

namespace Microsoft.Datasync.Client.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class EnsureThatExtensions_Tests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("", true)]
    [InlineData("null://local", false)]
    [InlineData("http://", false)]
    [InlineData("http://", true)]
    [InlineData("file://localhost/foo", false)]
    [InlineData("http://foo.azurewebsites.net", false)]
    [InlineData("http://foo.azure-api.net", false)]
    [InlineData("http://[2001:db8:0:b:0:0:0:1A]", false)]
    [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000", false)]
    [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false)]
    [InlineData("http://10.0.0.8", false)]
    [InlineData("http://10.0.0.8:3000", false)]
    [InlineData("http://10.0.0.8:3000/myapi", false)]
    [InlineData("foo/bar", true)]
    [Trait("Method", "IsValidEndpoint(Uri,string)")]
    public void IsValidEndpoint_UriParam_ThrowsWhenInvalid(string uri, bool isRelative)
    {
        Action act = () => Ensure.That(new Uri(uri, isRelative ? UriKind.Relative : UriKind.Absolute), "uri").IsValidEndpoint();
        act.Should().Throw<UriFormatException>();
    }

    [Theory]
    [InlineData("http://localhost/tables/foo")]
    [InlineData("http://devbox.local/tables/foo")]
    [InlineData("http://devbox.local:5000/tables/foo")]
    [InlineData("http://[::1]/tables/foo")]
    [InlineData("https://foo.azurewebsites.net/tables/foo")]
    [InlineData("https://foo.azure-api.net/tables/foo")]
    [InlineData("https://foo.azurewebsites.net:5001/tables/foo")]
    public void IsValidEndpoint_UriParam_DoesNotThrowWhenValid(string uri)
    {
        Action act = () => Ensure.That(new Uri(uri, UriKind.Absolute), "uri").IsValidEndpoint();
        act.Should().NotThrow();
    }
}
