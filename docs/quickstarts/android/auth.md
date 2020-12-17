# Add Authentication

In this tutorial, you add Microsoft authentication to the quickstart project on Android using Azure Active Directory. Before completing this tutorial, ensure you have [created the project](index.md) and [enabled offline sync](offline.md).

{!quickstarts/includes/quickstart-configure-auth.md!}

## Test that authentication is being requested

* Open your project in Android Studio. 
* From the **Run** menu, click **Run app**.
* Verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

This exception happens because the app attempts to access the back end as an unauthenticated user, but the *TodoItem* table now requires authentication.

## Add authentication to the app

1. Open your project in Android Studio.
2. Open the `MainActivity` class.  Add the following constant to the top of the class:

    ```kotlin linenums="16"
    class MainActivity : AppCompatActivity() {
        companion object {
            const val LOGIN_REQUEST_CODE = 1
        }
        // ... rest of class ...
    ```

3. Add the following method to the `MainActivity` class:

    ``` kotlin linenums="62"
    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        if (requestCode == LOGIN_REQUEST_CODE && resultCode == RESULT_OK) {
            TodoService.instance.onActivityResult(data) { error ->
                if (error != null) showError(error) else onRefreshItemsClicked()
            }
        }
        super.onActivityResult(requestCode, resultCode, data)
    }
    ```

    This code creates a method to handle the Azure Active Directory authentication process.  Once authentication is successful, the data is refreshed automatically.

4. In the `onResume()` method, replace the initialization of the TodoService with the following code:

    ``` kotlin linenums="53"
    override fun onResume() {
        super.onResume()

        // Automatically refresh the items when the view starts
        TodoService.instance.initialize(this) { error ->
            if (error != null)
                showError(error)
            else
                TodoService.instance.authenticate(LOGIN_REQUEST_CODE)
        }
    }
    ```

   This will start the authentication flow when the application starts.

5. To ensure redirection works as expected, add the following snippet to `AndroidManifest.xml`.  Ensure the snippet is added inside the `<application>` node:

    ```xml
    <activity android:name="com.microsoft.windowsazure.mobileservices.authentication.RedirectUrlActivity">
        <intent-filter>
            <action android:name="android.intent.action.VIEW" />
            <category android:name="android.intent.category.DEFAULT" />
            <category android:name="android.intent.category.BROWSABLE" />
            <data android:scheme="zumoquickstart" android:host="easyauth.callback"/>
        </intent-filter>
    </activity>
    ```

6. Add the `customtabs` library to the dependencies in your `build.gradle`:

    ```gradle
    dependencies {
        // ... other dependencies here ...
        implementation 'com.android.support:customtabs:28.0.0'
    }
    ```

    Note that the library does not work with the AndroidX version of custom tabs.

## Test the app

From the **Run** menu, click **Run app** to start the app.  You will be prompted for a Microsoft account.  When you are successfully signed in, the app should run as before without errors.

> **Deleting the resources**
>
> Now you have completed the quickstart tutorial, you can delete the resources with `az group delete -n zumo-quickstart`.

## Next steps

Take a look at the HOW TO sections:

* Server ([Node.js](../../howto/server/nodejs.md) or [ASP.NET Framework](../../howto/server/dotnet-framework.md))
* [Android Client](../../howto/client/android.md)

You can also do a Quick Start for another platform using the same backend server:

* [Apache Cordova](../cordova/index.md)
* [iOS](../ios/index.md)
* [UWP](../uwp/index.md)
* [Xamarin.Android](../xamarin-android/index.md)
* [Xamarin.iOS](../xamarin-ios/index.md)
* [Xamarin.Forms](../xamarin-forms/index.md)
