// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using aspnet6.Db;
using Microsoft.AspNetCore.Datasync;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatasyncControllers();

var app = builder.Build();

using (var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>())
{
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.MapGet("/error", () => Results.Problem("An error occurred.", statusCode: 500)).ExcludeFromDescription();
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
