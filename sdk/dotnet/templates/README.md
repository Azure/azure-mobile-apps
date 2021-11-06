# Microsoft.Datasync.Client: A client for the Microsoft Datasync Framework

The Microsoft Datasync Framework (previously Azure Mobile Apps) is a client-server protocol that allows a client application to synchronize table data from a service-based database. From a purely technical perspective, the protocol is based on [OData](https://www.odata.org) to allow searching, sorting, and paging of results, plus a version-aware CRUD (Create-Read-Update-Delete) web API.

This package provides a template for creating an ASP.NET Core datasync server.

## How to install

``` bash
dotnet new -i Microsoft.AspNetCore.Datasync.Template.CSharp
```

## How to create a new service

``` bash
mkdir new-project
cd new-project
dotnet new datasync-server
```

## Asking questions

Please ask questions either on [Stack Overflow](https://stackoverflow.com/questions/tagged/azure-mobile-services) or [GitHub Discussions](https://github.com/Azure/azure-mobile-apps/discussions).

## Reporting bugs or requesting features

Please report bugs or request features through [GitHub Issues](https://github.com/Azure/azure-mobile-apps/issues).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit [https://cla.opensource.microsoft.com](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
