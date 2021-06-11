Apache Cordova plugin for Azure Mobile Apps
=============================

With Microsoft Azure Mobile Apps you can add a scalable backend to your connected client applications in minutes.

To learn more, visit our [Developer Center](http://azure.microsoft.com/en-us/services/app-service/mobile/).

The source code for the Azure Mobile Apps Cordova plugin is available at https://github.com/Azure/azure-mobile-apps-js-client. The code is packed in a standalone Javascript bundle and is added to this repository for packaging as a Cordova plugin.

# Visual Studio App Center as modern and integrated solution for mobile development

Visual Studio App Center supports end to end and integrated services central to mobile app development. Developers can use the **Build**, **Test** and **Distribute** services to set up Continuous Integration and Delivery pipelines. Once the app is deployed, developers can monitor the status and usage of their app using the **Analytics** and **Diagnostics** services, and engage with users using the **Push** service. Developers can also leverage **Auth** to authenticate their users and **Data** to persist and sync app data in the cloud.

If you are looking to integrate cloud services in your mobile application, sign up with [App Center](https://appcenter.ms/signup?utm_source=zumo&utm_medium=Azure&utm_campaign=GitHub) today.

## Getting Started

If you are new to Mobile Apps, you can get started by following [Mobile Apps documentation](http://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-value-prop/).

### Offline data sync

Offline data sync is a feature of Azure Mobile Apps that makes it easy for developers to create apps that are functional without a network connection. Offline data sync is now available in the Cordova SDK. For detailed documentation, known issues and future work refer this [README](https://github.com/azure/azure-mobile-apps-js-client/tree/2.0.1).

### Sample usage ###
The following code creates a new client object to access the *todolist* mobile apps backend and create a new proxy object for the *TodoItem* table.

    var mobileAppsClient = new WindowsAzure.MobileServiceClient(
            "https://todolist.azurewebsites.net"
        );

    var todoTable = mobileAppsClient.getTable('TodoItem');
    
### Quickstart ###
Refer [README.md](https://github.com/Azure/azure-mobile-apps-js-client/blob/cordova-2.0.1/README.md) for detailed quickstart instructions.

## Need Help?

Be sure to check out the Mobile Services [Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/azuremobile/) if you are having trouble. The Azure Mobile Apps product team actively monitors the forum and will be more than happy to assist you.

## Future of Azure Mobile Apps
 
Microsoft is committed to fully supporting Azure Mobile Apps, including **support for the latest OS release, bug fixes, documentation improvements, and community PR reviews**. Please note that the product team is **not currently investing in any new feature work** for Azure Mobile Apps. We highly appreciate community contributions to all areas of Azure Mobile Apps. 

## Contribute Code or Provide Feedback

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/Azure/azure-mobile-apps-js-client/issues) section of the project.

## Learn More
[Microsoft Azure Mobile Apps Developer Center](http://azure.microsoft.com/en-us/services/app-service/mobile/)
