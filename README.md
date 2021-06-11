# Azure Mobile Apps

[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/azure/azure-mobile-apps/tree/main/LICENSE.txt)

Azure Mobile Apps is a set of client and server libraries for adding authenticated data access and offline synchronization to your mobile applications. 

> **UPDATE**<br/>
> We are currently updating the Azure Mobile Apps libraries to ASP.NET Core, and will be updating the supported environments soon.

Currently, we support:

* ASP.NET Framework on the server side (for security issues only)
* .NET Standard 2.0 Client (Xamarin, WPF, etc.)

To get started, take a look at [our documentation](https://azure.github.io/azure-mobile-apps).

The following libraries have been archived and can be considered deprecated (links take you to the archive)

* Android Client
* iOS Client
* JavaScript Client
* Apache Cordova Client
* Node Server

> **What does deprecation mean?**<br/>
> The libraries that have been deprecated are still available in [the archive](https://github.com/azure/azure-mobile-apps/tree/archive), and documentation is available in [the documentation](https://azure.github.io/azure-mobile-apps/), but no further work will be done on these libraries, and any issues opened on these libraries will be closed as "won't fix".  The libraries are open-source, and you are welcome to fork them to adjust as you see fit.
> 
> In particular, the upcoming ASP.NET Core service will implement ZUMO-API-VERSION 3.0.0 (whereas the Node and ASP.NET Framework implemented ZUMO-API-VERSION 2.0.0).  It is unlikely that the deprecated clients will work against the newer server versions.


## Libraries

### .NET (sdk/dotnet)

The .NET Library uses ASP.NET Core 5.0 and provides an in-memory store and an Entity Framework 5.0 based store.

> **TODO**
> Provide links to detailed instructions for ASP.NET Core when released.

## Templates

### dotnet new (templates/dotnet)

A template for `dotnet new` is provided to provide a base ASP.NET Core service.

> **TODO**
> Provide installation instructions for the template.

## Generating the documentation

This project uses [mkdocs](https://mkdocs.org) for documentation.  Documentation is written in Markdown.  To install the pre-requisites, use the following:

```bash
pip install mkdocs mkdocs-material pymdown-extensions markdown-include
```

To host the documentation locally, use:

```bash
mkdocs serve
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
