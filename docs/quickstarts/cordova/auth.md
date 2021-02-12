# Add Authentication

In this tutorial, you add Microsoft authentication to the quickstart project using Azure Active Directory. Before completing this tutorial, ensure you have [created the project](./index.md) and [enabled offline sync](./offline.md).

{!quickstarts/includes/quickstart-configure-auth.md!}

## Test that authentication is being requested

* Run the app using `cordova run android`
* Verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

This exception happens because the app attempts to access the back end as an unauthenticated user, but the *TodoItem* table now requires authentication.

## Add authentication to the app

To add authentication via the built-in provider, you must do the following:

* Add the authentication provider to the list of known good sources.
* Call the authentication provider prior to accessing data.

### Update the Content Security Policy

Each Apache Cordova app declares their known good sources via a `Content-Security-Policy` header. Each supported provider has an OAuth host that needs to be added:

| Provider | SDK Provider Name | OAuth Host |
|:--- |:--- |:--- |
| Azure Active Directory | aad | https://login.microsoftonline.com |
| Facebook | facebook | https://www.facebook.com |
| Google | google | https://accounts.google.com |
| Twitter | twitter | https://api.twitter.com |

Edit `www/index.html`; add the OAuth host for Azure Active Directory as follows:

``` html linenums="5"
<meta http-equiv="Content-Security-Policy" content="
    default-src 'self' 
    data: gap: https://login.microsoftonline.com https://ZUMOAPPNAME.azurewebsites.net; 
    style-src 'self'; media-src *;">
```

Note that we have spread the content over multiple lines for readability.  You should place all content on the same line.

``` html linenums="5"
<meta http-equiv="Content-Security-Policy" content="default-src 'self' data: gap: https://login.microsoftonline.com https://ZUMOAPPNAME.azurewebsites.net; style-src 'self'; media-src *;">
```

You have already replaced `ZUMOAPPNAME` with the name of your app.  For more information on the Content-Security-Policy meta tag, see the [Content-Security-Policy documentation](https://cordova.apache.org/docs/en/latest/guide/appdev/whitelist/index.html).

## Call the authentication provider

Edit `www/js/index.js`. Replace the `setup()` method with the following:

``` javascript linenums="61"
function setup() {
    client.login('aad').then(function () {
        // ORIGINAL CONTENTS OF FUNCTION
        todoTable = client.getSyncTable('todoitem');
        refreshDisplay();
        addItemEl.addEventListener('submit', addItemHandler);
        refreshButtonEl.addEventListener('click', refreshDisplay);
        // END OF ORIGINAL CONTENTS OF FUNCTION
    });
}
```

## Test the app

Run the following command:

``` bash
cordova run android
```

> **What to do if the emulator does not start?**
>
> On Windows, this is a common problem.  Start Android Studio, then select **Configure** > **AVD Manager**.  This will allow you to start the device manually.  If you run `adb devices -l`, you should see your selected emulated device.  This allows you to run `cordova run android` successfully.

Once the initial startup is complete, you will be prompted to sign in with your Microsoft credentials.  Once complete, you can add and delete items from the list.  

> **Deleting the resources**
>
> Now you have completed the quickstart tutorial, you can delete the resources with `az group delete -n zumo-quickstart`. You can also delete the global app registration used for authentication through the portal.

## Next steps

Take a look at the HOW TO sections:

* Server ([Node.js](../../howto/server/nodejs.md) or [ASP.NET Framework](../../howto/server/dotnet-framework.md))
* [.NET Client](../../howto/client/dotnet.md)

You can also do a Quick Start for another platform using the same backend server:

* [Windows (UWP)](../uwp/index.md)
* [Windows (WPF)](../wpf/index.md)
* [Xamarin.Android](../xamarin-android/index.md)
* [Xamarin.Forms](../xamarin-forms/index.md)
* [Xamarin.iOS](../xamarin-ios/index.md)
