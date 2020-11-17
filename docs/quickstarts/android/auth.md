# Add Authentication

In this tutorial, you add Microsoft authentication to the quickstart project on Android by using a supported identity provider. Before completing this tutorial, ensure you have [created the project](index.md) and [enabled offline sync](offline.md).

{!quickstarts/includes/quickstart-configure-auth.md!}

## Test that authentication is being requested

* Open your project in Android Studio. 
* From the **Run** menu, click **Run app**.
* Verify that an unhandled exception with a status code of 401 (Unauthorized) is raised after the app starts.

This exception happens because the app attempts to access the back end as an unauthenticated user, but the *TodoItem* table now requires authentication.

## Add authentication to the app

1. Open your project in Android Studio.
2. Open the `TodoActivity.java` file.  Add the following import statements:

    ```java
    import java.util.concurrent.ExecutionException;
    import java.util.concurrent.atomic.AtomicBoolean;

    import android.content.Context;
    import android.content.SharedPreferences;
    import android.content.SharedPreferences.Editor;

    import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
    import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
    ```

3. Add the following methods to the `TodoActivity` class:

    ```java
    // You can choose any unique number here to differentiate auth providers from each other. 
    // Note this is the same code at login() and onActivityResult().
    public static final int LOGIN_REQUEST_CODE = 1;

    private void authenticate() {
        // Sign in using the Azure Active Directory provider.
        mClient.login("aad", "zumoquickstart", LOGIN_REQUEST_CODE);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        // When request completes
        if (resultCode == RESULT_OK) {
            // Check the request code matches the one we send in the login request
            if (requestCode == LOGIN_REQUEST_CODE) {
                MobileServiceActivityResult result = mClient.onActivityResult(data);
                if (result.isLoggedIn()) {
                    // sign-in succeeded
                    createAndShowDialog(String.format("You are now signed in - %1$2s", 
                        mClient.getCurrentUser().getUserId()), "Success");
                    createTable();
                } else {
                    createAndShowDialog(result.getErrorMessage(), "Error");
                }
            }
        }
    }
    ```

    This code creates a mehtod to handle the Azure Active Directory authentication process.  A dialog displays the ID of the authenticated user.  You can only proceed on a successful authentication.

4. In the `onCreate` method, uncomment the `authenticate()` call and comment out the `createTable()` call.  The `createTable()` call is now done only when authentication is successful:

    ```java linenums="101"
    // AUTHENTICATE USERS
    authenticate();
    //createTable();
    ```

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
