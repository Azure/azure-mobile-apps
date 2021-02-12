# Create an Apache Cordova app

This tutorial shows you how to add a cloud-based backend service to an Apache Cordova cross-platform app by using Azure Mobile Apps and an Azure mobile app backend.  You will create both a new mobile app backend and a simple *Todo list* app that stores app data in Azure.

Completing this tutorial is a prerequisite for all other Apache Cordova tutorials about using the Mobile Apps feature in Azure App Service.

## Prerequisites

To complete this tutorial, you need the following:

* [A working Apache Cordova 8.1.2 installation](https://cordova.apache.org/docs/en/latest/).
* A text editor (such as [Visual Studio Code](https://visualstudio.com/code)).
* An [Azure account](https://azure.microsoft.com/pricing/free-trial).
* The [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli).
    * [Log into your Azure account](https://docs.microsoft.com/cli/azure/authenticate-azure-cli) and [select a subscription](https://docs.microsoft.com/cli/azure/manage-azure-subscriptions-azure-cli) using the Azure CLI.

> **Apache Cordova 8.1.2 or earlier required**
> Apache Cordova released an incompatible change to the tool in v9.0.0.  If you have Apache Cordova v9.0.0 or later installed, the plugin will not work, complaining of a dependency problem with the `q` module.

> **Visual Studio Code**
> There is an [Apache Cordova extension](https://marketplace.visualstudio.com/items?itemName=Msjsdiag.cordova-tools) for Visual Studio Code that allows you to run the application with debugging.  Visual Studio Code is highly recommended for Apache Cordova development.

This tutorial can be completed on either Windows or Mac systems.  The iOS version of the app can only be run on a Mac.  This tutorial uses Windows (with the app running on Android) only.

> **Gradle**
> The most common error when configuring Apache Cordova on Windows is the Gradle requirement.  This is installed by default using Android Studio but is not available for normal usage.  Download and unpack the [latest release](https://gradle.org/releases/), then add the `bin` directory to your PATH manually.

## Download the Apache Cordova quickstart project

The Apache Cordova quickstart project is located in the `samples/cordova` folder of the [azure/azure-mobile-apps](https://github.com/azure/azure-mobile-apps) GitHub repository.  You can [download the repository as a ZIP file](https://github.com/Azure/azure-mobile-apps/archive/master.zip), then unpack it.  The files will be created in the `azure-mobile-apps-master` folder.

Once downloaded, open a Terminal and change directory to the location of the files.  

{!quickstarts/includes/quickstart-deploy-backend.md!}

## Configure the Apache Cordova quickstart project

Run `npm install` to install all dependencies.

Add a platform to the project.  For example, to add the Android platform, use:

``` bash
cordova platform add android
```

You can add `browser`, `android`, and `ios` as needed.  However, note that the `browser` platform will not work with offline sync enabled. Once you have added all the platforms you wish to use, run `cordova requirements` to ensure all requirements have been met.

Open the `www/js/index.js` file in a text editor.  Edit the definition of `BackendUrl` to show your backend URL.  For example, if your backend URL was `https://zumo-abcd1234.azurewebsites.net`, then the Backend URL would look like this:

``` javascript linenums="4"
    var BackendUrl = "https://zumo-abcd1234.azurewebsites.net";
```

Save the file.  Open the `www/index.html` file in a text editor.  Edit the `Content-Security-Policy` to update the URL to match your backend URL; for example:

``` html linenums="5"
<meta http-equiv="Content-Security-Policy" 
    content="default-src 'self' data: gap: https://zumo-abcd1234.azurewebsites.net; style-src 'self'; media-src *;">
```

To build the app, use the following:

``` bash
cordova build
```

## Run the app

> **Enable CORS Support**
> If running from a browser (using `cordova platform add browser`), then you must enable CORS support within Azure App Service.  To do this, run the following command:
> 
> ```bash
> az webapp cors add -g zumo-quickstart --name ZUMOAPPNAME --allowed-origins "*"
> ```
>
> Replace the `ZUMPAPPNAME` with the name of your Azure App Service mobile backend.

Run the following command:

``` bash
cordova run android
```

> **What to do if the emulator does not start?**
>
> On Windows, this is a common problem.  Start Android Studio, then select **Configure** > **AVD Manager**.  This will allow you to start the device manually.  If you run `adb devices -l`, you should see your selected emulated device.  This allows you to run `cordova run android` successfully.

Once the initial startup is complete, you can add and delete items from the list.  They will be stored within the Azure SQL instance connected to your Azure Mobile Apps backend.

![Apache Cordova App](./media/cordova-android-startup.png)


