using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Azure.Mobile.Common.Test
{
    public static class TestHost
    {
        public static TestServer GetTestServer()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseStartup<TestStartup>();
            return new TestServer(builder);
        }
    }
}
