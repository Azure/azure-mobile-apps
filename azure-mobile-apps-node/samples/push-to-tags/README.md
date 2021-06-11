# Azure App Service Mobile Apps backend with Per Tag Push Notifications

This sample provides a template basic application for use with Azure App Service.
It demonstrates how to use Azure Notification Hubs to register devices for specific
sets of tags and send push notifications to those tags when you insert a record.

# Features

* Custom API for registering devices with Notification Hubs with a specific set of tags
* Send push notifications to specific tags after a record is inserted
* Windows Store client

# Usage

The sample implements a Windows Store client and WNS notifications but can easily
be adapted for other platforms, or cross platform template notifications.

To run the sample, follow these steps:

## Obtain and Install the Sample

Obtain the sample from github and run `npm i` from the server directory.
Open the push_to_tags Visual Studio solution from the client directory.

## Create a Notification Hub and Windows Store Registration

From the [push notifications tutorial](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-windows-store-dotnet-get-started-push/#create-hub),
complete the `Create a Notification Hub`, `Register your app for push notifications`
and `Configure the backend to send push notifications` sections.

There is no need to make other modifications to the client or server listed in this
tutorial.

## Create a Configuration File

Create a file called `azureMobile.js` in the server directory that contains the
following code:

```Javascript
module.exports = {
    notifications: {
        hubName: 'hub_name',
        connectionString: 'connection_string'
    }
};
```

Replace `hub_name` with the name of the Notification Hub you created before and
`connection_string` with the full shared access connection string. It can be
obtained from the Access Policies section of the Notification Hub portal.

If you are deploying the sample to Azure, this file is not required and is excluded
using the .gitignore file. The Notification Hub is configured using the Push
section of the Mobile App Portal.

## Start the Server and Client

Execute `node app.js` from the server directory and run the sample client from
Visual Studio.

# More Information

For more information, see the [Azure documentation](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-node-backend-how-to-use-server-sdk/).
