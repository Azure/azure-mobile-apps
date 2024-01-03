// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Datasync.NSwag.Tests.Helpers;

[ExcludeFromCodeCoverage]
public class ServiceApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IRepository<KitchenSink>, InMemoryRepository<KitchenSink>>();
            services.AddSingleton<IRepository<TodoItem>, InMemoryRepository<TodoItem>>();

            services.AddOpenApiDocument(settings => settings.AddDatasyncProcessors());
        });

        builder.Configure(app =>
        {
            app.UseOpenApi();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        });
    }
}
