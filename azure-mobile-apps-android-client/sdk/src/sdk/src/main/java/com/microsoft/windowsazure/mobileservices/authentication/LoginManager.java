/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */

/**
 * LoginManager.java
 */
package com.microsoft.windowsazure.mobileservices.authentication;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.graphics.Bitmap;
import android.util.DisplayMetrics;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.EditText;
import android.widget.LinearLayout;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.MoreExecutors;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonObject;
import com.google.gson.JsonParseException;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.http.MobileServiceConnection;
import com.microsoft.windowsazure.mobileservices.http.RequestAsyncTask;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequestImpl;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;

import java.net.URL;
import java.net.URLDecoder;
import java.util.HashMap;

/**
 * Class for handling Login operations with Authentication Providers and Microsoft
 * Azure Mobile Services
 */
@SuppressLint({"SetJavaScriptEnabled"})
public class LoginManager {
    /**
     * Login process initial URL
     */
    private static final String START_URL = ".auth/login";
    /**
     * Login process final URL
     */
    private static final String END_URL = ".auth/login/done";
    /**
     * Refresh user URL
     */
    private static final String REFRESH_USER_URL = ".auth/refresh";
    /**
     * Token indicator for interactive authentication URL
     */
    private static final String TOKEN_MARK = "#token=";
    /**
     * Authentication Token parameter in JSON objects
     */
    private static final String TOKEN_JSON_PARAMETER = "authenticationToken";
    /**
     * UserId property in JSON objects
     */
    private static final String USERID_JSON_PROPERTY = "userId";
    /**
     * User property in JSON objects
     */
    private static final String USER_JSON_PROPERTY = "user";
    /**
     * The MobileServiceClient used to invoke the login operations
     */
    private MobileServiceClient mClient;

    /**
     * Constructor for LoginManager
     *
     * @param client The MobileServiceClient used to invoke the login operations
     */
    public LoginManager(MobileServiceClient client) {
        if (client == null) {
            throw new IllegalArgumentException("Client can not be null");
        }

        mClient = client;
    }

    /**
     * Refreshes access token with the identity provider for the logged in user.
     * @param callback The callback to invoke when the authentication process finishes
     */
    public void refreshUser(final UserAuthenticationCallback callback) {

        ListenableFuture<MobileServiceUser> future = refreshUser();

        Futures.addCallback(future, new FutureCallback<MobileServiceUser>() {
            @Override
            public void onSuccess(MobileServiceUser user) {
                callback.onCompleted(user, null, null);
            }

            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }
        }, MoreExecutors.directExecutor());
    }

    /**
     * Refreshes access token with the identity provider for the logged in user.
     * @return Refreshed Mobile Service user
     */
    public ListenableFuture<MobileServiceUser> refreshUser() {

        final SettableFuture<MobileServiceUser> future = SettableFuture.create();

        if (mClient.getCurrentUser() == null || mClient.getCurrentUser().getUserId() == null) {
            future.setException(new MobileServiceException("MobileServiceUser must be set before calling refresh"));
            return future;
        }

        URL appUrl = UriHelper.createHostOnlyUrl(mClient.getAppUrl());

        String url = UriHelper.CombinePath(appUrl.toString(), REFRESH_USER_URL);

        if (this.mClient.getAlternateLoginHost() != null) {
            url = UriHelper.CombinePath(mClient.getAlternateLoginHost().toString(), REFRESH_USER_URL);
        }

        // Create a request
        final ServiceFilterRequest request =
                ServiceFilterRequestImpl.get(mClient.getOkHttpClientFactory(), url);

        final MobileServiceConnection connection = mClient.createConnection();

        // Create the AsyncTask that will execute the request
        new RequestAsyncTask(request, connection) {
            @Override
            protected void onPostExecute(ServiceFilterResponse response) {
                if (mTaskException == null && response != null) {
                    MobileServiceUser user;
                    try {
                        // Get the user from the response and create a
                        // MobileServiceUser object from the JSON
                        String content = response.getContent();
                        user = createUserFromJSON((JsonObject) new JsonParser().parse((content.trim())));
                        mClient.setCurrentUser(user);
                    } catch (Exception e) {
                        future.setException(new MobileServiceException("Error on refresh user.", e, response));
                        return;
                    }

                    future.set(user);
                } else {
                    if (mTaskException != null && mTaskException.getResponse() != null && mTaskException.getResponse().getStatus() != null) {
                        String message;
                        switch (mTaskException.getResponse().getStatus().code) {
                            case 400:
                                message = "Refresh failed with a 400 Bad Request error. The identity provider does not support refresh, or the user is not logged in with sufficient permission.";
                                break;
                            case 401:
                                message = "Refresh failed with a 401 Unauthorized error. Credentials are no longer valid.";
                                break;
                            case 403:
                                message = "Refresh failed with a 403 Forbidden error. The refresh token was revoked or expired.";
                                break;
                            default:
                                message = "Refresh failed due to an unexpected error.";
                                break;
                        }
                        future.setException(new MobileServiceException(message, mTaskException));
                    } else {
                        future.setException(new MobileServiceException("Error on refresh user."));
                    }
                }
            }
        }.executeTask();

        return future;
    }

    /**
     * Invokes an interactive authentication process using the specified
     * Authentication Provider
     *
     * @param provider The provider used for the authentication process
     * @param context  The context used to create the authentication dialog
     */
    public ListenableFuture<MobileServiceUser> authenticate(String provider, Context context) {

        return authenticate(provider, context, (HashMap<String, String>) null);
    }

    /**
     * Invokes an interactive authentication process using the specified
     * Authentication Provider
     *
     * @param provider   The provider used for the authentication process
     * @param context    The context used to create the authentication dialog
     * @param parameters Additional parameters for the authentication process
     */
    public ListenableFuture<MobileServiceUser> authenticate(String provider, Context context, HashMap<String, String> parameters) {
        final SettableFuture<MobileServiceUser> future = SettableFuture.create();

        if (provider == null || provider.length() == 0) {
            throw new IllegalArgumentException("provider cannot be null or empty");
        }

        String path = UriHelper.CombinePath(LoginManager.START_URL, UriHelper.normalizeProvider(provider));
        String loginAsyncDoneUriFragment = LoginManager.END_URL;
        if (mClient.getLoginUriPrefix() != null) {
            path = UriHelper.CombinePath(this.mClient.getLoginUriPrefix(), UriHelper.normalizeProvider(provider));
            loginAsyncDoneUriFragment = UriHelper.CombinePath(this.mClient.getLoginUriPrefix(), "done");
        }

        URL appUrl = UriHelper.createHostOnlyUrl(mClient.getAppUrl());

        String startUrl = UriHelper.CombinePath(appUrl.toString(), path);
        String endUrl = UriHelper.CombinePath(appUrl.toString(), loginAsyncDoneUriFragment);

        if (this.mClient.getAlternateLoginHost() != null) {
            startUrl = UriHelper.CombinePath(mClient.getAlternateLoginHost().toString(), path);
            endUrl = UriHelper.CombinePath(mClient.getAlternateLoginHost().toString(), loginAsyncDoneUriFragment);
        }
        parameters = this.addSessionMode(parameters);
        startUrl = startUrl + UriHelper.normalizeParameters(parameters);
        // Shows an interactive view with the provider's login
        showLoginUI(startUrl, endUrl, context, new LoginUIOperationCallback() {

            @Override
            public void onCompleted(String url, Exception exception) {
                if (exception == null) {
                    MobileServiceUser user;
                    try {
                        String decodedUrl = URLDecoder.decode(url, MobileServiceClient.UTF8_ENCODING);

                        JsonObject json = (JsonObject) new JsonParser().parse(decodedUrl.substring(decodedUrl.indexOf(TOKEN_MARK) + TOKEN_MARK.length()));

                        user = createUserFromJSON(json);
                    } catch (Exception e) {
                        future.setException(new MobileServiceException("Error while authenticating user.", e));
                        return;
                    }

                    future.set(user);
                } else {
                    future.setException(new MobileServiceException("Error while authenticating user.", exception));
                }
            }
        });

        return future;
    }

    /**
     * Invokes an interactive authentication process using the specified
     * Authentication Provider
     *
     * @param provider The provider used for the authentication process
     * @param context  The context used to create the authentication dialog
     * @param callback Callback to invoke when the authentication process finishes
     */
    public void authenticate(String provider, Context context, final UserAuthenticationCallback callback) {
        authenticate(provider, context, null, callback);
    }

    /**
     * Invokes an interactive authentication process using the specified
     * Authentication Provider
     *
     * @param provider   The provider used for the authentication process
     * @param context    The context used to create the authentication dialog
     * @param callback   Callback to invoke when the authentication process finishes
     * @param parameters Additional parameters for the authentication process
     */
    public void authenticate(String provider, Context context, HashMap<String, String> parameters, final UserAuthenticationCallback callback) {

        ListenableFuture<MobileServiceUser> authenticateFuture = authenticate(provider, context, parameters);

        Futures.addCallback(authenticateFuture, new FutureCallback<MobileServiceUser>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(MobileServiceUser user) {
                callback.onCompleted(user, null, null);
            }
        }, MoreExecutors.directExecutor());
    }

    /**
     * Invokes Microsoft Azure Mobile Service authentication using a
     * provider-specific oAuth token
     *
     * @param provider   The provider used for the authentication process
     * @param oAuthToken The oAuth token used for authentication
     * @param parameters Additional parameters for the authentication process
     */

    public ListenableFuture<MobileServiceUser> authenticate(String provider, String oAuthToken, HashMap<String, String> parameters) {
        if (oAuthToken == null || oAuthToken.trim().isEmpty()) {
            throw new IllegalArgumentException("oAuthToken can not be null or empty");
        }

        String path = UriHelper.CombinePath(LoginManager.START_URL, UriHelper.normalizeProvider(provider));
        if (mClient.getLoginUriPrefix() != null) {
            path = UriHelper.CombinePath(this.mClient.getLoginUriPrefix(), UriHelper.normalizeProvider(provider));
        }

        URL appUrl = UriHelper.createHostOnlyUrl(mClient.getAppUrl());

        // Create the login URL
        String url = UriHelper.CombinePath(appUrl.toString(), path);

        if (this.mClient.getAlternateLoginHost() != null) {
            url = UriHelper.CombinePath(mClient.getAlternateLoginHost().toString(), path);
        }
        url = url + UriHelper.normalizeParameters(parameters);

        return authenticateWithToken(oAuthToken, url);
    }

    /**
     * Invokes Microsoft Azure Mobile Service authentication using a
     * provider-specific oAuth token
     *
     * @param provider   The provider used for the authentication process
     * @param oAuthToken The oAuth token used for authentication
     */

    public ListenableFuture<MobileServiceUser> authenticate(String provider, String oAuthToken) {
        return authenticate(provider, oAuthToken, (HashMap<String, String>) null);
    }

    /**
     * Invokes Microsoft Azure Mobile Service authentication using a
     * provider-specific oAuth token
     *
     * @param provider   The provider used for the authentication process
     * @param oAuthToken The oAuth token used for authentication
     * @param callback   Callback to invoke when the authentication process finishes
     */
    public void authenticate(String provider, String oAuthToken, final UserAuthenticationCallback callback) {
        authenticate(provider, oAuthToken, null, callback);
    }

    /**
     * Invokes Microsoft Azure Mobile Service authentication using a
     * provider-specific oAuth token
     *
     * @param provider   The provider used for the authentication process
     * @param oAuthToken The oAuth token used for authentication
     * @param parameters Additional parameters for the authentication process
     * @param callback   Callback to invoke when the authentication process finishes
     */
    public void authenticate(String provider, String oAuthToken, HashMap<String, String> parameters, final UserAuthenticationCallback callback) {
        ListenableFuture<MobileServiceUser> authenticateFuture = authenticate(provider, oAuthToken, parameters);

        Futures.addCallback(authenticateFuture, new FutureCallback<MobileServiceUser>() {
            @Override
            public void onFailure(Throwable exception) {
                if (exception instanceof Exception) {
                    callback.onCompleted(null, (Exception) exception, MobileServiceException.getServiceResponse(exception));
                } else {
                    callback.onCompleted(null, new Exception(exception), MobileServiceException.getServiceResponse(exception));
                }
            }

            @Override
            public void onSuccess(MobileServiceUser user) {
                callback.onCompleted(user, null, null);
            }
        }, MoreExecutors.directExecutor());
    }

    /**
     * Creates the UI for the interactive authentication process
     *
     * @param startUrl The initial URL for the authentication process
     * @param endUrl   The final URL for the authentication process
     * @param context  The context used to create the authentication dialog
     * @param callback Callback to invoke when the authentication process finishes
     */
    protected void showLoginUI(final String startUrl, final String endUrl, final Context context, final LoginUIOperationCallback callback) {
        if (context == null) {
            throw new IllegalArgumentException("context cannot be null");
        }

        Activity activity = (Activity) context;

        activity.runOnUiThread(new Runnable() {
            public void run() {
                showLoginUIInternal(startUrl, endUrl, context, callback);
            }
        });
    }

    /**
     * Creates the UI for the interactive authentication process
     *
     * @param startUrl The initial URL for the authentication process
     * @param endUrl   The final URL for the authentication process
     * @param context  The context used to create the authentication dialog
     * @param callback Callback to invoke when the authentication process finishes
     */
    private void showLoginUIInternal(final String startUrl, final String endUrl, final Context context, LoginUIOperationCallback callback) {
        if (startUrl == null || startUrl.isEmpty()) {
            throw new IllegalArgumentException("startUrl can not be null or empty");
        }

        if (endUrl == null || endUrl.isEmpty()) {
            throw new IllegalArgumentException("endUrl can not be null or empty");
        }

        if (context == null) {
            throw new IllegalArgumentException("context can not be null");
        }

        final LoginUIOperationCallback externalCallback = callback;
        final AlertDialog.Builder builder = new AlertDialog.Builder(context);
        // Create the Web View to show the login page
        final WebView wv = new WebView(context);
        builder.setOnCancelListener(new DialogInterface.OnCancelListener() {

            @Override
            public void onCancel(DialogInterface dialog) {
                if (externalCallback != null) {
                    externalCallback.onCompleted(null, new MobileServiceException("User Canceled"));
                }
            }
        });

        wv.getSettings().setJavaScriptEnabled(true);

        DisplayMetrics displaymetrics = new DisplayMetrics();
        ((Activity) context).getWindowManager().getDefaultDisplay().getMetrics(displaymetrics);
        int webViewHeight = displaymetrics.heightPixels - 100;

        wv.setLayoutParams(new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, webViewHeight));

        wv.requestFocus(View.FOCUS_DOWN);
        wv.setOnTouchListener(new View.OnTouchListener() {

            @Override
            public boolean onTouch(View view, MotionEvent event) {
                int action = event.getAction();
                if (action == MotionEvent.ACTION_DOWN || action == MotionEvent.ACTION_UP) {
                    if (!view.hasFocus()) {
                        view.requestFocus();
                    }
                }

                return false;
            }
        });

        // Create a LinearLayout and add the WebView to the Layout
        LinearLayout layout = new LinearLayout(context);
        layout.setOrientation(LinearLayout.VERTICAL);
        layout.addView(wv);

        // Add a dummy EditText to the layout as a workaround for a bug
        // that prevents showing the keyboard for the WebView on some devices
        EditText dummyEditText = new EditText(context);
        dummyEditText.setVisibility(View.GONE);
        layout.addView(dummyEditText);

        // Add the layout to the dialog
        builder.setView(layout);

        final AlertDialog dialog = builder.create();

        wv.setWebViewClient(new WebViewClient() {

            @Override
            public void onPageStarted(WebView view, String url, Bitmap favicon) {
                // If the URL of the started page matches with the final URL
                // format, the login process finished

                if (isFinalUrl(url)) {
                    if (externalCallback != null) {
                        externalCallback.onCompleted(url, null);
                    }

                    dialog.dismiss();
                }

                super.onPageStarted(view, url, favicon);
            }

            // Checks if the given URL matches with the final URL's format
            private boolean isFinalUrl(String url) {
                if (url == null) {
                    return false;
                }

                return url.toLowerCase().startsWith(endUrl.toLowerCase());
            }

            // Checks if the given URL matches with the start URL's format
            private boolean isStartUrl(String url) {
                if (url == null) {
                    return false;
                }

                return url.toLowerCase().startsWith(startUrl.toLowerCase());
            }

            @Override
            public void onPageFinished(WebView view, String url) {
                if (isStartUrl(url)) {
                    if (externalCallback != null) {
                        externalCallback.onCompleted(null, new MobileServiceException("Logging in with the selected authentication provider is not enabled"));
                    }

                    dialog.dismiss();
                }
            }
        });

        wv.loadUrl(startUrl);
        dialog.show();
    }

    /**
     * Creates a User based on a Microsoft Azure Mobile Service JSON object
     * containing a UserId and Authentication Token
     *
     * @param json JSON object used to create the User
     * @return The created user if it is a valid JSON object. Null otherwise
     * @throws MobileServiceException
     */
    private MobileServiceUser createUserFromJSON(JsonObject json) throws MobileServiceException {
        if (json == null) {
            throw new IllegalArgumentException("json can not be null");
        }

        // If the JSON object is valid, create a MobileServiceUser object
        if (json.has(USER_JSON_PROPERTY)) {
            JsonObject jsonUser = json.getAsJsonObject(USER_JSON_PROPERTY);

            if (!jsonUser.has(USERID_JSON_PROPERTY)) {
                throw new JsonParseException(USERID_JSON_PROPERTY + " property expected");
            }
            String userId = jsonUser.get(USERID_JSON_PROPERTY).getAsString();

            MobileServiceUser user = new MobileServiceUser(userId);

            if (!json.has(TOKEN_JSON_PARAMETER)) {
                throw new JsonParseException(TOKEN_JSON_PARAMETER + " property expected");
            }

            user.setAuthenticationToken(json.get(TOKEN_JSON_PARAMETER).getAsString());
            return user;
        } else {
            // If the JSON contains an error property show it, otherwise raise
            // an error with JSON content
            if (json.has("error")) {
                throw new MobileServiceException(json.get("error").getAsString());
            } else {
                throw new JsonParseException(json.toString());
            }
        }
    }

    /**
     * Invokes Microsoft Azure Mobile Services authentication using the specified
     * token
     *
     * @param token The token used for authentication
     * @param url   The URL used for the authentication process
     */
    private ListenableFuture<MobileServiceUser> authenticateWithToken(String token, String url) {
        final SettableFuture<MobileServiceUser> future = SettableFuture.create();

        if (token == null) {
            throw new IllegalArgumentException("token can not be null");
        }

        if (url == null) {
            throw new IllegalArgumentException("url can not be null");
        }

        // Create a request
        final ServiceFilterRequest request =
                ServiceFilterRequestImpl.post(mClient.getOkHttpClientFactory(), url, token.getBytes());

        final MobileServiceConnection connection = mClient.createConnection();

        // Create the AsyncTask that will execute the request
        new RequestAsyncTask(request, connection) {
            @Override
            protected void onPostExecute(ServiceFilterResponse response) {
                if (mTaskException == null && response != null) {
                    MobileServiceUser user;
                    try {
                        // Get the user from the response and create a
                        // MobileServiceUser object from the JSON
                        String content = response.getContent();
                        user = createUserFromJSON((JsonObject) new JsonParser().parse((content.trim())));

                    } catch (Exception e) {
                        future.setException(new MobileServiceException("Error while authenticating user.", e, response));
                        return;
                    }

                    future.set(user);
                } else {
                    future.setException(new MobileServiceException("Error while authenticating user.", mTaskException));
                }
            }
        }.executeTask();

        return future;
    }

    private HashMap<String, String> addSessionMode(HashMap<String, String> parameters) {
        if (parameters == null) {
            parameters = new HashMap<>();
        } else {
            parameters = new HashMap<>(parameters);
        }
        parameters.put("session_mode", "token");
        return parameters;
    }

    /**
     * Internal callback used after the interactive authentication UI is
     * completed
     */
    protected interface LoginUIOperationCallback {
        /**
         * Method to call if the operation finishes successfully
         *
         * @param url The final login URL
         */
        void onCompleted(String url, Exception exception);
    }
}
