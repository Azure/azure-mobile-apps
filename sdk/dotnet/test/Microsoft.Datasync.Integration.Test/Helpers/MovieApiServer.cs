﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Integration.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Integration.Test
{
    /// <summary>
    /// A test host creator for live tests against an in-memory service.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    internal static class MovieApiServer
    {
        /// <summary>
        /// Creates a test service.
        /// </summary>
        internal static TestServer CreateTestServer()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Test")
                .UseContentRoot(System.AppContext.BaseDirectory)
                .UseStartup<MovieApiStartup>();
            var server = new TestServer(builder);

            // Initialize the database
            using var scope = server.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
            context.InitializeDatabase();

            return server;
        }
    }
}
