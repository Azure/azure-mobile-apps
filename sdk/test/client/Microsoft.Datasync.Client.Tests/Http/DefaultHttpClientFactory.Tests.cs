using Microsoft.Datasync.Client.Http;

namespace Microsoft.Datasync.Client.Tests.Http;

[ExcludeFromCodeCoverage]
public class DefaultHttpClientFactory_Tests
{
    [Fact]
    public void Ctor_CreatesDefaultOptions()
    {
        var sut = new DefaultHttpClientFactory();

        sut.Options.Endpoint.Scheme.Should().Be("null");
        sut.Options.HttpPipeline.Should().BeEmpty();
        sut.Options.HttpTimeout.ToString().Should().Be("00:01:40");
        sut.Options.InstallationId.Should().BeNull();
        sut.Options.ProtocolVersion.Should().Be("3.0.0");
        sut.Options.UserAgent.Should().StartWith("Datasync/");
    }

    [Fact]
    public void Ctor_CanStoreOptions()
    {
        var options = new HttpClientOptions
        {
            Endpoint = new Uri("http://localhost"),
            HttpPipeline = new[] { new HttpClientHandler() },
            HttpTimeout = TimeSpan.FromSeconds(10),
            InstallationId = "2d718a65-6c62-4186-ad29-b70c1a3dee1a",
            ProtocolVersion = "2.0.0",
            UserAgent = "MyUserAgent"
        };
        var sut = new DefaultHttpClientFactory(options);
        sut.Options.Should().BeSameAs(options);
        sut.Options.Endpoint.ToString().Should().Be("http://localhost/");
        sut.Options.HttpPipeline.Should().HaveCount(1);
        sut.Options.HttpTimeout.ToString().Should().Be("00:00:10");
        sut.Options.InstallationId.Should().Be("2d718a65-6c62-4186-ad29-b70c1a3dee1a");
        sut.Options.ProtocolVersion.Should().Be("2.0.0");
        sut.Options.UserAgent.Should().Be("MyUserAgent");
    }

    [Theory]
    [ClassData(typeof(EndpointTestCases))]
    public void CreateClient_CreatesClientWithCorrectEndpoint(EndpointTestCase testcase)
    {
        var options = new HttpClientOptions { Endpoint = new Uri(testcase.NormalizedEndpoint), InstallationId = "test-int-id", UserAgent = "test-user-agent" };
        var sut = new DefaultHttpClientFactory(options);
        var client = sut.CreateClient("default");

        client.Should().NotBeNull();
        client.BaseAddress.ToString().Should().Be(testcase.NormalizedEndpoint);
        client.DefaultRequestHeaders.Should().ContainKey("ZUMO-API-VERSION").WhoseValue.Should().BeEquivalentTo("3.0.0");
        client.DefaultRequestHeaders.Should().ContainKey("X-ZUMO-INSTALLATION-ID").WhoseValue.Should().BeEquivalentTo("test-int-id");
        client.DefaultRequestHeaders.Should().ContainKey("User-Agent").WhoseValue.Should().BeEquivalentTo("test-user-agent");
        client.DefaultRequestHeaders.Should().ContainKey("X-ZUMO-VERSION").WhoseValue.Should().BeEquivalentTo("test-user-agent");
    }

    [Fact]
    public void Dispose_DisposesCache()
    {
        var sut = new DefaultHttpClientFactory();
        sut._cache.CacheIsDisposed.Should().BeFalse();

        sut.Dispose();
        sut._cache.CacheIsDisposed.Should().BeTrue();
    }

    [Fact]
    public void CreatePipeline_NoHandlers_CreatesPipeline()
    {
        var options = new HttpClientOptions { HttpPipeline = Array.Empty<HttpMessageHandler>() };
        var factory = new DefaultHttpClientFactory(options);
        var handler = factory.CreatePipeline(options.HttpPipeline);

        handler.Should().BeOfType<HttpClientHandler>();
    }

    [Fact]
    public void CreatePipeline_C_CreatesPipeline()
    {
        var c = new HttpClientHandler();
        var options = new HttpClientOptions { HttpPipeline = new HttpMessageHandler[] { c } };
        var factory = new DefaultHttpClientFactory(options);
        var handler = factory.CreatePipeline(options.HttpPipeline);

        handler.Should().BeSameAs(c);
    }

    [Fact]
    public void CreatePipeline_B_CreatesPipeline()
    {
        var b = new MockDelegatingHandler();
        var options = new HttpClientOptions { HttpPipeline = new HttpMessageHandler[] { b } };
        var factory = new DefaultHttpClientFactory(options);
        var handler = factory.CreatePipeline(options.HttpPipeline);
        handler.Should().BeSameAs(b);
        b.InnerHandler.Should().BeOfType<HttpClientHandler>();
    }

    [Fact]
    public void CreatePipeline_BC_CreatesPipeline()
    {
        var b = new MockDelegatingHandler();
        var c = new HttpClientHandler();
        var options = new HttpClientOptions { HttpPipeline = new HttpMessageHandler[] { b, c } };
        var factory = new DefaultHttpClientFactory(options);
        var handler = factory.CreatePipeline(options.HttpPipeline);
        handler.Should().BeSameAs(b);
        b.InnerHandler.Should().BeSameAs(c);
    }

    [Fact]
    public void CreatePipeline_AB_CreatesPipeline()
    {
        var a = new MockDelegatingHandler();
        var b = new MockDelegatingHandler();
        var options = new HttpClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b } };
        var factory = new DefaultHttpClientFactory(options);
        var handler = factory.CreatePipeline(options.HttpPipeline);
        handler.Should().BeSameAs(a);
        a.InnerHandler.Should().BeSameAs(b);
        b.InnerHandler.Should().BeOfType<HttpClientHandler>();
    }

    [Fact]
    public void CreatePipeline_ABC_CreatesPipeline()
    {
        var a = new MockDelegatingHandler();
        var b = new MockDelegatingHandler();
        var c = new HttpClientHandler();

        var options = new HttpClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b, c } };
        var factory = new DefaultHttpClientFactory(options);
        var handler = factory.CreatePipeline(options.HttpPipeline);
        handler.Should().BeSameAs(a);
        a.InnerHandler.Should().BeSameAs(b);
        b.InnerHandler.Should().BeSameAs(c);
    }

    [Fact]
    public void CreatePipeline_CAB_ThrowsArgumentException()
    {
        var a = new MockDelegatingHandler();
        var b = new MockDelegatingHandler();
        var c = new HttpClientHandler();

        var options = new HttpClientOptions { HttpPipeline = new HttpMessageHandler[] { c, a, b } };
        var factory = new DefaultHttpClientFactory(options);
        Action act = () => factory.CreatePipeline(options.HttpPipeline);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreatePipeline_ACB_ThrowsArgumentException()
    {
        var a = new MockDelegatingHandler();
        var b = new MockDelegatingHandler();
        var c = new HttpClientHandler();

        var options = new HttpClientOptions { HttpPipeline = new HttpMessageHandler[] { a, c, b } };
        var factory = new DefaultHttpClientFactory(options);
        Action act = () => factory.CreatePipeline(options.HttpPipeline);
        act.Should().Throw<ArgumentException>();
    }

    internal class MockDelegatingHandler : DelegatingHandler
    {
    }
}
