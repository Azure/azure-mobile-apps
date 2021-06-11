
[![Build Status](https://travis-ci.org/Azure/azure-mobile-apps-node.svg?branch=master)](https://travis-ci.org/Azure/azure-mobile-apps-node)
[![Dependency Status](https://david-dm.org/Azure/azure-mobile-apps-node.svg)](https://david-dm.org/Azure/azure-mobile-apps-node)
[![devDependency Status](https://david-dm.org/Azure/azure-mobile-apps-node/dev-status.svg)](https://david-dm.org/Azure/azure-mobile-apps-node#info=devDependencies)
[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Azure/azure-mobile-apps-node?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

# Azure Mobile Apps - Node SDK

## Basic Usage

The Azure Mobile Apps Node.js SDK is an [express](http://expressjs.com/) middleware package which makes it easy to create a backend for your mobile application and get it running on Azure.

```js
var app = require('express')(); // Create an instance of an Express app
var mobileApp = require('azure-mobile-apps')(); // Create an instance of a Mobile App with default settings

mobileApp.tables.add('TodoItem'); // Create a table for 'TodoItem' with default settings

app.use(mobileApp);
app.listen(process.env.PORT || 3000);
```

## Installation

`npm install --save azure-mobile-apps`

## Documentation & Resources

 - [API Documentation](https://azure.github.io/azure-mobile-apps-node)
 - [Samples](https://github.com/Azure/azure-mobile-apps-node/tree/master/samples)
 - [Tutorials & How-Tos](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-value-prop-preview/)
 - [Azure .NET SDK](https://www.visualstudio.com/features/azure-tools-vs)
 - [Client & Server Quickstarts](https://github.com/Azure/azure-mobile-services-quickstarts)
 - [StackOverflow #azure-mobile-services](http://stackoverflow.com/questions/tagged/azure-mobile-services?sort=newest&pageSize=20)
 - [MSDN Forums](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=azuremobile)
 - [Chat on Gitter](https://gitter.im/Azure/azure-mobile-apps-node?utm_source=share-link&utm_medium=link&utm_campaign=share-link)

## Quickstart

1. Create a new directory, initialize git, and initialize npm

  ```
  mkdir quickstart
  cd quickstart
  git init
  npm init --yes
  ```

2. Install (with npm) the azure-mobile-apps and express packages

  `npm install express azure-mobile-apps --save`

3. Create a suitable .gitignore file.  You can generate a suitable .gitignore
file using the generator at [gitignore.io](https://www.gitignore.io)

4. Create a server.js file and add the following code to the file (or use the code from one of our samples):

  ```js
  var app = require('express')(); // Create an instance of an Express app
  var mobileApp = require('azure-mobile-apps')(); // Create an instance of a Mobile App with default settings

  mobileApp.tables.add('TodoItem'); // Create a table for 'TodoItem' with default settings

  app.use(mobileApp);
  app.listen(process.env.PORT || 3000);
  ```

5. Run your project locally with `node server.js`

6. Publish your project to an existing Azure Mobile App by adding it as a remote and pushing your changes.

  ```
  git remote add azure https://{user}@{sitename}.scm.azurewebsites.net:443/{sitename}.git
  git add package.json server.js
  git commit -m 'Quickstart created'
  git push azure master
  ```

To test steps 4-5, you can use any of the clients found in the [Client & Server Quickstarts](https://github.com/Azure/azure-mobile-services-quickstarts).

## Running Tests

To run the suite of unit and integration tests, execute the following commands in a console window.

    git clone https://github.com/Azure/azure-mobile-apps-node.git
    cd azure-mobile-apps-node
    npm i
    npm test

This runs tests using the default embedded SQLite data provider. To execute tests
against SQL Server, create a configuration file called `azureMobile.js` in the
`test` directory that contains relevant data configuration. See the
[API reference](http://azure.github.io/azure-mobile-apps-node/global.html#configuration)
for more information.

## GitHub Organization

Our GitHub repository has one branch with code in it - master.  Each version is tagged with
the version when we release a new version.  We have three suffixes for the release. An alpha
release indicates that the API may be unstable between releases and the library may not pass
the end to end tests yet.  You should not use an alpha release in production or testing. We
release alpha releases to provide an early look at the library.  Has all the functionality we
expect in the final release and should be API stable, so it can be used for development.  A
beta library release may not pass end to end tests yet.  A GA release passes all end to end
tests and is recommended for production code.

We use [GitHub Issues](https://github.com/Azure/azure-mobile-apps-node/issues) to track all work
with this library.  We use Milestones to track the work going into a particular release.

## Future of Azure Mobile Apps
 
Microsoft is committed to fully supporting Azure Mobile Apps, including **support for the latest OS release, bug fixes, documentation improvements, and community PR reviews**. Please note that the product team is **not currently investing in any new feature work** for Azure Mobile Apps. We highly appreciate community contributions to all areas of Azure Mobile Apps. 

## Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

For information on how to contribute to this project, please see the [contributor guide](https://github.com/Azure/azure-mobile-apps-node/blob/master/contributor.md).

## Contact Us

We can be contacted via a variety of methods.  The most effective are on Twitter (via @AzureMobile) and the [MSDN Forums](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=azuremobile)  If you need to reference a GitHub Issue, ensure you cut-and-paste the URL of the issue into the message.  You can also reach us on [Gitter](https://gitter.im/Azure/azure-mobile-apps-node?utm_source=share-link&utm_medium=link&utm_campaign=share-link).

## License

[MIT](./LICENSE)
