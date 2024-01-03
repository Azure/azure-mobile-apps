// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Tests.Helpers;

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

            services.AddSwaggerGen(options => options.AddDatasyncControllers());
        });

        builder.Configure(app =>
        {
            app.UseSwagger();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        });
    }
}
