# Add Authentication

In this tutorial, you add Microsoft authentication to the quickstart project using Azure Active Directory. Before completing this tutorial, ensure you have [created the project](./index.md) and [enabled offline sync](./offline.md).

{!quickstarts/includes/quickstart-configure-auth-native.md!}

## Test that authentication is being requested

* Open your project in Visual Studio. 
* From the **Run** menu, click **Run app**.
* Verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

This exception happens because the app attempts to access the back end as an unauthenticated user, but the *TodoItem* table now requires authentication.

## Add authentication to the app

There is no built-in authentication provider for WPF applications, so you must integrate the "provider" SDK for your authentication technique.  The provider SDK will authenticate the user through their normal mechanism, providing your app with an access token.  Your app then submits the access token or authorization code to the Azure App Service backend to get an appropriate access token for accessing the data in the backend.

In the case of Azure Active Directory and Microsoft accounts, the provider SDK is the [Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview).  You need to request an `access_token` from Azure Active Directory:

Next, add MSAL to your WPF project:

1. Open the project in Visual Studio.

2. Add the `Microsoft.Identity.Client` NuGet package to your app:
    * Right-click on the `ZumoQuickstart` project.
    * Select **Manage NuGet Packages...**.
    * Select the **Browse** tab.
    * Enter `Microsoft.Identity.Client` in the search box, then press Enter.
    * Select the `Microsoft.Identity.Client` result, then click **Install**.
    * Accept the license agreement to continue the installation.

3. Add the following variables to the `Constants.cs` file:

    ``` csharp linenums="3" hl_lines="11 16-19"
    public static class Constants
    {
        /// <summary>
        /// The base URL of the backend service within Azure.
        /// </summary>
        public static string BackendUrl { get; } = "https://ZUMOAPPNAME.azurewebsites.net";

        /// <summary>
        /// The Application (Client) Id for the AAD App Registration
        /// </summary>
        public static string ApplicationId { get; } = "CLIENTID";

        /// <summary>
        /// The list of scopes to ask for when authenticatign with MSAL
        /// </summary>
        public static string[] Scopes { get; } = new string[]
        {
            "SCOPE"
        };
    }
    ```

    You obtained the _Application (Client) ID_ (referenced here as `CLIENTID`) and the _Scope_ (referenced here as `SCOPE`) when you registered the application in the AAD App Registrations page.

4. Add the following code to the `App.xaml.cs` file:

    ``` csharp linenums="10" hl_lines="3-11"
    public partial class App : Application
    {
        public static IPublicClientApplication PublicClientApp { get; private set; }

        static App()
        {
            PublicClientApp = PublicClientApplicationBuilder.Create(Constants.ApplicationId)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .WithRedirectUri("http://localhost")
                .Build();
        }

        internal static void RunOnUiThread(Action p)
            => App.Current.Dispatcher.Invoke(p);
    }
    ```

5. Edit the `TodoService.cs` file, and add the token acquisition code to the `InitializeAsync()` method:

    ``` csharp linenums="49" hl_lines="15-18 20-26"
    private async Task InitializeAsync()
    {
        using (await initializationLock.LockAsync())
        {
            if (!isInitialized)
            {
                // Create the client
                mClient = new MobileServiceClient(Constants.BackendUrl, new LoggingHandler());

                // Define the offline store
                mStore = new MobileServiceSQLiteStore("todoitems.db");
                mStore.DefineTable<TodoItem>();
                await mClient.SyncContext.InitializeAsync(mStore).ConfigureAwait(false);

                // Obtain an MSAL authorization_code
                PublicClientApplication msalApp = App.PublicClientApp as PublicClientApplication;
                var authResult = await msalApp.AcquireTokenInteractive(Constants.Scopes)
                    .ExecuteAsync();

                // Call LoginAsync to authenticate user to Azure Mobile Apps Server
                // using the access_token from MSAL authentication result.  For details
                // on what you need to send for each provider, see:
                // https://docs.microsoft.com/en-us/azure/app-service/app-service-authentication-how-to#validate-tokens-from-providers
                await mClient.LoginAsync("aad", new JObject(
                    new JProperty("access_token", authResult.AccessToken)
                ));

                // Get a reference to the table
                mTable = mClient.GetSyncTable<TodoItem>();

                isInitialized = true;
            }
        }
    }
    ```

Lines 64-66 will use the MSAL library to authenticate the user.  This pops up a web browser to complete the authentication process.  Once complete, lines 72-74 submit the access token received from AAD to the App Service, receiving back an Azure Mobile Apps access token.  This token is then submitted on each request to the service to identify the user.

You can now run the app.  It will prompt you for a Microsoft credential prior to showing you the list of items.

## Test the app

From the **Run** menu, click **Run app** to start the app.  You will be prompted for a Microsoft account.  When you are successfully signed in, the app should run as before without errors.

> **Deleting the resources**
>
> Now you have completed the quickstart tutorial, you can delete the resources with `az group delete -n zumo-quickstart`. You can also delete the global app registration used for authentication through the portal.

## Next steps

Take a look at the HOW TO sections:

* Server ([Node.js](../../howto/server/nodejs.md) or [ASP.NET Framework](../../howto/server/dotnet-framework.md))
* [.NET Client](../../howto/client/dotnet.md)

You can also do a Quick Start for another platform using the same backend server:

* [Apache Cordova](../cordova/index.md)
* [Windows (UWP)](../uwp/index.md)
* [Xamarin.Android](../xamarin-android/index.md)
* [Xamarin.Forms](../xamarin-forms/index.md)
* [Xamarin.iOS](../xamarin-ios/index.md)
