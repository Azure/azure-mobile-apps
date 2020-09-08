// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Zumo.E2EServer
{
    public class Program
    {
        /// <summary>
        /// Main entrypoint
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// When the application enters through the test suite.
        /// </summary>
        /// <returns></returns>
        public static AspNetCore.TestHost.TestServer GetTestServer()
        {
            var applicationBasePath = System.AppContext.BaseDirectory;
            var builder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseContentRoot(applicationBasePath)
                .UseStartup<Startup>();

            var server = new AspNetCore.TestHost.TestServer(builder);

            return server;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
            });
    }
}
