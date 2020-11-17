# Create an Android app

This tutorial shows you how to add a cloud-based backend service to an Android mobile app by using an Azure mobile app backend.  You will create both a new mobile app backend and a simple *Todo list* Android app that stores app data in Azure.

Completing this tutorial is a prerequisite for all other Android tutorials about using the Mobile Apps feature in Azure App Service.

## Prerequisites

To complete this tutorial, you need the following:

* [Android Studio 4.1.1](https://developer.android.com/sdk/index.html).
* A suitable SDK, which can be installed from the [SDK Manager](https://developer.android.com/studio/intro/update).
  * Android 8.1 (Oreo) > Android SDK Platform 27
  * Android 8.1 (Oreo) > Google Play Intel x86 Atom System Image
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

Open the Android project (located at `samples/android`).  Edit the `Configuration.java` file to replace the `BackendUrl` with your backend URL.  For example, if your backend URL was `https://web-abcd1234.azurewebsites.net`, then the file would look like this:

```java
package com.example.zumoquickstart;

public class Configuration {
    /**
     * The Url to your ZUMO Backend
     */
    public static String BackendUrl = "https://web-abcd1234.azurewebsites.net";
}
```

Save the file, then build your application.

## Run the Android app

In the top bar, select the AVD that you created (for example, `Pixel 4 API 27`), then press the Run button next to the AVD selector.

Once the app starts, type some text, such as *Complete the tutorial*, then click 'Add'.  This will insert the text into the TodoItem SQL table you created earlier, and displays the text in the list.

![Quickstart Android](./media/startup.png)
