# Azure Mobile Apps

[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/azure/azure-mobile-apps/tree/main/LICENSE.txt)
[![Build Status](https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/Azure.azure-mobile-apps?repoName=Azure%2Fazure-mobile-apps&branchName=main)](https://dev.azure.com/devdiv/DevDiv/_build/latest?definitionId=14830&repoName=Azure%2Fazure-mobile-apps&branchName=main) [![NuGet Version][v1]](https://www.nuget.org/packages?q=Microsoft+Datasync)

Azure Mobile Apps is a set of client and server libraries for adding authenticated data access and offline synchronization to your mobile applications.

> **UPDATE**<br/>
> We are currently updating the Azure Mobile Apps libraries to ASP.NET Core, and will be updating the supported environments soon.

Currently, we support:

* ASP.NET Core for .NET 6
* .NET Standard 2.0 Client (Xamarin, WPF, etc.)

To get started, take a look at [our documentation](https://azure.github.io/azure-mobile-apps).

The following libraries have been archived and can be considered deprecated (links take you to the archive)

* Android Client
* iOS Client
* JavaScript Client
* Apache Cordova Client
* Node Server
* .NET Framework Server

> **What does deprecation mean?**<br/>
> The libraries that have been deprecated are still available in [the archive](https://github.com/azure/azure-mobile-apps/tree/archive), and documentation is available in [the documentation](https://azure.github.io/azure-mobile-apps/), but no further work will be done on these libraries, and any issues opened on these libraries will be closed as "won't fix".  The libraries are open-source, and you are welcome to fork them to adjust as you see fit.
>
> Note that the ASP.NET Core service uses OData v4 (with a backwards compatibility module for OData v3).  This means that older clients (such as those in the deprecation list) will not work in every situation, and additional configuration is required to support authentication.

## Libraries

### .NET (sdk/dotnet)

The .NET Library uses ASP.NET Core 6.0 and provides an in-memory store and an Entity Framework 6.0 based store.  You can download pre-release versions from [GitHub Releases](https://github.com/Azure/azure-mobile-apps/releases).

Released and supported versions of the library will be distributed by the normal [NuGet](https://www.nuget.org/) mechanism:

| Package | Version | Downloads |
|---------|---------|-----------|
| [Microsoft.AspNetCore.Datasync] | ![Core Library Version][v1] | ![Core Library Downloads][d1] |
| [Microsoft.AspNetCore.Datasync.Abstractions] | ![Abstractions Library Version][v2] | ![Abstractions Library Downloads][d2] |
| [Microsoft.AspNetCore.Datasync.EFCore] | ![EFCore Library Version][v3] | ![EFCore Library Downloads][d3] |
| [Microsoft.AspNetCore.Datasync.InMemory] | ![InMemory Library Version][v4] | ![InMemory Library Downloads][d4] |
| [Microsoft.Datasync.Client] | ![Client Library Version][v5] | ![Client Library Downloads][d5] |
| [Microsoft.Datasync.Client.SQLiteStore] | ![SQLiteStore Library Version][v6] | ![SQLiteStore Library Downloads][d6] |

## Templates

We provide a template for use with `dotnet new`.  The template pre-configures ASP.NET Core, Entity Framework Core, and the Datasync Server libraries.  To install the template:

```dotnetcli
dotnet new -i Microsoft.AspNetCore.Datasync.Template.CSharp
```

To create a server, use `dotnet new`:

```dotnetcli
mkdir My.Datasync.Server
cd My.Datasync.Server
dotnet new datasync-server
```

The Datasync Server template will be released to NuGet at the same time as the new libraries.

## Documentation

You can find the latest documentation on [docs.microsoft.com](https://docs.microsoft.com/azure/developer/mobile-apps/azure-mobile-apps/overview), and pre-release documentation in our docs directory.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.

<!-- Links -->
[Microsoft.AspNetCore.Datasync]: https://www.nuget.org/packages/Microsoft.AspNetCore.Datasync
[Microsoft.AspNetCore.Datasync.Abstractions]: https://www.nuget.org/packages/Microsoft.AspNetCore.Datasync.Abstractions
[Microsoft.AspNetCore.Datasync.EFCore]: https://www.nuget.org/packages/Microsoft.AspNetCore.Datasync.EFCore
[Microsoft.AspNetCore.Datasync.InMemory]: https://www.nuget.org/packages/Microsoft.AspNetCore.Datasync.InMemory
[Microsoft.Datasync.Client]: https://www.nuget.org/packages/Microsoft.Datasync.Client
[Microsoft.Datasync.Client.SQLiteStore]: https://www.nuget.org/packages/Microsoft.Datasync.Client.SQLiteStore

<!-- Images -->
[v1]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync
[v2]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.Abstractions
[v3]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.EFCore
[v4]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.InMemory
[v5]: https://badgen.net/nuget/v/Microsoft.Datasync.Client
[v6]: https://badgen.net/nuget/v/Microsoft.Datasync.Client.SQLiteStore

[d1]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync
[d2]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.Abstractions
[d3]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.EFCore
[d4]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.InMemory
[d5]: https://badgen.net/nuget/dt/Microsoft.Datasync.Client
[d6]: https://badgen.net/nuget/dt/Microsoft.Datasync.Client.SQLiteStore
