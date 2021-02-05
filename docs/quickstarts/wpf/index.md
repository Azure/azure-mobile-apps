# Create a Windows (WPF) app

This tutorial shows you how to add a cloud-based backend service to a Windows Presentation Framework (WPF) desktop app by using Azure Mobile Apps and an Azure mobile app backend.  You will create both a new mobile app backend and a simple *Todo list* app that stores app data in Azure.

Completing this tutorial is a prerequisite for all other WPF tutorials about using the Mobile Apps feature in Azure App Service.

## Prerequisites

To complete this tutorial, you need the following:

* [Visual Studio 2019](https://docs.microsoft.com/xamarin/get-started/installation/windows).
* An [Azure account](https://azure.microsoft.com/pricing/free-trial).
* The [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli).
    * [Log into your Azure account](https://docs.microsoft.com/cli/azure/authenticate-azure-cli) and [select a subscription](https://docs.microsoft.com/cli/azure/manage-azure-subscriptions-azure-cli) using the Azure CLI.

This tutorial can only be completed on a Windows system.

## Download the Windows (WPF) quickstart project

The WPF quickstart project is located in the `samples/wpf` folder of the [azure/azure-mobile-apps](https://github.com/azure/azure-mobile-apps) GitHub repository.  You can [download the repository as a ZIP file](https://github.com/Azure/azure-mobile-apps/archive/master.zip), then unpack it.  The files will be created in the `azure-mobile-apps-master` folder.

Once downloaded, open a Terminal and change directory to the location of the files.

{!quickstarts/includes/quickstart-deploy-backend.md!}

## Configure the WPF quickstart project

Open the `ZumoQuickstart` solution in Visual Studio (located at `samples/wpf`).  Edit the `Constants.cs` class to replace the `BackendUrl` with your backend URL.  For example, if your backend URL was `https://zumo-abcd1234.azurewebsites.net`, then the file would look like this:

``` csharp
namespace ZumoQuickstart
{
    /// <summary>
    /// Constants used to configure the application.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The base URL of the backend service within Azure.
        /// </summary>
        public static string BackendUrl { get; } = "https://zumo-abcd1234.azurewebsites.net";
    }
}
```

Save the file.

## Run the app

Ensure that the _Any CPU_ configuration is selected, and the _ZumoQuickstart_ target is shown:

![WPF Configuration](./media/wpf-configuration.png)

Press F5 to build and run the project. 

Enter some text in the **Add New Item** field, then press enter or click the add item icon.  This will add the item to the list.  Click on the item to set or clear the "completed" flag.

![WPF Android](./media/wpf-startup.png)
