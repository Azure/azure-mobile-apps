// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Datasync;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

WebApplicationBuilder appBuilder = WebApplication.CreateBuilder(args);

appBuilder.Services.AddSingleton<DbConnection>(_ =>
{
    SqliteConnection connection = new("Data Source=:memory:");
    connection.Open();
    return connection;
});

appBuilder.Services.AddDbContext<SqliteDbContext>((container, options) =>
{
    var connection = container.GetRequiredService<DbConnection>();
    options.UseSqlite(connection);
});

appBuilder.Services.AddDatasyncControllers();

using var app = appBuilder.Build();

app.UseRouting();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
    context.InitializeDatabase();
}

app.Run();

public partial class Program { }