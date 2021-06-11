# Azure App Service Mobile Apps backend for the QuickStart

This sample provides a template basic application for use with Azure App Service written in
ES2015 that is automatically transpiled with [BabelJS]. It can be used
to implement a backend for the Azure App Service Mobile Apps Quickstart applications - a simple
Todo list with authentication and offline sync support.

BabelJS is used as a [require-hook] in order to handle the [ES2015 module syntax] that is used
throughout this samples server-side code.

The major reason this sample exists is to provide a fully annotated source code for a backend.

# Features

* Single Table (TodoItem)
* Single API (static configuration for clients)
* Home Page
* Swagger support, including a UI at /swagger/ui

This project can be used with any of the client projects provided by the QuickStart blade under
your Web app in the [Azure Portal].

# More Information

For more information, see the [Azure documentation].

<!-- Links -->
[BabelJS]: http://babeljs.io/
[require-hook]: http://babeljs.io/docs/setup/#babel_register
[ES2015 module syntax]: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import
[Azure Portal]: https://portal.azure.com
[Azure documentation]: https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-node-backend-how-to-use-server-sdk/