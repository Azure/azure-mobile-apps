using E2EServer.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace E2EServer
{
    public class Program
    {
        /// <summary>
        /// Kestrel version of the service
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<E2EDbContext>();
                DbInitializer.Initialize(context);
            }

            host.Run();
        }

        /// <summary>
        /// Test implementation of the service
        /// </summary>
        /// <returns>A test server</returns>
        public static TestServer GetTestServer()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseStartup<Startup>();

            var server = new TestServer(builder);

            using (var scope = server.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<E2EDbContext>();
                DbInitializer.Initialize(context);
            }

            return server;
        }
    }
}
