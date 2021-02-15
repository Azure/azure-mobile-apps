# Add Authentication

In this tutorial, you add Microsoft authentication to the quickstart project on Xamarin.iOS using Azure Active Directory. Before completing this tutorial, ensure you have [created the project](./index.md) and [enabled offline sync](./offline.md).

{!quickstarts/includes/quickstart-configure-auth.md!}

## Test that authentication is being requested

* Open your project in Visual Studio. 
* Press F5 to run the app.
* Verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

This exception happens because the app attempts to access the back end as an unauthenticated user, but the *TodoItem* table now requires authentication.

## Add authentication to the app

Open the `TodoService.cs` class.  Edit the `InitializeAsync()` method to request authentication prior to marking initialization as complete:

``` csharp linenums="67" hl_lines="4-6"
    // Get a reference to the table.
    mTable = mClient.GetSyncTable<TodoItem>();

    // Authenticate
    var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;
    await mClient.LoginAsync(rootController, "aad", "zumoquickstart");

    isInitialized = true;
```

Use _Alt+Enter_ to add the required package (UIKit).

Open the `AppDelegate.cs` class.  Add the following code to the end of the class:

``` csharp linenums="38"
    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        => Xamarin.Essentials.Platform.OpenUrl(app, url, options);
```

Right-click on the `Info.plist` file, then select **Open with...**.  Select the **XML (Text) Editor**.  Add the following to the file right before the final `</dict>` line.

``` xml
    <key>CFBundleURLTypes</key>
    <array>
      <dict>
        <key>CFBundleURLName</key>
        <string>URL Type 1</string>
        <key>CFBundleURLSchemes</key>
        <array>
          <string>zumoquickstart</string>
        </array>
        <key>CFBundleTypeRole</key>
        <string>None</string>
      </dict>
    </array>
```

This redirects the response from the authentication web view back into the application.  You can now build and run the application.  When it runs, the login process will be triggered prior to the list of items being displayed.

## Test the app

Press F5 to run the app.  You will be prompted for a Microsoft account.  When you are successfully signed in, the app should run as before without errors.

> **Deleting the resources**
>
> Now you have completed the quickstart tutorial, you can delete the resources with `az group delete -n zumo-quickstart`.

## Next steps

Take a look at the HOW TO sections:

* Server ([Node.js](../../howto/server/nodejs.md) or [ASP.NET Framework](../../howto/server/dotnet-framework.md))
* [.NET Client](../../howto/client/dotnet.md)

You can also do a Quick Start for another platform using the same backend server:

* [Apache Cordova](../cordova/index.md)
* [Windows (UWP)](../uwp/index.md)
* [Windows (WPF)](../wpf/index.md)
* [Xamarin.Android](../xamarin-android/index.md)
* [Xamarin.Forms](../xamarin-forms/index.md)
