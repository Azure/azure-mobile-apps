// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.InMemory;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

// In-Memory Repository - always required.
var repository = new InMemoryRepository<InMemoryMovie>(Movies.OfType<InMemoryMovie>());
builder.Services.AddSingleton<IRepository<InMemoryMovie>>(repository);

// Add Controllers.
builder.Services.AddDatasyncControllers();

// Build the application pipeline.

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Run the service.
app.Run();

// For testing with WebApplicationFactory
[ExcludeFromCodeCoverage]
public partial class Program { }
