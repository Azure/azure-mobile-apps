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
package com.microsoft.windowsazure.mobileservices.sdk.testapp.test;

import android.content.Context;
import android.net.Uri;
import android.test.InstrumentationTestCase;
import android.util.Log;
import android.util.MalformedJsonException;
import android.os.Looper;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.google.gson.JsonParseException;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.authentication.CustomTabsLoginActivity;
import com.microsoft.windowsazure.mobileservices.authentication.CustomTabsLoginManager;
import com.microsoft.windowsazure.mobileservices.authentication.CustomTabsLoginState;
import com.microsoft.windowsazure.mobileservices.authentication.LoginManager;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.ResultsContainer;
import com.microsoft.windowsazure.mobileservices.table.sync.MobileServiceSyncTable;
import okhttp3.Headers;
import okhttp3.Protocol;
import okhttp3.internal.http.StatusLine;

import junit.framework.Assert;

import java.lang.reflect.Method;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.Arrays;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;
import java.util.concurrent.CountDownLatch;

public class CustomTabsLoginTests extends InstrumentationTestCase {

    private static final int MIN_CODE_VERIFIER_LENGTH = 43;

    String appUrl = "";

    @Override
    protected void setUp() throws Exception {
        appUrl = "https://myapp.com/";
        super.setUp();
    }

    public void testSha256Base64Encode() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("sha256Base64Encode", String.class);
        method.setAccessible(true);

        // sha256Base64Encode works when input String is not empty
        String result = (String) method.invoke(null, "abcde");
        assertEquals("NrvlDtloQdEEQ7y2cNZVTwo0t2G+Z+ycSorSwMRMpCw=", result);

        // sha256Base64Encode works when input String is null
        result = (String) method.invoke(null, (String) null);
        assertEquals(null, result);

        // sha256Base64Encode works when input String is empty
        result = (String) method.invoke(null, "");
        assertEquals(null, result);
    }

    public void testGenerateRandomCodeVerifier() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("generateRandomCodeVerifier");
        method.setAccessible(true);

        String result = (String) method.invoke(null);

        // make sure generateRandomCodeVerifier output is no less than MIN_CODE_VERIFIER_LENGTH
        assertNotNull(result);
        assertTrue(result.length() >= MIN_CODE_VERIFIER_LENGTH);
    }

    public void testRedirectURLValid() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("isRedirectURLValid", Uri.class, String.class);
        method.setAccessible(true);

        // valid when redirect url matches uri scheme
        Uri validUri = Uri.parse("zumoe2etest://easyauth.callback");
        assertTrue((boolean) method.invoke(null, validUri, "ZumoE2ETest"));

        // valid when redirect url matches uri scheme in case insensitive manner
        validUri = Uri.parse("zumoe2etest://easyauth.callback");
        assertTrue((boolean) method.invoke(null, validUri, "zumoe2etest"));

        // valid when redirect url matches uri scheme and authorization_code exists
        validUri = Uri.parse("zumoe2etest://easyauth.callback/#authorization_code=12345");
        assertTrue((boolean) method.invoke(null, validUri, "zumoe2etest"));

        // valid when redirect url is longer than one word and matches uri scheme
        validUri = Uri.parse("com.example.zumoe2etest://easyauth.callback/#authorization_code=12345");
        assertTrue((boolean) method.invoke(null, validUri, "com.example.zumoe2etest"));
    }

    public void testRedirectURLInvalid() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("isRedirectURLValid", Uri.class, String.class);
        method.setAccessible(true);

        // invalid when redirect url is null
        Uri invalidUri = (Uri) null;
        assertFalse((boolean) method.invoke(null, invalidUri, "zumoe2etest"));

        // invalid when redirect url is malformed
        invalidUri = Uri.parse("zumoe2etest:///easyauth.callback/#authorization_code=12345");
        assertFalse((boolean) method.invoke(null, invalidUri, "zumoe2etest"));

        // invalid when redirect url doesn't match "easyauth.callback"
        invalidUri = Uri.parse("zumoe2etest://other.callback/#authorization_code=12345");
        assertFalse((boolean) method.invoke(null, invalidUri, "zumoe2etest"));

        // invalid when redirect url doesn't match uri scheme
        invalidUri = Uri.parse("forbar://easyauth.callback/#authorization_code=12345");
        assertFalse((boolean) method.invoke(null, invalidUri, "zumoe2etest"));
    }

    public void testAuthorizationCodeFromRedirectUrl() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("authorizationCodeFromRedirectUrl", Uri.class);
        method.setAccessible(true);

        Uri redirectUrl = Uri.parse("zumoe2etest://easyauth.callback/#authorization_code=code123");
        assertEquals("code123", (String) method.invoke(null, redirectUrl));

        redirectUrl = Uri.parse("zumoe2etest://easyauth.callback/#authorization_code=abc%2Fde%3D%3D");
        assertEquals("abc/de==", (String) method.invoke(null, redirectUrl));
    }

    public void testAuthorizationCodeNotFoundFromRedirectUrl() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("authorizationCodeFromRedirectUrl", Uri.class);
        method.setAccessible(true);

        Uri redirectUrl = Uri.parse("zumoe2etest://easyauth.callback/");
        assertEquals(null, (String) method.invoke(null, redirectUrl));
    }

    public void testBuildLoginUriWithParameters() throws Throwable {
        HashMap<String, String> params = new HashMap<>();
        params.put("param1", "value1");
        params.put("param2", "value/2==");

        this.testBuildLoginUri(params, Uri.parse("https://myapp.com/.auth/login/google?post_login_redirect_url=zumoe2etest%3A%2F%2Feasyauth.callback&session_mode=token&code_challenge_method=S256&code_challenge=4iF9Pk4SDGozcqGJDwPiMrNa1lnXH3piUBpO4gSj5m0%3D&param1=value1&param2=value%2F2%3D%3D"));
    }

    public void testBuildLoginUriWithNullParameters() throws Throwable {

        this.testBuildLoginUri(null, Uri.parse("https://myapp.com/.auth/login/google?code_challenge_method=S256&code_challenge=4iF9Pk4SDGozcqGJDwPiMrNa1lnXH3piUBpO4gSj5m0%3D&post_login_redirect_url=zumoe2etest%3A%2F%2Feasyauth.callback&session_mode=token"));
    }

    private void testBuildLoginUri(HashMap<String, String> parameters, Uri expectedUri) throws Throwable {
        Method method = CustomTabsLoginActivity.class.getDeclaredMethod("buildLoginUri", CustomTabsLoginState.class);
        method.setAccessible(true);

        String provider = "google";
        String uriScheme = "zumoe2etest";
        String codeVerifier = "67890";
        CustomTabsLoginState loginState = new CustomTabsLoginState(uriScheme, codeVerifier, provider, appUrl, null, null, parameters);

        assertUriEquals(expectedUri, (Uri) method.invoke(null, loginState));
    }

    /**
     * Compare URIs with ignore parameters order.
     */
    private void assertUriEquals(Uri uri1, Uri uri2) {
        assertEquals(uri1.getScheme(), uri2.getScheme());
        assertEquals(uri1.getAuthority(), uri2.getAuthority());
        assertEquals(uri1.getHost(), uri2.getHost());
        assertEquals(uri1.getPort(), uri2.getPort());
        assertEquals(uri1.getPath(), uri2.getPath());
        String[] params1 = uri1.getQuery().split("&");
        String[] params2 = uri2.getQuery().split("&");
        assertEquals(params1.length, params2.length);
        Arrays.sort(params1);
        Arrays.sort(params2);
        assertTrue("Parameters are not equal", Arrays.equals(params1, params2));
        assertEquals(uri1.getFragment(), uri2.getFragment());
    }

    public void testBuildUrlPathWithLoginUriPrefixAndAlternateLoginHost() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("buildUrlPath", String.class, String.class, String.class, String.class);
        method.setAccessible(true);

        String provider = "google";
        String loginUriPrefix = ".auth/login/foobar";
        String alternateLoginHost = "https://localhost";

        assertEquals("https://localhost/.auth/login/foobar/google", (String) method.invoke(null, provider, appUrl, loginUriPrefix, alternateLoginHost));
    }

    public void testBuildUrlPath() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("buildUrlPath", String.class, String.class, String.class, String.class);
        method.setAccessible(true);

        String provider = "google";
        String loginUriPrefix = null;
        String alternateLoginHost = null;

        assertEquals("https://myapp.com/.auth/login/google", (String) method.invoke(null, provider, appUrl, loginUriPrefix, alternateLoginHost));
    }

    public void testBuildCodeExchangeUrl() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("buildCodeExchangeUrl", CustomTabsLoginState.class, String.class);
        method.setAccessible(true);

        CustomTabsLoginState loginState = new CustomTabsLoginState("zumoe2etest", "verifier123", "google", "https://appurl.com", null, null, null);
        assertEquals("https://appurl.com/.auth/login/google/token?authorization_code=authcode123&code_verifier=verifier123", method.invoke(null, loginState, "authcode123"));
    }

    public void testBuildCodeExchangeUrlWithLoginPrefixAndAlternateLoginHost() throws Throwable {
        Method method = CustomTabsLoginManager.class.getDeclaredMethod("buildCodeExchangeUrl", CustomTabsLoginState.class, String.class);
        method.setAccessible(true);

        CustomTabsLoginState loginState = new CustomTabsLoginState("zumoe2etest", "verifier123", "google", "https://appurl.com", ".auth/login/foobar", "https://localhost", null);
        assertEquals("https://localhost/.auth/login/foobar/google/token?authorization_code=authcode123&code_verifier=verifier123", method.invoke(null, loginState, "authcode123"));
    }

    public void testPerformCodeExchange() throws Throwable {
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        CustomTabsLoginActivityMock customTabsLoginActivityMock = new CustomTabsLoginActivityMock();

        CustomTabsLoginState loginState = new CustomTabsLoginState("urlscheme", "codeVerfier", "google", "http://appurl.com", null, null, null);
        customTabsLoginActivityMock.setupLoginState(loginState);

        final ResultsContainer result = new ResultsContainer();

        // Add a new filter to the client
        MobileServiceClient client = new MobileServiceClient("https://appurl.com", getInstrumentation().getTargetContext());
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{authenticationToken:'123abc', user:{userId:'123456'}}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        customTabsLoginActivityMock.setupMobileServiceClient(client);

        UserAuthenticationCallback callback = new UserAuthenticationCallback() {
            @Override
            public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                assertNotNull(user);
                assertEquals("123456", user.getUserId());
                assertEquals("123abc", user.getAuthenticationToken());
                assertNull(response);
                assertNull(exception);
            }
        };
        customTabsLoginActivityMock.performCodeExchange("https://appurl.com/.auth/login/google/token?authorization_code=authcode123&code_verifier=verifier123", callback);
    }

    public void testPerformCodeExchangeResponseContainsErrorMessage() throws Throwable {
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        CustomTabsLoginActivityMock customTabsLoginActivityMock = new CustomTabsLoginActivityMock();

        CustomTabsLoginState loginState = new CustomTabsLoginState("urlscheme", "codeVerfier", "google", "http://appurl.com", null, null, null);
        customTabsLoginActivityMock.setupLoginState(loginState);

        final ResultsContainer result = new ResultsContainer();
        final String errorMessage = "foobar error message";

        // Add a new filter to the client
        MobileServiceClient client = new MobileServiceClient("https://appurl.com", getInstrumentation().getTargetContext());
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{error:'" + errorMessage + "'}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        customTabsLoginActivityMock.setupMobileServiceClient(client);

        UserAuthenticationCallback callback = new UserAuthenticationCallback() {
            @Override
            public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                assertTrue(exception instanceof MobileServiceException);
                MobileServiceException cause = (MobileServiceException) exception.getCause();
                assertEquals(errorMessage, cause.getMessage());
                assertNull(user);
            }
        };
        customTabsLoginActivityMock.performCodeExchange("https://appurl.com/.auth/login/google/token?authorization_code=authcode123&code_verifier=verifier123", callback);
    }

    public void testPerformCodeExchangeMalformedJsonResponse() throws Throwable {
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        CustomTabsLoginActivityMock customTabsLoginActivityMock = new CustomTabsLoginActivityMock();

        CustomTabsLoginState loginState = new CustomTabsLoginState("urlscheme", "codeVerfier", "google", "http://appurl.com", null, null, null);
        customTabsLoginActivityMock.setupLoginState(loginState);

        final ResultsContainer result = new ResultsContainer();

        // Add a new filter to the client
        MobileServiceClient client = new MobileServiceClient("https://appurl.com", getInstrumentation().getTargetContext());
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                String malformedJson = "authenticationToken:'123abc', user:{userId:'123456'}}";
                response.setContent(malformedJson);

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        customTabsLoginActivityMock.setupMobileServiceClient(client);

        UserAuthenticationCallback callback = new UserAuthenticationCallback() {
            @Override
            public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                assertNull(user);
                assertNotNull(response);
                assertTrue(exception instanceof MobileServiceException);
                assertTrue(exception.getCause() instanceof com.google.gson.JsonSyntaxException);
            }
        };
        customTabsLoginActivityMock.performCodeExchange("https://appurl.com/.auth/login/google/token?authorization_code=authcode123&code_verifier=verifier123", callback);
    }

    public void testPerformCodeExchangeResponseEmptyJson() throws Throwable {
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        this.testPerformCodeExchangeInvalidJsonResponse("{}", "{}");
    }

    public void testPerformCodeExchangeResponseMissingUserId() throws Throwable {
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        this.testPerformCodeExchangeInvalidJsonResponse("{authenticationToken:'123abc', user:{}}", "userId property expected");
    }

    public void testPerformCodeExchangeResponseMissingToken() throws Throwable {
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        this.testPerformCodeExchangeInvalidJsonResponse("{user:{userId:'123456'}}", "authenticationToken property expected");
    }

    private void testPerformCodeExchangeInvalidJsonResponse(final String jsonResponse, final String expectedErrorMessage) throws Throwable {
        if (Looper.myLooper() == null)
        {
            Looper.prepare();
        }
        CustomTabsLoginActivityMock customTabsLoginActivityMock = new CustomTabsLoginActivityMock();

        CustomTabsLoginState loginState = new CustomTabsLoginState("urlscheme", "codeVerfier", "google", "http://appurl.com", null, null, null);
        customTabsLoginActivityMock.setupLoginState(loginState);

        final ResultsContainer result = new ResultsContainer();

        // Add a new filter to the client
        MobileServiceClient client = new MobileServiceClient("https://appurl.com", getInstrumentation().getTargetContext());
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent(jsonResponse);

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        customTabsLoginActivityMock.setupMobileServiceClient(client);

        UserAuthenticationCallback callback = new UserAuthenticationCallback() {
            @Override
            public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                assertNull(user);
                assertNotNull(response);
                assertTrue(exception instanceof MobileServiceException);
                assertTrue(exception.getCause() instanceof JsonParseException);
                assertEquals(expectedErrorMessage, exception.getCause().getMessage());
            }
        };
        customTabsLoginActivityMock.performCodeExchange("https://appurl.com/.auth/login/google/token?authorization_code=authcode123&code_verifier=verifier123", callback);
    }

    class CustomTabsLoginActivityMock extends CustomTabsLoginActivity {

        public void setupLoginState(CustomTabsLoginState loginState) {
            super.setLoginState(loginState);
        }

        public void setupMobileServiceClient(MobileServiceClient client) {
            super.setMobileServiceClient(client);
        }

        @Override
        protected void performCodeExchange(String codeExchangeUrl, final UserAuthenticationCallback callback) {
            super.performCodeExchange(codeExchangeUrl, callback);
        }

    }
}
