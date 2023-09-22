# Azure Mobile Apps

[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/azure/azure-mobile-apps/tree/main/LICENSE.txt)
[![Build Status](https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/Azure.azure-mobile-apps?repoName=Azure%2Fazure-mobile-apps&branchName=main)](https://dev.azure.com/devdiv/DevDiv/_build/latest?definitionId=14830&repoName=Azure%2Fazure-mobile-apps&branchName=main) [![NuGet Version][v1]](https://www.nuget.org/packages?q=Microsoft+Datasync)

Azure Mobile Apps (also known as the Microsoft Datasync Framework) is a set of client and server libraries for adding authenticated data access and offline synchronization to your mobile applications.

Currently, we support:

* Server: ASP.NET Core for .NET 6.
* Client: .NET Standard 2.0 and .NET 6.

The client platforms that support .NET Standard 2.0 and .NET 6 include:

* AvaloniaUI
* .NET MAUI
* Uno Platform
* Windows (UWP, WinUI3, WPF)
* Xamarin Forms
* Xamarin Native (Android and iOS)

Blazor and Unity are known to have issues with offline support since neither supports Sqlite natively.

To get started, take a look at [our documentation](https://docs.microsoft.com/en-us/azure/developer/mobile-apps/azure-mobile-apps/overview).

The older (v4.2.0 and earlier) libraries are retired and no longer supported.  You can find the source code in [the archive branch](https://github.com/Azure/azure-mobile-apps/tree/archive).

## Libraries

### .NET (sdk/dotnet)

The server-side library uses ASP.NET Core 6.0 and provides stores for in-memory data, LiteDb, and Entity Framework Core.

Released and supported versions of the library will be distributed by the normal [NuGet](https://www.nuget.org/) mechanism:

| Package | Version | Downloads |
|---------|---------|-----------|
| [Microsoft.AspNetCore.Datasync] | ![Core Library Version][v1] | ![Core Library Downloads][d1] |
| [Microsoft.AspNetCore.Datasync.Abstractions] | ![Abstractions Library Version][v2] | ![Abstractions Library Downloads][d2] |
| [Microsoft.AspNetCore.Datasync.EFCore] | ![EFCore Library Version][v3] | ![EFCore Library Downloads][d3] |
| [Microsoft.AspNetCore.Datasync.InMemory] | ![InMemory Library Version][v4] | ![InMemory Library Downloads][d4] |
| [Microsoft.AspNetCore.Datasync.LiteDb] | ![LiteDb Library Version][v5] | ![LiteDb Library Downloads][d5] |
| [Microsoft.AspNetCore.Datasync.NSwag] | ![NSwag Library Version][v6] | ![LiteDb Library Downloads][d6] |
| [Microsoft.AspNetCore.Datasync.Swashbuckle] | ![Swashbuckle Library Version][v7] | ![LiteDb Library Downloads][d7] |
| [Microsoft.Datasync.Client] | ![Client Library Version][vc1] | ![Client Library Downloads][dc1] |
| [Microsoft.Datasync.Client.SQLiteStore] | ![SQLiteStore Library Version][vc2] | ![SQLiteStore Library Downloads][dc2] |

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

## Documentation

You can find the latest documentation on [docs.microsoft.com](https://docs.microsoft.com/azure/developer/mobile-apps/azure-mobile-apps/overview).  Additional information will be added to the repository Wiki when appropriate.

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
[Microsoft.AspNetCore.Datasync.LiteDb]: https://www.nuget.org/packages/Microsoft.AspNetCore.Datasync.LiteDb
[Microsoft.AspNetCore.Datasync.NSwag]: https://www.nuget.org/packages/Microsoft.AspNetCore.Datasync.NSwag
[Microsoft.AspNetCore.Datasync.Swashbuckle]: https://www.nuget.org/packages/Microsoft.AspNetCore.Datasync.Swashbuckle
[Microsoft.Datasync.Client]: https://www.nuget.org/packages/Microsoft.Datasync.Client
[Microsoft.Datasync.Client.SQLiteStore]: https://www.nuget.org/packages/Microsoft.Datasync.Client.SQLiteStore

<!-- Images -->
[v1]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync
[v2]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.Abstractions
[v3]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.EFCore
[v4]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.InMemory
[v5]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.LiteDb
[v6]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.NSwag
[v7]: https://badgen.net/nuget/v/Microsoft.AspNetCore.Datasync.Swashbuckle
[vc1]: https://badgen.net/nuget/v/Microsoft.Datasync.Client
[vc2]: https://badgen.net/nuget/v/Microsoft.Datasync.Client.SQLiteStore

[d1]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync
[d2]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.Abstractions
[d3]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.EFCore
[d4]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.InMemory
[d5]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.LiteDb
[d6]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.NSwag
[d7]: https://badgen.net/nuget/dt/Microsoft.AspNetCore.Datasync.Swashbuckle
[dc1]: https://badgen.net/nuget/dt/Microsoft.Datasync.Client
[dc2]: https://badgen.net/nuget/dt/Microsoft.Datasync.Client.SQLiteStore
