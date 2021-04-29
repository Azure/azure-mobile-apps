// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using aspnetcore.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AzureMobile.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace aspnetcore
{
    public class Startup
    {
        /// <summary>
        /// <para>Construct a new <see cref="Startup"/> object.</para>
        /// <para>
        /// The <see cref="Startup"/> object is used to tell ASP.NET Core how to handle
        /// requests from client connections.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// The current configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The <see cref="ConfigureServices(IServiceCollection)"/> method is called by the host before the
        /// <see cref="Configure(IApplicationBuilder, IWebHostEnvironment)"/> method to configure application
        /// services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Initialize Entity Framework Core
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AppDbContext")));

            services.AddAuthentication("AppService")
                .AddAppServiceAuthentication((_) => { });

            // Add Web API Controllers
            services.AddControllers();

            // Add Azure Mobile Apps
            services.AddAzureMobile();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableAzureMobile();
            });
        }
    }
}
