using api.Extensions;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.NSwag;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["AZURE_COSMOS_CONNECTION_STRING"] ?? throw new ApplicationException("CosmosDB Connection String not specified");
var databaseName = builder.Configuration["AZURE_COSMOS_DATABASE"] ?? throw new ApplicationException("CosmosDB Database Name not specified");

builder.Services.AddCosmosDatasync(connectionString, databaseName);
builder.Services.AddDatasyncControllers();
builder.Services.AddOpenApiDocument(options => { options.AddDatasyncProcessors(); });
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var app = builder.Build();

app.UseCors(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
});

app.UseOpenApi();
app.UseSwaggerUi3();

app.MapControllers();
app.Run();