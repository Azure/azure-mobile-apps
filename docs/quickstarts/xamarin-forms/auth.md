# Add Authentication

In this tutorial, you add Microsoft authentication to the quickstart project on Xamarin.Forms using Azure Active Directory. Before completing this tutorial, ensure you have [created the project](./index.md) and [enabled offline sync](./offline.md).

{!quickstarts/includes/quickstart-configure-auth.md!}

## Test that authentication is being requested

* Open your project in Android Studio. 
* From the **Run** menu, click **Run app**.
* Verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

This exception happens because the app attempts to access the back end as an unauthenticated user, but the *TodoItem* table now requires authentication.

## Add authentication to the app

Authentication is handled differently on each platform.   First, add a required method to the `Utils\IAppContext.cs` interface:

``` csharp
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace ZumoQuickstart
{
    public interface IAppContext
    {
        // Place any methods required on the main entry-point here.
        Task<bool> AuthenticateAsync(MobileServiceClient client);
    }
}
```

Open the `TodoService.cs` class.  Edit the `InitializeAsync()` method to request authentication prior to marking initialization as complete:

``` csharp linenums="67"
    // Get a reference to the table.
    mTable = mClient.GetSyncTable<TodoItem>();

    // Add the following line:
    await mContext.AuthenticateAsync(mClient).ConfigureAwait(false);

    isInitialized = true;
```

### Add authentication to the Android app

Open the `MainActivity.cs` class in the `ZumoQuickStart.Android` project.  Add the following method to the class:

``` csharp linenums="35"
public async Task<bool> AuthenticateAsync(MobileServiceClient client)
{
    try
    {
        var user = await client.LoginAsync(this, "aad", "zumoquickstart").ConfigureAwait(false);
        return user != null;
    }
    catch (Exception error)
    {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.SetMessage(error.Message);
        builder.SetTitle("Sign in result");
        builder.Create().Show();
        return false;
    }
}
```

Edit the `Properties\AndroidManifest.xml` to register the authentication response handler:

``` xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android" android:versionCode="1" android:versionName="1.0" package="com.companyname.zumoquickstart">
    <uses-sdk android:minSdkVersion="21" android:targetSdkVersion="28" />
    <application android:label="ZumoQuickstart.Android" android:theme="@style/MainTheme">
      <activity
          android:name="com.microsoft.windowsazure.mobileservices.authentication.RedirectUrlActivity"
          android:launchMode="singleTop" android:noHistory="true">
        <intent-filter>
          <action android:name="android.intent.action.VIEW" />
          <category android:name="android.intent.category.DEFAULT" />
          <category android:name="android.intent.category.BROWSABLE" />
          <data android:scheme="zumoquickstart" android:host="easyauth.callback" />
        </intent-filter>
      </activity>      
    </application>
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
</manifest>
```

You can now run the Android app in the emulator.  It will prompt you for a Microsoft credential prior to showing you the list of items.

### Add authentication to the iOS app

Open the `AppDelegate.cs` class in the `ZumoQuickstart.iOS` project.  Add the following code to the end of the class:

> **TODO** This code doesn't work yet.

``` csharp linenums="38"
    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        => Xamarin.Essentials.Platform.OpenUrl(app, url, options);

    public async Task<bool> AuthenticateAsync(MobileServiceClient client)
    {
        var view = UIApplication.SharedApplication.KeyWindow.RootViewController;
        try
        {
            var user = await client.LoginAsync(view, "aad", "zumoquickstart").ConfigureAwait(false);
            return user != null;
        }
        catch (Exception error)
        {
            var alert = UIAlertController.Create("Sign-in result", error.Message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            view.PresentViewController(alert, true, null);
            return false;
        }
    }
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

### Add authentication to the UWP app

## Test the app

From the **Run** menu, click **Run app** to start the app.  You will be prompted for a Microsoft account.  When you are successfully signed in, the app should run as before without errors.

> **Deleting the resources**
>
> Now you have completed the quickstart tutorial, you can delete the resources with `az group delete -n zumo-quickstart`.

## Next steps

Take a look at the HOW TO sections:

* Server ([Node.js](../../howto/server/nodejs.md) or [ASP.NET Framework](../../howto/server/dotnet-framework.md)
* [.NET Client](../../howto/client/dotnet.md)

You can also do a Quick Start for another platform using the same backend server:

* [Android](../android/index.md)
* [Apache Cordova](../cordova/index.md)
* [iOS](../ios/index.md)
* [UWP](../uwp/index.md)
* [Xamarin.Android](../xamarin-android/index.md)
* [Xamarin.iOS](../xamarin-ios/index.md)
