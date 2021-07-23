// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Webservice
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Startup
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Startup"/> class
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// The application configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The <see cref="ConfigureServices(IServiceCollection)"/> method is called by the host before the
        /// <see cref="Configure(IApplicationBuilder, IWebHostEnvironment)"/> method to configure application
        /// services.
        /// </summary>
        /// <param name="services">The service collection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Implement logging!
            // services.AddLogging(options => options.AddConsole());

            // Add singletons for each InMemoryRepository to be used.
            var seedData = Movies.OfType<InMemoryMovie>();
            services.AddSingleton<IRepository<InMemoryMovie>>(new InMemoryRepository<InMemoryMovie>(seedData));

            var softData = Movies.OfType<SoftMovie>();
            softData.ForEach(m => m.Deleted = m.Rating == "R");
            services.AddSingleton<IRepository<SoftMovie>>(new InMemoryRepository<SoftMovie>(softData));

            // Add authentication - force enabling the authentication pipeline.
            services.AddAuthentication(AzureAppServiceAuthentication.AuthenticationScheme)
                .AddAzureAppServiceAuthentication(options => options.ForceEnable = true);

            // Add Datasync-aware Controllers
            services.AddDatasyncControllers();
        }

        /// <summary>
        /// The <see cref="Configure(IApplicationBuilder)"/> method is used to specify how the app
        /// responds to HTTP requests.  The request pipeline is configured by adding middleware components to an
        /// <see cref="IApplicationBuilder"/> instance (provided by hosting)
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableTableControllers();
            });
        }
    }
}
