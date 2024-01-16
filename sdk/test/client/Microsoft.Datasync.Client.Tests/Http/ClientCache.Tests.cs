using Microsoft.Datasync.Client.Http;

namespace Microsoft.Datasync.Client.Tests.Http;

[ExcludeFromCodeCoverage]
public class ClientCache_Tests
{
    [Fact]
    public void Add_ReturnsValueIfInCache()
    {
        static string factory() => "factory-generated";
        var sut = new ClientCache<string, string>() { Timeout = TimeSpan.FromSeconds(3) };
        sut.Cache.TryAdd("test", "cached-value");

        var result = sut.Add("test", factory);
        result.Should().Be("cached-value");
    }

    [Fact]
    public void GetOrAdd_ReturnsCachedValue()
    {
        static string factory() => "factory-generated";
        var sut = new ClientCache<string, string>() { Timeout = TimeSpan.FromSeconds(3) };
        sut.Cache.TryAdd("test", "cached-value");

        var result = sut.GetOrAdd("test", factory);
        result.Should().Be("cached-value");
    }

    [Fact]
    public async Task Add_ThrowsTimeout_IfStuckInFactory()
    {
        static string stuckFactory() { Thread.Sleep(TimeSpan.FromSeconds(10)); return "factory-stuck"; }
        static string notStuckFactory() { return "factory-generated"; }
        var sut = new ClientCache<string, string>() { Timeout = TimeSpan.FromSeconds(1) };

        Task<string> t = Task.Run(() => sut.Add("test", stuckFactory));
        Action act = () => { Thread.Sleep(250); sut.Add("test", notStuckFactory); };
        act.Should().Throw<TimeoutException>();
        await t;        // Swallow the task.
    }

    [Fact]
    public void Dispose_ClearsCache()
    {
        var sut = new ClientCache<string, HttpClient>();
        sut.Cache.TryAdd("t1", new HttpClient());
        sut.Cache.TryAdd("t2", new HttpClient());

        sut.Cache.Should().HaveCount(2);
        sut.CacheIsDisposed.Should().BeFalse();

        sut.Dispose();

        sut.Cache.Should().BeEmpty();
        sut.CacheIsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Add_Throws_WhenDisposed()
    {
        var sut = new ClientCache<string, HttpClient>();
        sut.Cache.TryAdd("t1", new HttpClient());
        sut.Cache.TryAdd("t2", new HttpClient());
        sut.Dispose();

        Action act = () => sut.Add("t3", () => new HttpClient());
        act.Should().Throw<InvalidOperationException>();
    }
}
