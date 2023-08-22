using Azure.Identity;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.NSwag;
using Microsoft.EntityFrameworkCore;

using api.Db;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["AZURE_COSMOS_CONNECTION_STRING"] ?? throw new ApplicationException("CosmosDB Connection String not specified");
var databaseName = builder.Configuration["AZURE_COSMOS_DATABASE"] ?? throw new ApplicationException("CosmosDB Database Name not specified");

builder.Services.AddDbContext<TodoAppContext>(options => options.UseCosmos(connectionString, databaseName: databaseName));
builder.Services.AddDatasyncControllers();
builder.Services.AddOpenApiDocument(options => { options.AddDatasyncProcessors(); });
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoAppContext>();
    await context.InitializeDatabaseAsync().ConfigureAwait(false);
}

app.UseCors(policy => {
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
});

app.UseOpenApi();
app.UseSwaggerUi3();

app.MapControllers();
app.Run();
