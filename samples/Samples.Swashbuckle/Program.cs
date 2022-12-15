using Microsoft.AspNetCore.Datasync;
using Microsoft.EntityFrameworkCore;
using Samples.Swashbuckle.Db;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatasyncControllers();
builder.Services.AddSwaggerGen(options => { options.AddDatasyncControllers(); });
builder.Services.AddSwaggerGenNewtonsoftSupport();


var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.InitializeDatabaseAsync().ConfigureAwait(false);
}

app.UseSwagger();
app.UseSwaggerUI(options => 
{
    options.SwaggerEndpoint("swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// Configure and run the web service.
app.MapControllers();
app.Run();