// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common;
using Datasync.Common.TestData;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.InMemory;
using System.Diagnostics.CodeAnalysis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// In-Memory Repository - always required.
var inMemoryMovieRepository = new InMemoryRepository<InMemoryMovie>(Movies.OfType<InMemoryMovie>());
builder.Services.AddSingleton<IRepository<InMemoryMovie>>(inMemoryMovieRepository);

var inMemoryKitchenSinkRepository = new InMemoryRepository<InMemoryKitchenSink>();
builder.Services.AddSingleton<IRepository<InMemoryKitchenSink>>(inMemoryKitchenSinkRepository);

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
