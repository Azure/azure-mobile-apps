# Add Authentication

In this tutorial, you add Microsoft authentication to the quickstart project using Azure Active Directory. Before completing this tutorial, ensure you have [created the project](./index.md) and [enabled offline sync](./offline.md).

{!quickstarts/includes/quickstart-configure-auth.md!}

## Test that authentication is being requested

* Open your project in Visual Studio. 
* From the **Run** menu, click **Run app**.
* Verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

This exception happens because the app attempts to access the back end as an unauthenticated user, but the *TodoItem* table now requires authentication.

## Add authentication to the app

To add authentication via the built-in provider, you must do the following:

* Register the protocol in the package manifest.
* Complete the login process when the callback is called.
* Trigger the login process before data is requested.

### Register the protocol in the package manifest

Protocols are registered in the app manifest:

* Open the `Package.appxmanifest` file.
* Navigate to the **Declarations** tab.
* Select **Protocol** in the **Available Declarations** dropdown list, then select **Add**.
* Enter the following information:
    * **Display Name**: _ZumoQuickstart_
    * **Name**: `zumoquickstart`
* Save the app manifest with **Ctrl+S**.

The **Name** field must match the protocol for the callback.  In our case, this is `zumoquickstart://easyauth.callback`, so the name is `zumoquickstart`.

### Handle the callback

Edit `App.xaml.cs`.  Add the following method to the class:

``` csharp linenums="104"
protected override void OnActivated(IActivatedEventArgs args)
{
    if (args.Kind == ActivationKind.Protocol)
    {
        ProtocolActivatedEventArgs protocolArgs = args as ProtocolActivatedEventArgs;
        Frame content = Window.Current.Content as Frame;
        if (content.Content.GetType() == typeof(MainPage))
        {
            content.Navigate(typeof(MainPage), protocolArgs.Uri);
        }
    }
    Window.Current.Activate();
    base.OnActivated(args);
}
```

When the `zumoquickstart` protocol is detected, the app will launch the main page with a Uri.  This is handled by the `OnNavigatedTo()` method within `MainPage.xaml.cs`.

### Trigger the login process

Edit the `MainPage.xaml.cs` file.  Edit the `OnNavigatedTo()` method to trigger the authentication on first entry:

``` csharp linenums="47" hl_lines="15-16"
protected override async void OnNavigatedTo(NavigationEventArgs e)
{
    if (e.Parameter is Uri)
    {
        _service.ResumeWithUri(e.Parameter as Uri);
        await RefreshItemsAsync(true);
        _service.TodoListUpdated += OnServiceUpdated;
    }
    else
    {
        await _service.AuthenticateAsync();
    }
}
```

Add the authentication code to `DataModel/TodoService.cs`:

``` csharp linenums="65"
public void ResumeWithUri(Uri uri) => mClient.ResumeWithURL(uri);

public Task AuthenticateAsync() => mClient.LoginAsync("aad", "zumoquickstart");
```

## Test the app

From the **Run** menu, click **Local Machine** to start the app.  You will be prompted for a Microsoft account in a browser.  When you are successfully signed in, the app should run as before without errors.


> **Deleting the resources**
>
> Now you have completed the quickstart tutorial, you can delete the resources with `az group delete -n zumo-quickstart`. You can also delete the global app registration used for authentication through the portal.

## Next steps

Take a look at the HOW TO sections:

* Server ([Node.js](../../howto/server/nodejs.md) or [ASP.NET Framework](../../howto/server/dotnet-framework.md))
* [.NET Client](../../howto/client/dotnet.md)

You can also do a Quick Start for another platform using the same backend server:

* [Apache Cordova](../cordova/index.md)
* [Windows (WPF)](../wpf/index.md)
* [Xamarin.Android](../xamarin-android/index.md)
* [Xamarin.Forms](../xamarin-forms/index.md)
* [Xamarin.iOS](../xamarin-ios/index.md)
