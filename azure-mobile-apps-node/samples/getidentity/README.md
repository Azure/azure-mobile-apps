# Azure App Service Mobile Apps backend for the QuickStart

This sample provides a template basic application for use with Azure App Service.  It can be used
to implement a backend for the Azure App Service Mobile Apps Quickstart applications - a simple
Todo list with authentication and offline sync support.

This version of the ToDoItem application stores the data annotated with an email address pulled
from the authentication mechanism.  In order to properly use this, set up Authentication &
Authorization on the Azure App Service and ensure the email address is required as a claim.

This has been tested with Microsoft Account authentication only.

# Features

* Single Table (TodoItem) linked to the email address of the user
* Home Page (available in rc4)
* Swagger support, including a UI at /swagger/ui (available in Beta4)

This project can be used with any of the client projects provided by the QuickStart blade under
your Web app in the [Azure Portal](https://portal.azure.com).

# More Information

For more information, see the [Azure documentation](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-node-backend-how-to-use-server-sdk/).