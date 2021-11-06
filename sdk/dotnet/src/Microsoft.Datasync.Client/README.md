# Microsoft.Datasync.Client: A client for the Microsoft Datasync Framework

The Microsoft Datasync Framework (previously Azure Mobile Apps) is a client-server protocol that allows a client application to synchronize table data from a service-based database. From a purely technical perspective, the protocol is based on [OData](https://www.odata.org) to allow searching, sorting, and paging of results, plus a version-aware CRUD (Create-Read-Update-Delete) web API.

This library provides the client access library for a v5.0.0 based ASP.NET Core datasync service.

## Supported Environments

The following runtime environments are tested:

* .NET Standard 2.0
* Xamarin.iOS
* Xamarin.Android
* Xamarin.Forms

Other environments may be used, but are not guaranteed to work.  In particular, Blazor is not supported at this time.  You can find complete samples for each environment on [our GitHub repository](https://github.com/azure/azure-mobile-apps).

## Preview Library

This library is provided in a "preview" state.  We do not feel it is ready for production use yet.

## How to Use

This library provides online capabilities only at this time. First, create a model class for transfer:

``` csharp
``` csharp
using Microsoft.AspNetCore.Datasync.EFCore;
using System.ComponentModel.DataAnnotations;

namespace TodoAppService.NET6.Db
{
    public class TodoItem : DatasyncClientData
    {
        public string Title { get; set; } = "";

        public bool IsComplete { get; set; }
    }
}
```

The model should match what is on the service, although you may decide to drop fields that are not of interest. 

Create a client:

``` csharp
var client = new DatasyncClient(serviceUri);
```

Then create a table reference:

``` csharp
var table = client.GetTable<TodoItem>();
```

You can now use standard async CRUDL operations:

``` csharp
var itemsEnumerable = table.ToAsyncEnumerable();
await foreach (var item in items) 
{
  // Process the item
}
```

The table supports a subset of [LINQ] that supports OData v4:

``` csharp
var itemsEnumerable = table.Where(m => !m.IsComplete).OrderBy(m => m.UpdatedAt).ToAsyncEnumerable();
```

For more information, please review [our documentation](https://azure.github.io/azure-mobile-apps/howto/server/aspnetcore/).

## Asking questions

Please ask questions either on [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-mobile-services) or [GitHub Discussions](https://github.com/Azure/azure-mobile-apps/discussions).

## Reporting bugs or requesting features

Please report bugs or request features through [GitHub Issues](https://github.com/Azure/azure-mobile-apps/issues).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.opensource.microsoft.com](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.


