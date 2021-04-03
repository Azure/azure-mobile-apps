// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Common.Test.TestData;
using Microsoft.AzureMobile.Server;
using Microsoft.AzureMobile.Server.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.AzureMobile.WebService.Test
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
            // Add singletons for each InMemoryRepository to be used.
            var seedData = Movies.OfType<InMemoryMovie>();
            services.AddSingleton<IRepository<InMemoryMovie>>(new InMemoryRepository<InMemoryMovie>(seedData));

            // Add the controllers.  Note that we require a specific setup for Newtonsoft.JSON to work.
            // <b>DO NOT USE System.Text.Json IN Azure Mobile Apps APPLICATIONS</b>
            services
                .AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // Add Azure Mobile Apps
            services.AddAzureMobile();
        }

        /// <summary>
        /// The <see cref="Configure(IApplicationBuilder, IWebHostEnvironment)"/> method is used to specify how the app
        /// responds to HTTP requests.  The request pipeline is configured by adding middleware components to an
        /// <see cref="IApplicationBuilder"/> instance (provided by hosting)
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableAzureMobile();
            });
        }
    }
}
