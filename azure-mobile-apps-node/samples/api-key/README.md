# Azure App Service Mobile Apps backend implementing an API key

This sample provides a basic application template for use with Azure App Service.  It demonstrates the ability to validate 
api keys, a method of custom authentication.

# Features

* Single Table (TodoItem)
* Single Api (Ok)

# More Information

One common form of custom authentication is to use an api key.  In this form of authentication, a request will be authorized if it provides the correct api key to the endpoint.

To enable api key authentication for your app, there are a few steps:
  1. Add the api key as an application setting to your web app.
    * Navigate to the [Azure Portal](https://portal.azure.com) -> web app -> settings -> application settings
    * Add the app setting 'zumo-api-key' with api key value of your choice. Keep this key secret!
  2. Add the [validateApiKey](https://github.com/Azure/azure-mobile-apps-node/blob/master/samples/api-key/validateApiKey.js#L15) middleware to your project.  This middleware uses the app setting from step 1 to validate incoming requests.
  3. Attach the validateApiKey middleware to your table and api endpoints which require api key authentication.
    * [All table operations](https://github.com/Azure/azure-mobile-apps-node/blob/master/samples/api-key/tables/TodoItem.js#L20)    
    * [Single table operation](https://github.com/Azure/azure-mobile-apps-node/blob/master/samples/api-key/tables/TodoItem.js#L23) (insert, etc)
    * [Api methods](https://github.com/Azure/azure-mobile-apps-node/blob/master/samples/api-key/api/Ok.js#L10)
  4. Set the access of your table / api to 'anonymous'.  This can be done in [code](https://github.com/Azure/azure-mobile-apps-node/blob/master/samples/api-key/tables/TodoItem.js#L17), or if you are using the Easy Tables / Easy Apis experience, the portal.  If the access property is set to 'authenticated', requests will require a valid api key AND an authenticated user (from facebook, google, etc).
  5. By default, authenticated users (from facebook, google, etc) are rejected from the endpoint if they do not provide a valid api key.  If you would like to give authenticated users access without a valid api key, set the 'allowUsersWithoutApiKey' property in your [mobile app config](https://github.com/Azure/azure-mobile-apps-node/blob/master/samples/api-key/app.js#L19) to true.
  6. Make a request to the endpoint with the header 'zumo-api-key' set to your api key!

For more information, see the [Azure documentation](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-node-backend-how-to-use-server-sdk/).
