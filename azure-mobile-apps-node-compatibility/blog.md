# Azure Mobile Apps Compatibility Package

We recently announced that Azure Mobile Services will be deprecated and all
services will be migrated to Azure App Service.  At this point, you will 
become responsible for the code running your app.  You may want to upgrade
your site to take advantage of the additional facilities that Azure App 
Service provides you.

The Azure Mobile Apps compatibility package allows you to convert older Azure
Mobile Services applications written for the Node platform so that they can 
utilize the latest Azure Mobile Apps Node SDK.

## How Does it Work?

The package takes the raw source code from a Node-based Azure Mobile Service 
and generates the equivalent set of table and custom API definitions that will 
work with the Azure Mobile Apps Node SDK.  You will have a new project at the 
end that you can deploy as a new site to Azure App Service.  Both platforms 
offer a similar set of functionality with different APIs. The compatibility 
package maps the Mobile Services API to the newer Mobile Apps API.

The generated app is ready to deploy to an Azure App Service and should work
for most applications. It's important to review the code afterwards as some
common scenarios (such as authentication) will require specific configuration
or code changes in addition to the conversion.

## Performing a Conversion

Because the conversion produces a new site, there is a natural process to the
conversion.  Follow the process given below.  If you run into problems, please
get live help - we listen in on 
[Stack Overflow](http://stackoverflow.com/questions/tagged/azure-mobile-services?sort=newest&pageSize=20), 
the [MSDN Forums](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=azuremobile) and have a 
[Gitter channel](https://gitter.im/Azure/azure-mobile-apps-node) for live assistance.

Note that this process cannot be attempted until AFTER you have migrated the
site from Azure Mobile Services to Azure App Service. This is performed by
clicking the "Migrate to App Service" button at the bottom of the screen
in the old Azure portal (https://manage.windowsazure.com/).

### Obtain your Azure Mobile Services Scripts

Open the following URL in your browser:

    https://_mobile_service_name_.scm.azure-mobile.net/DebugConsole

Navigate by clicking on the directory names to the following location:

    site/wwwroot/App_Data/config

Download the `scripts` directory in ZIP format by clicking on the download
icon next to the folder name.

### Update Client

The client application must be updated to use the latest version of the Azure
Mobile Apps SDK. In many cases, this will simply be a matter of updating the
Azure Mobile Apps libraries (keep in mind the names of the Nuget packages for
Mobile Apps have changed from WindowsAzure.MobileServices to 
Microsoft.Azure.Mobile.Client). However, in some cases, additional code changes 
may be required.

You also need to update the URL that is passed to the constructor of the
Mobile App client object to the URL of the mobile app you created above.

### Creating the Mobile App

First, install the compatibility package by executing the following with
elevated privileges:

    npm i -g azure-mobile-apps-compatibility

This installs a command line utility with the following usage:

    scaffold-mobile-app <inputPath> <outputPath>

For example,

    scaffold-mobile-app scripts out

reads the Azure Mobile Service definition from the `scripts` directory located
in the current working directory and creates a directory called `out` with a
scaffolded Mobile App.

Once the app has been created, check the target folder to make sure it
contains files for the tables and custom APIs you defined in your mobile service.

Your app is almost ready to deploy!

## Deploying and Testing

### Create Database Compatibility Views

The scaffolded app includes a SQL script called `createViews.sql`. This script
must be executed against the target database. The connection string for the
target database can be obtained from your Mobile Service or migrated Mobile App
from the Settings (or Configure) page under the Connection String section.
The connection string name is MS_TableConnectionString. 

This script creates read / write views in the dbo database schema that map older 
reserved column names to new column names.

This script can also be obtained from https://raw.githubusercontent.com/Azure/azure-mobile-apps-node-compatibility/master/static/createViews.sql.

### Create Target Mobile App

Create a new Mobile App using the Azure portal and perform the following tasks:

* Take note of the URL for your Mobile App. You will need it later.
* Configure a data connection that points to the Mobile Service database.
* Configure push settings to use the same configuration as the Mobile Service.

If you previously used one of the built in authentication providers, there are
additional steps that you must take. Read 
[this](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-net-upgrading-from-mobile-services/#authentication) 
article for more information.

### Deploying

The simplest way to get your app onto Azure is using FTP. The URL, username
and password can be obtained from the portal. Before copying the files, you must
run `npm install` in a console window from the output folder created above. Copy
the entire contents of the output folder to the site/wwwroot folder of the FTP host.

If you are familiar with git, we recommend you follow the steps
[here](https://azure.microsoft.com/en-us/documentation/articles/web-sites-publish-source-control/)
up to the end of the Deploy your project section.

After you have deployed your app, open your browser and navigate to the URL
of your Mobile App. You should see the home page for your app. You can now
run and test your updated client. Additionally, the Easy Tables and Easy API
sections for your app in the portal should now be functional.

## Troubleshooting

To determine next steps, enable streaming logs for your app by opening the
portal to the settings of your app, opening the Diagnostics Logs section and
turning on Application Logging (Filesystem). Then open Log Stream from the Tools
section of your app.

Sometimes, it is necessary to view the startup logs. To do this, click the
Restart button from the toolbar, open the Log Stream and then open the app URL
in a browser. Any errors that occur during startup should appear.

### Cannot find module 'xxx'

Dependencies on external modules such as `async` have not been included by
default to reduce the size of the application. If you are using any external
modules, you will need to install them by opening a browser to
`https://<mobile_service_name>.scm.azure-mobile.net/DebugConsole`, navigating
to the site/wwwroot folder and executing:

    npm i <module_name>@<version>

The `@<version>` parameter is optional. However, it is highly recommended to
install the same package versions that were used in your Mobile Service to
ensure compatibility.

### The table 'xxx' does not exist

The getTable function is now case sensitive. Check to ensure the appropriate
case is being used.

### Invalid column name '__createdAt'

The double underscore notation for createdAt, updatedAt, version and deleted
columns have been removed. You will need to update any explicit column
references manually.

### process.env

If you are accessing any application settings that have been set in the portal
using process.env, they will not be configured for local debugging. These can be
set directly from the `azureMobile.js` file.

For applications deployed to Azure, ensure application settings have been copied
from the Mobile Service to the Mobile App.

### Can't set headers after they are sent

Calling request.respond or response.send more than once per request will result
in this error. Older versions of the web framework used by Mobile Services,
express, allowed this behavior, but the current version does not.

Use the generated stack trace to identify the offending module and change
the code to ensure these functions are only called once.

### Error in sideband demultiplexer

This usually indicates a corrupt git repository. You can fix this by running:

    git remote set-head origin master

This assumes your remote repository uses the default name `origin` and the
branch you are pushing to is called `master`.

## Caveats

There are a couple of areas that require additional changes. For example,
if you are using Mobile Services authentication, you need to update redirect URLs
on your identity provider as they have changed. Read 
[this](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-net-upgrading-from-mobile-services/#authentication) 
article for more information. Custom authentication (i.e. not using an identity 
provider such as facebook) should not be affected and should continue to work.

### Other issues

Our [github repository](https://github.com/Azure/azure-mobile-apps-node-compatibility)
will be updated with new troubleshooting steps as common cases are uncovered.

It's important to note that this package is experimental. We need your help
to make the experience as seamless as possible. Join the conversation on
[gitter](https://gitter.im/Azure/azure-mobile-apps-node) and let us know
about your experiences.

