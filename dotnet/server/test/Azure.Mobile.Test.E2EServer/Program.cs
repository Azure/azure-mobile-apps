using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Mobile.Test.E2EServer.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Azure.Mobile.Test.E2EServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    DbInitializer.Initialize(scope.ServiceProvider.GetRequiredService<TableServiceContext>());
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

    }
}
