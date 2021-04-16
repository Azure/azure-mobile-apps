# Azure Mobile Apps

[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/azure/azure-mobile-apps/tree/main/LICENSE.txt)

Azure Mobile Apps is a set of client and server libraries for adding authenticated data access and offline synchronization to your mobile applications.  The server side is a Node.js or ASP.NET Framework web API that runs on [Azure App Service](https://azure.microsoft.com/services/app-service/).  The client side contains everything necessary to provide offline sync for your Android, iOS, UWP, or Xamarin application.

To get started, take a look at [our documentation](https://azure.github.io/azure-mobile-apps).

## Libraries

### [.NET Core 5](sdk/dotnet/README.md)

[![.NET Library](https://github.com/Azure/azure-mobile-apps/actions/workflows/build-dotnet-library.yml/badge.svg?branch=main)](https://github.com/Azure/azure-mobile-apps/actions/workflows/build-dotnet-library.yml)

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

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html). 
