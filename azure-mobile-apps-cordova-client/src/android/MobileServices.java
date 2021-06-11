// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

package com.microsoft.windowsazure.mobileservices.cordova;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;

import com.microsoft.windowsazure.mobileservices.MobileServiceActivityResult;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;

import org.apache.cordova.CallbackContext;
import org.apache.cordova.CordovaArgs;
import org.apache.cordova.CordovaPlugin;
import org.apache.cordova.PluginResult;
import org.apache.cordova.PluginResult.Status;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.HashMap;

import static android.app.Activity.RESULT_OK;

public class MobileServices extends CordovaPlugin {

    public static final int GOOGLE_LOGIN_REQUEST_CODE = 1;
    private static final String MANIFEST_REDIRECT_URI_SCHEME_KEY = ".azure_mobileapps_redirect_uri_scheme";
    private static final String COULD_NOT_READ_REDIRECT_URI_SCHEME_MSG =
            "Could not read redirect uri scheme from manifest. " +
                    "Did you have this line '<meta-data android:name=\".azure_mobileapps_redirect_uri_scheme\" android:value=\"YOUR_URI_SCHEME\" />' " +
                    "appeared and configured in your AndroidManifest.xml?";
    private MobileServiceClient mobileServiceClient = null;
    private CallbackContext resultContext;

    @Override
    public boolean execute(String action, CordovaArgs args, CallbackContext callbackContext) throws JSONException {
        if ("loginWithGoogle".equals(action)) {
            return execLoginWithGoogle(args, callbackContext);
        }

        return false;
    }

    private boolean execLoginWithGoogle(CordovaArgs args, CallbackContext callbackContext) {
        try {
            String appUrl = args.getString(0);
            mobileServiceClient = new MobileServiceClient(appUrl, this.cordova.getActivity());

            cordova.setActivityResultCallback(this);
            resultContext = callbackContext;

            String uriScheme = getUriSchemeFromManifest();

            mobileServiceClient.login(MobileServiceAuthenticationProvider.Google, uriScheme, GOOGLE_LOGIN_REQUEST_CODE);
        } catch (Exception e) {
            callbackContext.error("Could not authenticate with google provider. " + e.getMessage());
        }

        return true;
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        // When request completes
        if (resultCode == RESULT_OK) {
            // Check the request code matches the one we send in the login request
            if (requestCode == GOOGLE_LOGIN_REQUEST_CODE) {
                MobileServiceActivityResult result = mobileServiceClient.onActivityResult(data);
                if (result.isLoggedIn()) {
                    // login succeeded
                    JSONObject token = getTokenObjectFromUser(mobileServiceClient.getCurrentUser());
                    resultContext.sendPluginResult(new PluginResult(Status.OK, token));
                } else {
                    // login failed, check the error message
                    String errorMessage = result.getErrorMessage();
                    resultContext.sendPluginResult(new PluginResult(Status.ERROR, errorMessage));
                }
            }
        } else {
            resultContext.sendPluginResult(
                    new PluginResult(Status.ERROR, "Cannot authenticate due to error occured. " + data.getDataString())
            );
        }
    }

    /**
     * Get redirect uri scheme value from AndroidManifest.xml
     *
     * @return String
     * @throws Exception
     */
    private String getUriSchemeFromManifest() throws Exception {
        Activity activity = this.cordova.getActivity();
        ApplicationInfo ai;
        try {
            ai = activity.getPackageManager().getApplicationInfo(activity.getPackageName(), PackageManager.GET_META_DATA);
        } catch (PackageManager.NameNotFoundException e) {
            throw new Exception(COULD_NOT_READ_REDIRECT_URI_SCHEME_MSG);
        }

        Bundle bundle = ai.metaData;
        if (bundle == null) {
            throw new Exception(COULD_NOT_READ_REDIRECT_URI_SCHEME_MSG);
        }
        String uriScheme = bundle.getString(MANIFEST_REDIRECT_URI_SCHEME_KEY);
        if (uriScheme == null || uriScheme.isEmpty()) {
            String message =
                    "'.azure_mobileapps_redirect_uri_scheme' meta-data value is empty in your AndroidManifest.xml. " +
                            "Please, fill it with non-empty value.";
            throw new Exception(message);
        }

        return uriScheme;
    }

    /**
     * Convert MobileServiceUser object to javascript 'Token' object to pass it in callback
     *
     * @param user MobileServiceUser object
     * @return JSONObject
     */
    private JSONObject getTokenObjectFromUser(MobileServiceUser user) {
        HashMap<String, Object> tokenMap = new HashMap<String, Object>();
        tokenMap.put("authenticationToken", user.getAuthenticationToken());
        HashMap<String, String> userMap = new HashMap<String, String>();
        userMap.put("sid", user.getUserId());
        tokenMap.put("user", userMap);

        return new JSONObject(tokenMap);
    }
}