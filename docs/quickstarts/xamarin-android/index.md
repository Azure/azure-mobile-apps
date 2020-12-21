# Create a Xamarin.Android app

This tutorial shows you how to add a cloud-based backend service to an Android mobile app by using Xamarin and an Azure mobile app backend.  You will create both a new mobile app backend and a simple *Todo list* app that stores app data in Azure.

Completing this tutorial is a prerequisite for all other Xamarin Android tutorials about using the Mobile Apps feature in Azure App Service.

## Prerequisites

To complete this tutorial, you need the following:

* An appropriate IDE:
    * For Windows: install [Visual Studio 2019](https://docs.microsoft.com/xamarin/get-started/installation/windows).
    * For Mac: install [Visual Studio for Mac](https://docs.microsoft.com/visualstudio/mac/installation).

* An [Azure account](https://azure.microsoft.com/pricing/free-trial).
* The [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli).
    * [Log into your Azure account](https://docs.microsoft.com/cli/azure/authenticate-azure-cli) and [select a subscription](https://docs.microsoft.com/cli/azure/manage-azure-subscriptions-azure-cli) using the Azure CLI.
* An [Android Virtual Device](https://developer.android.com/studio/run/managing-avds), with the following settings:
    * Phone: Pixel 4 (includes Play Store)
    * System Image: Pie (API 28, x86, Google Play)

You can complete this tutorial on Mac or Windows.

## Download the Xamarin.Android quickstart project

The Xamarin.Android quickstart project is located in the `samples/xamarin-android` folder of the [azure/azure-mobile-apps](https://github.com/azure/azure-mobile-apps) GitHub repository.  You can [download the repository as a ZIP file](https://github.com/Azure/azure-mobile-apps/archive/master.zip), then unpack it.  The files will be created in the `azure-mobile-apps-master` folder.

Once downloaded, open a Terminal and change directory to the location of the files.

{!quickstarts/includes/quickstart-deploy-backend.md!}

## Configure the Xamarin.Android quickstart project

Open the `ZumoQuickstart` solution in Visual Studio (located at `samples/xamarin-android`).  Edit the `Constants.cs` class to replace the `BackendUrl` with your backend URL.  For example, if your backend URL was `https://zumo-abcd1234.azurewebsites.net`, then the file would look like this:

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

Ensure that the _Any CPU_ configuration is selected, and a suitable Android emulator is shown:

![Android Configuration](./media/android-configuration.png)

Press F5 to build and run the project.  The Android emulator will start, then Visual Studio will install the app, and finally the app will start.

Enter some text in the **Add New Item** field, then press enter or click the add item button.  This will add the item to the list.  Click on the item to set or clear the "completed" flag.

![Quickstart Android](./media/android-startup.png)

## Troubleshooting

If you have problems building the solution, run the NuGet package manager and update to the latest version of the `Microsoft.Azure.Mobile.Client` packages. Quickstart projects might not always include the latest versions.

Please note that all the support packages referenced in your Android project must have the same version. The [Azure Mobile Apps NuGet package](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client/) has `Xamarin.Android.Support.CustomTabs` dependency for Android platform, so if your project uses newer support packages you need to install this package with required version directly to avoid conflicts.
