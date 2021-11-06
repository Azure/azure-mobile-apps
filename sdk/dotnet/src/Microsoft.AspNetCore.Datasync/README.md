# Microsoft Datasync Framework for ASP.NET Core

The Microsoft Datasync Framework (previously Azure Mobile Apps) is a client-server protocol that allows a client application to synchronize table data from a service-based database. From a purely technical perspective, the protocol is based on [OData](https://www.odata.org) to allow searching, sorting, and paging of results, plus a version-aware CRUD (Create-Read-Update-Delete) web API.

This library must be paired with a suitable database driver.  Two drivers are supported:

* [In Memory](https://www.nuget.org/packages?q=Microsoft.AspNetCore.Datasync.InMemory)
* [Entity Framework Core](https://www.nuget.org/packages?q=Microsoft.AspNetCore.Datasync.EFCore)

## Preview Library

This library is provided in a "preview" state.  We do not feel it is ready for production use yet.

## Supported environments

This library support ASP.NET Core for .NET 5 and .NET 6.

## Typical ASP.NET 6 code

A typical `Program.cs` (using Entity Framework Core) might look like this:

``` csharp
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.EntityFrameworkCore;
using TodoAppService.NET6.Db;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatasyncControllers();

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.InitializeDatabaseAsync().ConfigureAwait(false);
}

// Configure and run the web service.
app.MapControllers();
app.Run();
```

You can then add a Data Transfer Object (DTO) that inherits from `EntityTableData` and create a controller:

``` csharp
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;
using TodoAppService.NET6.Db;

namespace TodoAppService.NET6.Controllers
{
    [Route("tables/todoitem")]
    public class TodoItemController : TableController<TodoItem>
    {
        public TodoItemController(AppDbContext context)
            : base(new EntityTableRepository<TodoItem>(context))
        {
        }
    }
}
```

A `dotnet new` template is available.  For information, see [Microsoft.AspNetCore.Datasync.Template.CSharp](https://www.nuget.org/packages?q=Microsoft.AspNetCore.Datasync.Template.CSharp).

The service can use standard ASP.NET Authentication and Authorization, and can be hosted anywhere, including Azure App Service and Azure Container Apps.

For more information, please review [our documentation](https://azure.github.io/azure-mobile-apps/howto/server/aspnetcore/).

## Asking questions

Please ask questions either on [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-mobile-services) or [GitHub Discussions](https://github.com/Azure/azure-mobile-apps/discussions).

## Reporting bugs or requesting features

Please report bugs or request features through [GitHub Issues](https://github.com/Azure/azure-mobile-apps/issues).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.opensource.microsoft.com](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
