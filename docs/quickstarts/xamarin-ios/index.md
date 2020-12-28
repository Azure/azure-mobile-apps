# Create a Xamarin.iOS app

This tutorial shows you how to add a cloud-based backend service to an iOS mobile app by using Xamarin.iOS and an Azure mobile app backend.  You will create both a new mobile app backend and a simple *Todo list* app that stores app data in Azure.

Completing this tutorial is a prerequisite for all other Xamarin.iOS tutorials about using the Mobile Apps feature in Azure App Service.

## Prerequisites

To complete this tutorial, you need the following:

* An appropriate IDE:
    * For Windows: install [Visual Studio 2019](https://docs.microsoft.com/xamarin/get-started/installation/windows).
    * For Mac: install [Visual Studio for Mac](https://docs.microsoft.com/visualstudio/mac/installation).
* An [Azure account](https://azure.microsoft.com/pricing/free-trial).
* The [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli).
    * [Log into your Azure account](https://docs.microsoft.com/cli/azure/authenticate-azure-cli) and [select a subscription](https://docs.microsoft.com/cli/azure/manage-azure-subscriptions-azure-cli) using the Azure CLI.
* An available Mac.
    * Install [XCode](https://itunes.apple.com/us/app/xcode/id497799835?mt=12)
    * You must manually open Xcode after installing so that it can add any additional components.
    * Once open, select **XCode Preferences...** > **Components**, and install an iOS simulator.
    * If completing the tutorial on Windows, follow the guide to [Pair to Mac](https://docs.microsoft.com/xamarin/ios/get-started/installation/windows/connecting-to-mac/).

You can complete this tutorial on Mac or Windows, but you must have a Mac available for iOS compilations.

## Download the Xamarin.iOS quickstart project

The Xamarin.Forms quickstart project is located in the `samples/xamarin-ios` folder of the [azure/azure-mobile-apps](https://github.com/azure/azure-mobile-apps) GitHub repository.  You can [download the repository as a ZIP file](https://github.com/Azure/azure-mobile-apps/archive/master.zip), then unpack it.  The files will be created in the `azure-mobile-apps-master` folder.

Once downloaded, open a Terminal and change directory to the location of the files.

{!quickstarts/includes/quickstart-deploy-backend.md!}

## Configure the Xamarin.iOS quickstart project

Open the `ZumoQuickstart` solution in Visual Studio (located at `samples/xamarin-ios`). Edit the `Constants.cs` class to replace the `BackendUrl` with your backend URL.  For example, if your backend URL was `https://zumo-abcd1234.azurewebsites.net`, then the file would look like this:

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

## Run the iOS app

> **NOTE**: If you are running Visual Studio on Windows, you **MUST** follow the guide to [Pair to Mac](https://docs.microsoft.com/xamarin/ios/get-started/installation/windows/connecting-to-mac/).  You will receive errors when compiling or running iOS applications without a paired Mac.

The "start" button in the top ribbon may show an iOS device.  Ensure that the _iPhoneSimulator_ configuration is selected:

![iOS Configuration](./media/ios-configuration.png)

Press F5 to build and run the project.  The iOS simulator will start, then Visual Studio will install the app, and finally the app will start.  

Enter some text in the **Add New Item** field, then press enter or click the add item button.  This will add the item to the list.  Click on the item to set or clear the "completed" flag.

![Quickstart iOS](./media/ios-startup.png)
