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
import android.test.InstrumentationTestCase;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.SettableFuture;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceException;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.authentication.LoginManager;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.NextServiceFilterCallback;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilter;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterRequest;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.filters.ServiceFilterResponseMock;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.ResultsContainer;
import okhttp3.Headers;
import okhttp3.Protocol;
import okhttp3.internal.http.StatusLine;

import junit.framework.Assert;

import java.net.MalformedURLException;
import java.net.URL;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;
import java.util.concurrent.CountDownLatch;

public class LoginTests extends InstrumentationTestCase {
    String appUrl = "";
    String urlPrefix = "";

    @Override
    protected void setUp() throws Exception {
        appUrl = "http://myapp.com/";
        urlPrefix = ".auth/login/";
        super.setUp();
    }

    public void testRefreshUserSuccess() throws Throwable {
        testRefreshUserSetup(null);
    }

    public void testRefreshUserSuccessWithAlternateLoginHost() throws Throwable {
        testRefreshUserSetup(new URL("https://www.testalternatelogin.com/"));
    }

    private void testRefreshUserSetup(URL alternateLoginHost) throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        final String expectedRefreshRequestUrl = alternateLoginHost != null ? alternateLoginHost.toString() + ".auth/refresh" : appUrl + ".auth/refresh";

        if (alternateLoginHost != null) {
            client.setAlternateLoginHost(alternateLoginHost);
        }

        MobileServiceUser user = new MobileServiceUser("123456");
        user.setAuthenticationToken("old-auth-token");
        client.setCurrentUser(user);

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());
                assertEquals(expectedRefreshRequestUrl, request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{authenticationToken:'new-auth-token', user:{userId:'123456'}}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        user = client.refreshUser().get();

        assertNotNull(user);
        assertEquals("123456", user.getUserId());
        assertEquals("new-auth-token", user.getAuthenticationToken());
    }

    public void testRefreshUserThrowsWhenUserIsNotLoggedIn() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        MobileServiceUser user = null;
        try {
            user = client.refreshUser().get();
        } catch (Exception ex) {
            assertNull(user);
            assertTrue(ex.getCause() instanceof MobileServiceException);
            assertEquals("MobileServiceUser must be set before calling refresh", ex.getCause().getMessage());
        }
    }

    public void testRefreshUserThrowsOn400Error() throws Throwable {
        testRefreshUserThrowsOnExceptionSetup(400, "Refresh failed with a 400 Bad Request error. The identity provider does not support refresh, or the user is not logged in with sufficient permission.");
    }

    public void testRefreshUserThrowsOn401Error() throws Throwable {
        testRefreshUserThrowsOnExceptionSetup(401, "Refresh failed with a 401 Unauthorized error. Credentials are no longer valid.");
    }

    public void testRefreshUserThrowsOn403Error() throws Throwable {
        testRefreshUserThrowsOnExceptionSetup(403, "Refresh failed with a 403 Forbidden error. The refresh token was revoked or expired.");
    }

    public void testRefreshUserThrowsOn500Error() throws Throwable {
        testRefreshUserThrowsOnExceptionSetup(500, "Refresh failed due to an unexpected error.");
    }

    private void testRefreshUserThrowsOnExceptionSetup(final int statusCode, String expectedErrorMessage) throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        MobileServiceUser user = new MobileServiceUser("123456");
        user.setAuthenticationToken("old-auth-token");
        client.setCurrentUser(user);

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setStatus((new StatusLine(Protocol.HTTP_2, statusCode, "")));

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.setException(new MobileServiceException("", response));

                return resultFuture;
            }
        });

        try {
            user = client.refreshUser().get();
        } catch (Exception ex) {
            assertEquals(expectedErrorMessage, ex.getCause().getMessage());
            assertTrue(ex.getCause() instanceof MobileServiceException);
        }
    }

    public void testRefreshUserThrowsOnEmptyJsonResponse() throws Throwable {
        testRefreshUserThrowsOnInvalidJsonResponseSetup("{}");
    }

    public void testRefreshUserThrowsOnJsonResponseMissingUserId() throws Throwable {
        testRefreshUserThrowsOnInvalidJsonResponseSetup("{authenticationToken:'123abc', user:{}}");
    }

    public void testRefreshUserThrowsOnJsonResponseMissingAuthToken() throws Throwable {
        testRefreshUserThrowsOnInvalidJsonResponseSetup("{user:{userId:'123456'}}");
    }

    private void testRefreshUserThrowsOnInvalidJsonResponseSetup(final String jsonResponse) {
        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }


        MobileServiceUser user = new MobileServiceUser("123456");
        user.setAuthenticationToken("old-auth-token");
        client.setCurrentUser(user);

        // Add a new filter to the client
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

        try {
            user = client.refreshUser().get();
            Assert.fail();
        } catch (Exception exception) {
            assertTrue(exception.getCause() instanceof MobileServiceException);
            assertEquals("Error on refresh user.", exception.getCause().getMessage());
        }
    }

    public void testLoginOperationWithOAuthToken() throws Throwable {
        testLoginOperationWithOAuthToken(MobileServiceAuthenticationProvider.Facebook);
        testLoginOperationWithOAuthToken(MobileServiceAuthenticationProvider.Twitter);
        testLoginOperationWithOAuthToken(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginOperationWithOAuthToken(MobileServiceAuthenticationProvider.Google);

        testLoginOperationWithOAuthToken("FaCeBoOk");
        testLoginOperationWithOAuthToken("twitter");
        testLoginOperationWithOAuthToken("MicrosoftAccount");
        testLoginOperationWithOAuthToken("GOOGLE");
    }

    private void testLoginOperationWithOAuthToken(final Object provider) throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
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

        try {

            MobileServiceUser user = null;

            if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
                user = client.login((MobileServiceAuthenticationProvider) provider, "{myToken:123}").get();

            } else {
                user = client.login((String) provider, "{myToken:123}").get();
            }

            assertEquals("123456", user.getUserId());
            assertEquals("123abc", user.getAuthenticationToken());

        } catch (Exception exception) {
            Assert.fail();
        }

        // Assert
        assertEquals(buildExpectedUrl(provider, null), result.getRequestUrl());
    }

    public void testLoginOperationWithParameter() throws Throwable {
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.Facebook);
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.Twitter);
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginOperationWithParameter(MobileServiceAuthenticationProvider.Google);

        testLoginOperationWithParameter("FaCeBoOk");
        testLoginOperationWithParameter("twitter");
        testLoginOperationWithParameter("MicrosoftAccount");
        testLoginOperationWithParameter("GOOGLE");
    }

    private void testLoginOperationWithParameter(final Object provider) throws Throwable {
        HashMap<String, String> parameters = new HashMap<>();
        parameters.put("p1", "p1value");
        parameters.put("p2", "p2value");
        testLoginOperationWithParameter(provider, parameters);
    }

    private void testLoginOperationWithParameter(final Object provider, HashMap<String, String> parameters) throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
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

        try {
            ListenableFuture<MobileServiceUser> future;
            if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
                future = client.login((MobileServiceAuthenticationProvider) provider, "{myToken:123}", parameters);
            } else {
                future = client.login((String) provider, "{myToken:123}", parameters);
            }

            Futures.addCallback(future, new FutureCallback<MobileServiceUser>() {
                @Override
                public void onFailure(Throwable exception) {
                    Assert.fail();
                }

                @Override
                public void onSuccess(MobileServiceUser user) {
                    assertNotNull(user);
                    assertEquals("123456", user.getUserId());
                    assertEquals("123abc", user.getAuthenticationToken());
                }
            });
        } catch (Exception exception) {
            Assert.fail();
        }
    }

    public void testLoginCallbackOperation() throws Throwable {
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.Facebook);
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.Twitter);
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginCallbackOperation(MobileServiceAuthenticationProvider.Google);

        testLoginCallbackOperation("FaCeBoOk");
        testLoginCallbackOperation("twitter");
        testLoginCallbackOperation("MicrosoftAccount");
        testLoginCallbackOperation("GOOGLE");
    }

    @SuppressWarnings("deprecation")
    private void testLoginCallbackOperation(final Object provider) throws Throwable {
        final CountDownLatch latch = new CountDownLatch(1);
        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
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

        UserAuthenticationCallback callback = new UserAuthenticationCallback() {

            @Override
            public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                if (exception == null) {
                    assertEquals("123456", user.getUserId());
                    assertEquals("123abc", user.getAuthenticationToken());
                } else {
                    Assert.fail();
                }

                latch.countDown();
            }
        };

        if (provider.getClass().equals(MobileServiceAuthenticationProvider.class)) {
            client.login((MobileServiceAuthenticationProvider) provider, "{myToken:123}", callback);
        } else {
            client.login((String) provider, "{myToken:123}", callback);
        }

        latch.await();

        // Assert
        assertEquals(buildExpectedUrl(provider, null), result.getRequestUrl());
    }

    public void testLoginShouldThrowError() throws Throwable {

        testLoginShouldThrowError(MobileServiceAuthenticationProvider.Facebook);
        testLoginShouldThrowError(MobileServiceAuthenticationProvider.MicrosoftAccount);
        testLoginShouldThrowError(MobileServiceAuthenticationProvider.Twitter);
        testLoginShouldThrowError(MobileServiceAuthenticationProvider.Google);
    }

    class LoginManagerMock extends LoginManager {
        String mStartUrl = "";

        public LoginManagerMock(MobileServiceClient client) {
            super(client);
        }

        @Override
        protected void showLoginUI(final String startUrl, final String endUrl, final Context context, final LoginUIOperationCallback callback) {
            this.mStartUrl = startUrl;
        }
    }

    public void testSessionMode() throws Throwable {
        MobileServiceClient client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        LoginManagerMock loginManager = new LoginManagerMock(client);

        HashMap<String, String> params = new HashMap<>();
        params.put("session_mode", "cookie");

        loginManager.authenticate("Facebook", (Context) null, params);

        // no modification to user supplied params object
        assertEquals("cookie", params.get("session_mode"));

        // url overrides user supplied session mode with token
        assertTrue(loginManager.mStartUrl.contains("session_mode=token"));
        assertFalse(loginManager.mStartUrl.contains("session_mode=cookie"));
    }

    private void testLoginShouldThrowError(final MobileServiceAuthenticationProvider provider) throws Throwable {
        final ResultsContainer result = new ResultsContainer();

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                result.setRequestUrl(request.getUrl());

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setStatus(new StatusLine(Protocol.HTTP_2, 400, ""));

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.setException(new MobileServiceException("", response));

                return resultFuture;
            }
        });

        try {
            client.login(provider, "{myToken:123}").get();
            Assert.fail();
        } catch (Exception exception) {
            assertTrue(exception.getCause() instanceof MobileServiceException);
            assertEquals("Error while authenticating user.", exception.getCause().getMessage());
        }
    }

    public void testAuthenticatedRequest() throws Throwable {
        final MobileServiceUser user = new MobileServiceUser("dummyUser");
        user.setAuthenticationToken("123abc");

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
            client.setCurrentUser(user);
        } catch (MalformedURLException e) {
        }

        // Add a new filter to the client
        client = client.withFilter(new ServiceFilter() {

            @Override
            public ListenableFuture<ServiceFilterResponse> handleRequest(ServiceFilterRequest request, NextServiceFilterCallback nextServiceFilterCallback) {

                int headerIndex = -1;

                Headers headers = request.getHeaders();
                for (int i = 0; i < headers.size(); i++) {
                    if (headers.name(i) == "X-ZUMO-AUTH") {
                        headerIndex = i;
                    }
                }
                if (headerIndex == -1) {
                    Assert.fail();
                }

                assertEquals(user.getAuthenticationToken(), headers.value(headerIndex));

                ServiceFilterResponseMock response = new ServiceFilterResponseMock();
                response.setContent("{}");

                final SettableFuture<ServiceFilterResponse> resultFuture = SettableFuture.create();

                resultFuture.set(response);

                return resultFuture;
            }
        });

        client.getTable("dummy").execute().get();
    }

    public void testLoginWithEmptyTokenShouldFail() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        String token = null;
        try {
            client.login(MobileServiceAuthenticationProvider.Facebook, token).get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }

        try {
            client.login(MobileServiceAuthenticationProvider.Facebook, "").get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }
    }

    public void testLoginWithEmptyProviderShouldFail() throws Throwable {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        String provider = null;
        try {
            client.login(provider, "{myToken:123}").get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }

        try {
            client.login("", "{myToken:123}").get();
            Assert.fail();
        } catch (IllegalArgumentException e) {
            // It should throw an exception
        }
    }

    public void testLogout() {

        // Create client
        MobileServiceClient client = null;
        try {
            client = new MobileServiceClient(appUrl, getInstrumentation().getTargetContext());
        } catch (MalformedURLException e) {
        }

        client.setCurrentUser(new MobileServiceUser("abc"));

        try {
            client.logout().get();
        } catch (Exception e) {
            Assert.fail(e.getMessage());
        }

        assertNull(client.getCurrentUser());
    }

    private String buildExpectedUrl(Object provider, HashMap<String, String> params) {
        if (params == null) {
            params = new HashMap<String, String>();
        }

        String paramString = "";
        for (Map.Entry<String, String> parameter : params.entrySet()) {
            if (paramString.isEmpty()) {
                paramString = "?";
            } else {
                paramString += "&";
            }
            paramString += parameter.getKey() + "=" + parameter.getValue();
        }

        return appUrl + urlPrefix + provider.toString().toLowerCase(Locale.getDefault()) + paramString;
    }
}
