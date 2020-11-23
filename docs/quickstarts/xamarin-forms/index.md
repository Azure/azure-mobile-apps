# Create a Xamarin.Forms app

This tutorial shows you how to add a cloud-based back-end service to a Xamarin.Forms mobile app by using the Mobile Apps feature of Azure App Service as the back end. You create both a new Mobile Apps back end and a simple to-do list Xamarin.Forms app that stores app data in Azure.

Completing this tutorial is a prerequisite for all other Mobile Apps tutorials for Xamarin.Forms.

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
  * System Image: Oreo (API 27, x86, Google Play)
* If compiling for iOS, you must have an available Mac.
  * Install [XCode](https://itunes.apple.com/us/app/xcode/id497799835?mt=12)
  * You must manually open Xcode after installing so that it can add any additional components.
  * If completing the tutorial on Windows, follow the guide to [Pair to Mac](https://docs.microsoft.com/xamarin/ios/get-started/installation/windows/connecting-to-mac/).
  * 

You can complete this tutorial on Mac or Windows.  If you are compiling for iOS, you must have a Mac available.

## Download the Xamarin.Forms quickstart project

The Android quickstart project is located in the `samples/xamarin-forms` folder of the [azure/azure-mobile-apps](https://github.com/azure/azure-mobile-apps) GitHub repository.  You can [download the repository as a ZIP file](https://github.com/Azure/azure-mobile-apps/archive/master.zip), then unpack it.  The files will be created in the `azure-mobile-apps-master` folder.

Once downloaded, open a Terminal and change directory to the location of the files.

{!quickstarts/includes/quickstart-deploy-backend.md!}

## Configure the Xamarin.Forms quickstart project

Open the Xamarin.Forms solution in Visual Studio.  Edit the `Constants.cs` file in the `ZumoQuickstart` project.  Replace the contents of the `BackendUrl` variable with your backend URL.  For example, if your backend URL was `https://web-abcd1234.azurewebsites.net`, then the file would look like this:

```csharp
namespace ZumoQuickstart
{
    public static class Constants
    {
        public static string BackendUrl = @"https://web-abcd1234.azurewebsites.net";
    }
}
```

Save the file, then build your application

## Run the Android app

In this section, you run the Xamarin.Android project.  You can skip this section if you are not working with Android devices.

1. Right-click the Android project, then select **Set as Startup Project**.
2. Press the **F5** key or click the **Start** button.

The app will be built.  The Android emulator will then start and the app will start running.  In the app, type meaningful text, such as _Learn Xamarin_, then select the plus sign to add the record.

![Android to-do app](./media/android-startup.png)

## (Optional) Run the iOS app

In this section, you run the Xamarin.iOS project.  You can skip this section if you are not working with iOS devices.

1. Right-click the iOS project, then select **Set as Startup Project**.
2. Press the **F5** key or click the **Start** button.

The app will be built.  The iOS simulator will then start and the app will start running.  In the app, type meaningful text, such as _Learn Xamarin_, then select the plus sign to add the record.

![iOS to-do app](./media/ios-startup.png)

> **NOTE**
> You will find the code that accesses your Azure Mobile Apps backend in the `TodoItemManager.cs` file within the shared code project in the solution.

## Troubleshooting

If you have problems building the solution, run the NuGet package manager and update to the latest version of `Xamarin.Forms`, and in the Android project, update the `Xamarin.Android` support packages. Quickstart projects might not always include the latest versions.

Please note that all the support packages referenced in your Android project must have the same version. The [Azure Mobile Apps NuGet package](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client/) has `Xamarin.Android.Support.CustomTabs` dependency for Android platform, so if your project uses newer support packages you need to install this package with required version directly to avoid conflicts.
