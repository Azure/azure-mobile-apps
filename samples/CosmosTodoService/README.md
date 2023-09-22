# Cosmos DB Sample for Azure Mobile Apps

This directory contains a sample backend service that implements the TodoItems backend using Cosmos DB as a backing store.  It is compatible with the [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/overview), a developer-centric CLI for provisioning and deploying applications on Azure.

## Pre-requisites

Before continuing, ensure you have:

* [Installed the Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd).
* [Installed the dotnet tool and SDK](https://dotnet.microsoft.com/download/dotnet/6.0).

## Provision and deploy the backend

To provision the resources and deploy the backend, use `azd up`:

``` bash
azd up
```

The process will ask you a few questions (including the environment name, which region you wish to deploy to, and which subscription you wish to use).  It will then provision an App Service and Cosmos DB and deploy the backend to the App Service.  You can find the bicep files for this process in the `infra` directory.

## Shut down the service

You can delete all Azure resources created with this template:

``` bash
azd down
```

You will be asked for confirmation before the process starts.

## Reporting issues and feedback

If you experience an issue, please [file an issue](https://github.com/Azure/azure-mobile-apps/issues).
