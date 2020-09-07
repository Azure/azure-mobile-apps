// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Zumo.E2EServer.DataObjects;
using Microsoft.Zumo.E2EServer.Models;
using Microsoft.Zumo.Server;
using System;

namespace Microsoft.Zumo.E2EServer
{
    public class Startup
    {
        /// <summary>
        /// Instantiates a new <see cref="Startup"/> class
        /// </summary>
        /// <param name="configuration">The configuration for the application</param>
        /// <param name="environment">The environment the web host is operating within</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            WebHostEnvironment = environment;
        }

        /// <summary>
        /// The current application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The current environment
        /// </summary>
        public IWebHostEnvironment WebHostEnvironment { get; }

        /// <summary>
        /// Configures  the ASP.NET core services.  A service is a reusable component that provides
        /// app functionality.  Services are registered here and consumed across the app via dependency
        /// injection (DI).  Examples include ASP.NET Core controllers, database context, and authentication.
        /// </summary>
        /// <param name="services">The service collection to adjust.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            // Automapper
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MovieProfile());
                cfg.AddProfile(new RoundTripProfile());
            });
            services.AddSingleton(mapperConfiguration);

            // Database Context - uses SQL Server
            var dbConnectionString = Configuration.GetConnectionString("MS_TableConnectionString");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(dbConnectionString));

            // Azure Mobile Apps
            services.AddAzureMobileApps();
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application pipeline</param>
        /// <param name="env">The web host environment</param>
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
                endpoints.EnableAzureMobileApps();
            });

            // Initialize the database
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DbInitializer.Initialize(env, context);
        }
    }
}
