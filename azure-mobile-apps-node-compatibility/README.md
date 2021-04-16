# azure-mobile-apps-compatibility

This module allows you to generate a set of scaffolded table and custom API
definitions from the set of definition files from an Azure Mobile Service.

The generated app is ready to deploy to an Azure Mobile App and should work
for simple applications. More complex applications, particularly those using
authentication, will likely require some code changes.

##### This module is experimental.

## Preparation

### Obtain Mobile Service Definitions

Open the following URL in your browser:

    https://<mobile_service_name>.scm.azure-mobile.net/DebugConsole

Navigate by clicking on the directory names to the following location:

    site/wwwroot/App_Data/config

Download the `scripts` directory in ZIP format by clicking on the download
icon next to the folder name.

### Create Database Compatibility Views

The scaffolded app includes a SQL script called `createViews.sql`. This script
must be executed against the target database. This script can also be obtained
from https://raw.githubusercontent.com/Azure/azure-mobile-apps-node-compatibility/master/static/createViews.sql.

### Create Target Mobile App

Create a new Mobile App using the Azure portal and perform the following tasks:

* configure a data connection that points to the Mobile Service database
* configure push settings to use the same configuration as the Mobile Service
* copy any custom application settings from the Mobile Service to the Mobile App

If you previously used one of the built in authentication providers, there are
additional steps that you must take. See http://url/ for more information.

### Update Client

The client application must be updated to use the latest version of the Azure
Mobile Apps SDK.

## Usage

To install, execute the following with elevated privileges:

    npm i -g azure-mobile-apps-compatibility

This installs a command line utility with the following usage:

    scaffold-mobile-app <inputPath> <outputPath>

For example,

    scaffold-mobile-app scripts out

reads the Azure Mobile Service definition from the `scripts` directory located
in the current working directory and creates a directory called `out` with a
scaffolded Mobile App.

## Running Locally

To run the app locally, change to the output directory and install required
node modules by executing:

    npm i

You must also edit the `azureMobile.js` file and provide appropriate data
connection and notification hub information.

The server can then be started by running:

    node --debug app.js

from the output directory. This starts the server on port 3000.

## Troubleshooting

### Cannot find module 'xxx'

Dependencies on external modules such as `async` have not been included by
default to reduce the size of the application. If you are using any external
modules, you will need to install them by executing:

    npm i <module_name>@<version> --save

The `@<version>` parameter is optional. However, it is highly recommended to
install the same package versions that were used in your Mobile Service to
ensure compatibility.

The `--save` option adds the dependency to the `package.json` file so it is
also installed when deployed to Azure.

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
