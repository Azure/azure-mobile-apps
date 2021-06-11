# Microsoft Azure Mobile Apps: Android Client SDK

With Microsoft Azure Mobile Apps you can add a scalable backend to your connected client applications in minutes. To learn more, visit our [Developer Center](http://azure.microsoft.com/en-us/develop/mobile).

## Getting Started

If you are new to Mobile Services, you can get started by following our tutorials for connecting your Mobile
Services cloud backend to [Android apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-android-get-started/).

## Supported Platforms

The current release of the Android SDK for Azure Mobile Apps supports:

* Android releases for API 19 through 26 (KitKat through Oreo)

Other platforms may work, but have not been tested.

## Version Information

There are two major versions of this SDK:

- SDK v2.x.x is used with Azure Mobile Services.
- SDK v3.x.x is used with Azure App Service / Azure Mobile Apps.

Please ensure you are using the right SDK version that matches your backend cloud service.

## Download Source Code

To get the source code of our SDKs and samples via **git** just type:

    git clone https://github.com/Azure/azure-mobile-apps-android-client.git
    cd ./azure-mobile-apps-android-client/

## Reference Documentation
- [Reference documentation (Java docs)](http://azure.github.io/azure-mobile-apps-android-client/)
- [Change Log](CHANGELOG.md)

## Android SDK
Microsoft Azure Mobile Services can be used with an Android-based device using our Android SDK. You can get the Android SDK in one of the following two ways or you can download the source code using the instructions above.

1. For an Android studio project, add the line `compile 'com.microsoft.azure:azure-mobile-android:3.5.1@aar'` to the app’s Gradle.build file with your desired SDK version plugged in (you can find the latest versions [here](https://go.microsoft.com/fwLink/?LinkID=525472&clcid=0x409)):
2. Eclipse users can [download the Android SDK](http://go.microsoft.com/fwlink/?LinkID=717033&clcid=0x409) directly or can download the source code using the instructions above.

### Prerequisites
The SDK requires Android Studio.

### Building and Referencing the SDK
1. Open the folder `\azure-mobile-apps-android-client\sdk` using the option `Open an existing Android Studio Project` in Android Studio.
2. Project should be built automatically, In case it does not build, Right click on `sdk` folder and select `Make Module 'sdk'`.
3. The file `sdk-release.aar` should be present at `\azure-mobile-apps-android-client\sdk\src\sdk\build\outputs\aar`.
4. Rename the file `sdk-release.aar` to `sdk-release.zip`.
5. Extract the zip file, `classes.jar` should be present in the root folder.

### Running the Tests

The SDK has a suite of unit tests that you can easily run.

1. Open the folder `\azure-mobile-apps-android-client\sdk` using the option `Open an existing Android Studio Project` in Android Studio.
2. Project should be built automatically, In case it does not build, Right click on `sdk` folder and select `Make Module 'sdk.testapp'`.
3. Expand `sdk.testapp` and sub folder `java`.
4. Right click on `com.microsoft.windowsazure.mobileservices.sdk.testapp`, Select `Run`, Select `Tests in com.microsoft.windowsazure.mobileservices.sdk.testapp` (with Android tests icon).

## Future of Azure Mobile Apps
 
Microsoft is committed to fully supporting Azure Mobile Apps, including **support for the latest OS release, bug fixes, documentation improvements, and community PR reviews**. Please note that the product team is **not currently investing in any new feature work** for Azure Mobile Apps. We highly appreciate community contributions to all areas of Azure Mobile Apps. 

## Useful Resources

* [Quickstarts](https://github.com/Azure/azure-mobile-services-quickstarts)
* [Samples](https://github.com/Azure/mobile-services-samples)
* Tutorials and product overview are available at [Microsoft Azure Mobile Services Developer Center](http://azure.microsoft.com/en-us/develop/mobile).
* Our product team actively monitors the [Mobile Services Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/azuremobile/) to assist you with any troubles.

## Contribute Code or Provide Feedback

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/Azure/azure-mobile-apps-android-client/issues) section of the project.
