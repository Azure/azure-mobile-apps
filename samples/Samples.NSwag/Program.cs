using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.NSwag;
using Microsoft.EntityFrameworkCore;
using Samples.NSwag.Db;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatasyncControllers();
builder.Services.AddOpenApiDocument(options =>
{
    options.AddDatasyncProcessors();
});

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.InitializeDatabaseAsync().ConfigureAwait(false);
}

// Swagger support
app.UseOpenApi();
app.UseSwaggerUi3();

// Configure and run the web service.
app.MapControllers();
app.Run();