// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.Datasync.Tests.Helpers;

/// <summary>
/// Sets up a web test that can be used to test the Datasync service
/// with an in-memory repository based on Sqlite.  The EF Core context
/// is in the Datasync.Common project.
/// </summary>
[ExcludeFromCodeCoverage]
public class DatasyncServiceFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseConfiguration(new ConfigurationBuilder().AddInMemoryCollection().Build())
            .UseEnvironment("Testing");
        base.ConfigureWebHost(builder);
    }
}