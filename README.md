# Azure Mobile Apps

[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/azure/azure-mobile-apps/tree/main/LICENSE.txt)

Azure Mobile Apps is a set of client and server libraries for adding authenticated data access and offline synchronization to your mobile applications. 

Currently supported environments:

| Server | Client |
|--------|--------|
| NodeJS | Andriod (Java) |
| ASP.NET Framework | iOS (Swift, Objective-C) |
|| Apache Cordova |
|| Xamarin (.NET Standard 2.0) |

Currently in development

| Server | Client |
| ASP.NET Core ||

To get started, take a look at [our documentation](https://azure.github.io/azure-mobile-apps).

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

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html). 
