# Create an Android app

This tutorial shows you how to add a cloud-based backend service to an Android mobile app by using an Azure mobile app backend.  You will create both a new mobile app backend and a simple *Todo list* Android app that stores app data in Azure.

Completing this tutorial is a prerequisite for all other Android tutorials about using the Mobile Apps feature in Azure App Service.

## Prerequisites

To complete this tutorial, you need the following:

* [Android Studio 4.1.1](https://developer.android.com/sdk/index.html).
* A suitable SDK, which can be installed from the [SDK Manager](https://developer.android.com/studio/intro/update).
  * A suitable emulator - one should be installed for you when you install Android Studio.
* An [Android Virtual Device](https://developer.android.com/studio/run/managing-avds), with the following settings:
  * Phone: Pixel 4 (includes Play Store)
  * System Image: Oreo (API 27, x86, Google Play)
* An [Azure account](https://azure.microsoft.com/pricing/free-trial).
* The [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli).
    * [Log into your Azure account](https://docs.microsoft.com/cli/azure/authenticate-azure-cli) and [select a subscription](https://docs.microsoft.com/cli/azure/manage-azure-subscriptions-azure-cli) using the Azure CLI.

You can complete this tutorial on Mac, Linux, or Windows.

## Download the Android quickstart project

The Android quickstart project is located in the `samples/android` folder of the [azure/azure-mobile-apps](https://github.com/azure/azure-mobile-apps) GitHub repository.  You can [download the repository as a ZIP file](https://github.com/Azure/azure-mobile-apps/archive/master.zip), then unpack it.  The files will be created in the `azure-mobile-apps-master` folder.

Once downloaded, open a Terminal and change directory to the location of the files.

{!quickstarts/includes/quickstart-deploy-backend.md!}

## Configure the Android quickstart project

Open the Android project in Android Studio (located at `samples/android`).  Edit the `Configuration.java` file to replace the `BackendUrl` with your backend URL.  For example, if your backend URL was `https://zumo-abcd1234.azurewebsites.net`, then the file would look like this:

```kotlin
package com.azure.mobile.zumoquickstart

/**
 * Constants used to configure the application
 */
class Constants {
    companion object {
        /**
         * The base URL of the backend service within Azure.
         */
        const val BackendUrl = "https://zumo-abcd1234.azurewebsites.net/"
    }
}
```

Save the file, then build your application.

## Run the Android app

In the top bar, select a suitable emulator, then press the Run button next to the AVD selector.

Once the app starts, press the **Add Item** floating action button, type some text, such as *Complete the tutorial*, then click **OK**.  This will insert the text into the TodoItem SQL table you created earlier, and display the text in the list.

![Quickstart Android](./media/startup.png)
