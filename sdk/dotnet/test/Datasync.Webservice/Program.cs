// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Webservice
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public static class Program
    {
        /// <summary>
        /// Entry point for the normal (Kestrel) hosting provider.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
                .Build()
                .Run();
        }

        /// <summary>
        /// Creates an in-memory test server suitable for running the unit test suite.
        /// </summary>
        public static TestServer CreateTestServer()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseContentRoot(System.AppContext.BaseDirectory)
                .UseStartup<Startup>();

            return new TestServer(builder);
        }
    }
}
