using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.Zumo.Server;
using Todo.AspNetCore.Server.Database;

namespace Todo.AspNetCore.Server
{
    /// <summary>
    /// The Startup class for bootstrapping the ASP.NET Core application.  
    /// </summary>
    /// <remarks>
    /// See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-3.1
    /// </remarks>
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
            // Authentication
            services
                .AddMicrosoftWebApiAuthentication(Configuration.GetSection("Authentication"))
                .AddInMemoryTokenCaches();

            services.AddAuthorization();
            services.AddHttpContextAccessor();

            // Database Context - uses SQL Server
            var dbConnectionString = Configuration.GetConnectionString("MS_TableConnectionString");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(dbConnectionString));

            // API Controllers that return JSON payloads.
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

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
            DbInitializer.Initialize(context);
        }
    }
}
