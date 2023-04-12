// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Datasync.NSwag.Test.Service;

/// <summary>
/// Startup for the MovieAPI Service used for testing
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test suite")]
internal class ServiceStartup
{
    /// <summary>
    /// Creates a new instance of the <see cref="Startup"/> class
    /// </summary>
    /// <param name="configuration">The application configuration</param>
    public ServiceStartup(IConfiguration configuration)
    {
        Configuration = configuration;
        DbConnection = new SqliteConnection("Data Source=:memory:");
        DbConnection.Open();
    }

    /// <summary>
    /// The application configuration
    /// </summary>
    public IConfiguration Configuration { get; }

    public SqliteConnection DbConnection { get; }

    /// <summary>
    /// The <see cref="ConfigureServices(IServiceCollection)"/> method is called by the host before the
    /// <see cref="Configure(IApplicationBuilder, IWebHostEnvironment)"/> method to configure application
    /// services.
    /// </summary>
    /// <param name="services">The service collection</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Options for the database setup
        var sqlConfiguration = new Action<DbContextOptionsBuilder>(options =>
        {
            options.UseSqlite(DbConnection);
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        });

        // Add the database context
        services.AddDbContext<ServiceDbContext>(sqlConfiguration, contextLifetime: ServiceLifetime.Transient, optionsLifetime: ServiceLifetime.Singleton);

        // Add authentication - force enabling the authentication pipeline.
        services.AddAuthentication(AzureAppServiceAuthentication.AuthenticationScheme)
            .AddAzureAppServiceAuthentication(options => options.ForceEnable = true);

        // Add Datasync-aware Controllers
        services.AddDatasyncControllers();

        services.AddOpenApiDocument(options =>
        {
            options.AddDatasyncProcessors();
        });
    }

    /// <summary>
    /// The <see cref="Configure(IApplicationBuilder)"/> method is used to specify how the app
    /// responds to HTTP requests.  The request pipeline is configured by adding middleware components to an
    /// <see cref="IApplicationBuilder"/> instance (provided by hosting)
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public static void Configure(IApplicationBuilder app)
    {
        app.UseOpenApi();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}

