using E2EServer.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

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
                try
                {
                    DbInitializer.Initialize(scope.ServiceProvider.GetRequiredService<E2EDbContext>());
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database");
                    throw;
                }
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
                try
                {
                    DbInitializer.Initialize(scope.ServiceProvider.GetRequiredService<E2EDbContext>());
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database");
                    throw;
                }
            }

            return server;
        }
    }
}
