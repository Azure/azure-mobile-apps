# Azure Mobile Apps: .NET Client SDK

With Azure Mobile Apps you can add a scalable backend to your connected client applications in minutes.

## Getting Started

The Azure Mobile Apps .NET Client code is part of Azure Mobile Apps - an offline capable data service.  To use, add the [Microsoft.Azure.Mobile.Client](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client/) package, and optionally, the [Microsoft.Azure.Mobile.Client.SQLiteStore](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client.SQLiteStore) packages to your project.

If you are new to Azure Mobile Apps, you can get started by following our tutorials for connecting to your hosted cloud backend with a [Xamarin.Forms client](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-xamarin-forms-get-started/) or [Windows client](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-windows-store-dotnet-get-started/).  To learn more about the client library, see [How to use the managed client for Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-dotnet-how-to-use-client-library/).

## Supported platforms

* .NET Standard 2.0
* Xamarin.Android 9.0 (up to Android API level 29)
* Xamarin.iOS 1.0 (up to iOS 14.0)
* UAP 10.0 (builds 16299 - 19041)

## Building the Library

### Prerequisites

The SDK requires Visual Studio 2019.

### Download Source Code

To get the source code of our SDKs and samples via **git** just type:

    git clone https://github.com/Azure/azure-mobile-apps-net-client.git
    cd ./azure-mobile-apps-net-client


### Building and Referencing the SDK

1. Open the `Microsoft.Azure.Mobile.Client.sln` solution file in Visual Studio 2019.
2. Use Solution -> Restore NuGet Packages...
3. Press F6 to build the solution.

### Running the Unit Tests

The following test suites under the 'unittests' directory contain the unit tests:

* MobileClient.Tests
* SQLiteStore.Tests

You can run the unit tests using the xUnit test runner.  Ensure you run the unit tests prior to submitting a PR.

### Running the E2E Tests

Before running the E2E Test Suites, you must deploy the E2E Test Server, which can be obtained from the [azure/azure-mobile-apps-net-server](https://github.com/azure/azure-mobile-apps-net-server) repository.

* Download the [repository](https://github.com/azure/azure-mobile-apps-net-server)
* Open the `ServerSDK` solution.
* Build the `ZumoE2EServerApp` project.
* Create an Azure App Service, with a connected SQL Azure database.
* Set the `MS_TableConnectionString` app setting in your Azure App Service to the connection string for the SQL Azure database.
* Deploy the `ZumoE2EServerApp` to the App Service.

Once complete, you can run the E2E tests.  Compile one of the projects (for iOS or Android) in the `e2etests` folder, and run it on a device (which can be an emulator or simulator, if you wish).

## Future of Azure Mobile Apps

Microsoft is committed to fully supporting Azure Mobile Apps, including **support for the latest OS release, bug fixes, documentation improvements, and community PR reviews**. Please note that the product team is **not currently investing in any new feature work** for Azure Mobile Apps. We highly appreciate community contributions to all areas of Azure Mobile Apps.

## Useful Resources

* [Quickstarts](https://docs.microsoft.com/azure/developer/mobile-apps/azure-mobile-apps/overview).
* [Azure Mobile Developer Center](https://docs.microsoft.com/azure/developer/mobile-apps/).
* StackOverflow: tag [azure-mobile-services](http://stackoverflow.com/questions/tagged/azure-mobile-services).
* [Instructions on enabling VisualStudio to load symbols from SymbolSource](http://www.symbolsource.org/Public/Wiki/Using)

## Contribute Code or Provide Feedback

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library, please file an issue in the [Issues](https://github.com/Azure/azure-mobile-apps-net-client/issues) section of the project.
