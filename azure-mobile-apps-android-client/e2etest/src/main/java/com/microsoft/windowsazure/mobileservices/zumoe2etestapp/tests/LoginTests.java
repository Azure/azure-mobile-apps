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
package com.microsoft.windowsazure.mobileservices.zumoe2etestapp.tests;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.UserAuthenticationCallback;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceAuthenticationProvider;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.table.MobileServiceJsonTable;
import com.microsoft.windowsazure.mobileservices.table.TableDeleteCallback;
import com.microsoft.windowsazure.mobileservices.table.TableJsonOperationCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.LogServiceFilter;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;

import junit.framework.Assert;

import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Random;

public class LoginTests extends TestGroup {

    protected static final String USER_PERMISSION_TABLE_NAME = "authenticated";

    private static JsonObject lastUserIdentityObject;

    boolean isNetBackend;

    public LoginTests(boolean isNetBackend) {
        super("Login tests");

        this.isNetBackend = isNetBackend;

        this.addTest(createLogoutTest());
        this.addTest(createCRUDTest(USER_PERMISSION_TABLE_NAME, null, TablePermission.User, false));

        int indexOfStartAuthenticationTests = this.getTestCases().size();

        ArrayList<MobileServiceAuthenticationProvider> providersWithRecycledTokenSupport = new ArrayList<MobileServiceAuthenticationProvider>();
        providersWithRecycledTokenSupport.add(MobileServiceAuthenticationProvider.Facebook);
        providersWithRecycledTokenSupport.add(MobileServiceAuthenticationProvider.Twitter);
        providersWithRecycledTokenSupport.add(MobileServiceAuthenticationProvider.MicrosoftAccount);

        ArrayList<MobileServiceAuthenticationProvider> providersWithRefreshTokenSupport = new ArrayList<MobileServiceAuthenticationProvider>();
        providersWithRefreshTokenSupport.add(MobileServiceAuthenticationProvider.Google);
        providersWithRefreshTokenSupport.add(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory);
        providersWithRefreshTokenSupport.add(MobileServiceAuthenticationProvider.MicrosoftAccount);

        // Known bug - Drop login via Google token until Google client flow is
        // reintroduced
        // providersWithRecycledTokenSupport.add(MobileServiceAuthenticationProvider.Google);

        for (MobileServiceAuthenticationProvider provider : MobileServiceAuthenticationProvider.values()) {
            // basic crud auth test
            this.addTest(createLoginTest(provider));

            if (providersWithRefreshTokenSupport.contains(provider)) {
                this.addTest(createRefreshUserTest(provider));
            }

            this.addTest(createCRUDTest(USER_PERMISSION_TABLE_NAME, provider, TablePermission.User, true));
            this.addTest(createLogoutTest());

            // ensure logout has cleared auth cookies
            this.addTest(createCRUDTest(USER_PERMISSION_TABLE_NAME, provider, TablePermission.User, false));

            // client side login test, if supported
            if (providersWithRecycledTokenSupport.contains(provider)) {
                this.addTest(createClientSideLoginTest(provider));
                this.addTest(createCRUDTest(USER_PERMISSION_TABLE_NAME, provider, TablePermission.User, true));
                this.addTest(createLogoutTest());
            }

            // callback crud auth test
            this.addTest(createLoginWithCallbackTest(provider));
            this.addTest(createCRUDWithCallbackTest(USER_PERMISSION_TABLE_NAME, provider, TablePermission.User, true));
            this.addTest(createLogoutWithCallbackTest());

            // callback client side login test, if supported
            if (providersWithRecycledTokenSupport.contains(provider)) {
                this.addTest(createClientSideLoginWithCallbackTest(provider));
                this.addTest(createLogoutWithCallbackTest());
            }
        }

        // this.addTest(createLogoutTest());
        // this.addTest(createLoginWithGoogleAccountTest(true, null));
        //
        // this.addTest(createLogoutTest());
        // this.addTest(createLoginWithGoogleAccountTest(true,
        // MobileServiceClient.GOOGLE_USER_INFO_SCOPE +
        // " https://www.googleapis.com/auth/userinfo.email"));
        //
        // this.addTest(createLogoutTest());
        // this.addTest(createLoginWithGoogleAccountTest(false, null));

        List<TestCase> testCases = this.getTestCases();
        for (int i = indexOfStartAuthenticationTests; i < testCases.size(); i++) {
            testCases.get(i).setCanRunUnattended(false);
        }
    }

    public static TestCase createRefreshUserTest(final MobileServiceAuthenticationProvider provider) {
        TestCase test = new TestCase(provider.toString() + " RefreshUser") {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final TestCase testCase = this;
                    log("Calling the overload MobileServiceClient.refreshUser()");

                    TestResult result = new TestResult();
                    String userId = "";
                    String authToken = "";

                    try {
                        MobileServiceUser user = client.refreshUser().get();
                        userId = user.getUserId();
                        authToken = user.getAuthenticationToken();
                    } catch (Exception exception) {
                        log("Exception: " + exception.toString());
                    }

                    log("User " + userId + " refreshed with auth token: " + authToken);

                    MobileServiceUser currentUser = client.getCurrentUser();

                    if (currentUser == null) {
                        result.setStatus(TestStatus.Failed);
                    } else {
                        result.setStatus(TestStatus.Passed);
                    }
                    result.setTestCase(testCase);

                    callback.onTestComplete(testCase, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        return test;
    }

    public static TestCase createLoginTest(final MobileServiceAuthenticationProvider provider) {
        TestCase test = new TestCase(provider.toString() + " Login") {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    log("Calling the overload MobileServiceClient.login(MobileServiceAuthenticationProvider provider, HashMap<String, String> parameters)");

                    TestResult result = new TestResult();
                    String userId;

                    try {

                        HashMap<String, String> parameters = null;
                        if (provider == MobileServiceAuthenticationProvider.Facebook) {
                            parameters = new HashMap<>();

                            parameters.put("display", "popup");
                        }
                        if (provider == MobileServiceAuthenticationProvider.Google) {
                            // request offline permission for Google refresh token
                            parameters = new HashMap<>();
                            parameters.put("access_type", "offline");
                        }
                        if (provider == MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory) {
                            // request offline permission for AAD refresh token
                            parameters = new HashMap<>();
                            parameters.put("response_type", "code id_token");
                        }

                        MobileServiceUser user = client.login(provider, parameters).get();
                        userId = user.getUserId();

                    } catch (Exception exception) {
                        userId = "NULL";
                        log("Error during login, user == null");
                        log("Exception: " + exception.toString());

                    }

                    log("Logged in as " + userId);

                    MobileServiceUser currentUser = client.getCurrentUser();

                    if (currentUser == null) {
                        result.setStatus(TestStatus.Failed);
                    } else {
                        result.setStatus(TestStatus.Passed);
                    }
                    result.setTestCase(this);

                    callback.onTestComplete(this, result);

                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                    return;
                }
            }
        };

        return test;
    }

    public static TestCase createLogoutTest() {

        TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, TestExecutionCallback callback) {
                try {
                    client.logout().get();
                } catch (Exception ex) {
                    Assert.fail(ex.getMessage());
                }

                log("Logged out");
                TestResult result = new TestResult();
                result.setTestCase(this);
                result.setStatus(client.getCurrentUser() == null ? TestStatus.Passed : TestStatus.Failed);

                callback.onTestComplete(this, result);
            }
        };

        test.setName("Logout");

        return test;
    }

    @SuppressWarnings("deprecation")
    public static TestCase createLoginWithCallbackTest(final MobileServiceAuthenticationProvider provider) {
        TestCase test = new TestCase(provider.toString() + " Login With Callback") {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {
                final TestCase testCase = this;
                long seed = new Date().getTime();
                final Random rndGen = new Random(seed);

                UserAuthenticationCallback authCallback = new UserAuthenticationCallback() {

                    @Override
                    public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                        TestResult result = new TestResult();
                        String userName;
                        if (user == null) {
                            log("Error during login, user == null");
                            if (exception != null) {
                                log("Exception: " + exception.toString());
                            }

                            userName = "NULL";
                        } else {
                            userName = user.getUserId();
                        }

                        log("Logged in as " + userName);
                        result.setStatus(client.getCurrentUser() != null ? TestStatus.Passed : TestStatus.Failed);
                        result.setTestCase(testCase);

                        callback.onTestComplete(testCase, result);
                    }
                };

                boolean useEnumOverload = rndGen.nextBoolean();
                if (useEnumOverload) {
                    log("Calling the overload MobileServiceClient.login(MobileServiceAuthenticationProvider, UserAuthenticationCallback)");
                    client.login(provider, authCallback);
                } else {
                    log("Calling the overload MobileServiceClient.login(String, UserAuthenticationCallback)");
                    client.login(provider.toString(), authCallback);
                }
            }
        };

        return test;
    }

    public static TestCase createLogoutWithCallbackTest() {

        TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, TestExecutionCallback callback) {

                try {
                    client.logout().get();
                } catch (Exception ex) {
                    Assert.fail(ex.getMessage());
                }
                log("Logged out");
                TestResult result = new TestResult();
                result.setTestCase(this);
                result.setStatus(client.getCurrentUser() == null ? TestStatus.Passed : TestStatus.Failed);

                callback.onTestComplete(this, result);
            }
        };

        test.setName("With Callback - Logout");

        return test;
    }

    private TestCase createClientSideLoginTest(final MobileServiceAuthenticationProvider provider) {
        TestCase test = new TestCase(provider.toString() + " Token Login") {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                final TestCase testCase = this;
                long seed = new Date().getTime();
                final Random rndGen = new Random(seed);

                if (lastUserIdentityObject == null) {
                    log("Last identity is null. Cannot run this test.");
                    TestResult testResult = new TestResult();
                    testResult.setTestCase(testCase);
                    testResult.setStatus(TestStatus.Failed);
                    callback.onTestComplete(testCase, testResult);
                    return;
                }

                JsonObject providerToken = getProviderToken(lastUserIdentityObject, provider);
                lastUserIdentityObject = null;
                if (providerToken == null) {
                    log("Cannot find identity for specified provider. Cannot run this test.");
                    TestResult testResult = new TestResult();
                    testResult.setTestCase(testCase);
                    testResult.setStatus(TestStatus.Failed);
                    callback.onTestComplete(testCase, testResult);
                    return;
                }

                boolean useEnumOverload = rndGen.nextBoolean();
                if (useEnumOverload) {
                    log("Calling the overload MobileServiceClient.login(MobileServiceAuthenticationProvider, JsonObject, UserAuthenticationCallback)");

                    TestResult testResult = new TestResult();
                    testResult.setTestCase(testCase);
                    try {

                        MobileServiceUser user = client.login(provider, providerToken).get();

                        log("Logged in as " + user.getUserId());
                        testResult.setStatus(TestStatus.Passed);
                    } catch (Exception exception) {
                        log("Exception during login: " + exception.toString());
                        testResult.setStatus(TestStatus.Failed);
                    }

                    callback.onTestComplete(testCase, testResult);

                } else {
                    log("Calling the overload MobileServiceClient.login(String, JsonObject, UserAuthenticationCallback)");

                    TestResult testResult = new TestResult();
                    testResult.setTestCase(testCase);
                    try {

                        MobileServiceUser user = client.login(provider.toString(), providerToken).get();

                        log("Logged in as " + user.getUserId());
                        testResult.setStatus(TestStatus.Passed);
                    } catch (Exception exception) {
                        log("Exception during login: " + exception.toString());
                        testResult.setStatus(TestStatus.Failed);
                    }

                    callback.onTestComplete(testCase, testResult);
                }
            }
        };

        return test;
    }

    private TestCase createCRUDTest(final String tableName, final MobileServiceAuthenticationProvider provider, final TablePermission tableType,
                                    final boolean userIsAuthenticated) {
        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(this);
                final TestCase testCase = this;

                MobileServiceClient logClient = client.withFilter(new LogServiceFilter());

                final MobileServiceJsonTable table = logClient.getTable(tableName);
                final boolean crudShouldWork = tableType == TablePermission.Public || (tableType == TablePermission.User && userIsAuthenticated);
                final JsonObject item = new JsonObject();
                item.addProperty("name", "John Doe");
                log("insert item");

                String id = "1";

                try {

                    JsonObject jsonEntityInsert = table.insert(item).get();

                    id = jsonEntityInsert.get("id").getAsString();

                    item.addProperty("id", id);
                } catch (Exception exception) {
                    if (!validateExecution(crudShouldWork, exception, result)) {
                        callback.onTestComplete(testCase, result);
                        return;
                    }
                }

                item.addProperty("name", "Jane Doe");
                log("update item");

                try {
                    table.update(item).get();
                } catch (Exception exception) {
                    if (!validateExecution(crudShouldWork, exception, result)) {
                        callback.onTestComplete(testCase, result);
                        return;
                    }
                }

                log("lookup item");

                try {

                    JsonObject jsonEntity = table.lookUp(item.get("id").getAsString()).get().getAsJsonObject();
                    if (userIsAuthenticated && tableType == TablePermission.User) {
                        lastUserIdentityObject = parseIdentityObject(jsonEntity);
                    }

                    log("delete item");

                } catch (Exception exception) {
                    if (!validateExecution(crudShouldWork, exception, result)) {
                        callback.onTestComplete(testCase, result);
                        return;
                    }
                }

                try {
                    table.delete(item.get("id").getAsString()).get();
                } catch (Exception exception) {
                    if (!validateExecution(crudShouldWork, exception, result)) {
                        callback.onTestComplete(testCase, result);
                        return;
                    }
                }

                callback.onTestComplete(testCase, result);

                return;
            }

            private boolean validateExecution(boolean crudShouldWork, Exception exception, TestResult result) {
                if (crudShouldWork && exception != null || !crudShouldWork && exception == null) {
                    createResultFromException(result, exception);
                    result.setStatus(TestStatus.Failed);
                    return false;
                } else {
                    return true;
                }
            }
        };

        String providerString;
        if (userIsAuthenticated) {
            providerString = provider.toString();
        } else {
            providerString = "(Neg) Unauthenticated";
        }

        test.setName(providerString + " CRUD");

        return test;
    }

    @SuppressWarnings("deprecation")
    private TestCase createClientSideLoginWithCallbackTest(final MobileServiceAuthenticationProvider provider) {
        TestCase test = new TestCase(provider.toString() + " Token Login With Callback") {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                final TestCase testCase = this;
                long seed = new Date().getTime();
                final Random rndGen = new Random(seed);

                if (lastUserIdentityObject == null) {
                    log("Last identity is null. Cannot run this test.");
                    TestResult testResult = new TestResult();
                    testResult.setTestCase(testCase);
                    testResult.setStatus(TestStatus.Failed);
                    callback.onTestComplete(testCase, testResult);
                    return;
                }

                JsonObject providerToken = getProviderToken(lastUserIdentityObject, provider);
                lastUserIdentityObject = null;

                if (providerToken == null) {
                    log("Cannot find identity for specified provider. Cannot run this test.");
                    TestResult testResult = new TestResult();
                    testResult.setTestCase(testCase);
                    testResult.setStatus(TestStatus.Failed);
                    callback.onTestComplete(testCase, testResult);
                    return;
                }

                UserAuthenticationCallback authCallback = new UserAuthenticationCallback() {

                    @Override
                    public void onCompleted(MobileServiceUser user, Exception exception, ServiceFilterResponse response) {
                        TestResult testResult = new TestResult();
                        testResult.setTestCase(testCase);
                        if (exception != null) {
                            log("Exception during login: " + exception.toString());
                            testResult.setStatus(TestStatus.Failed);
                        } else {
                            log("Logged in as " + user.getUserId());
                            testResult.setStatus(TestStatus.Passed);
                        }

                        callback.onTestComplete(testCase, testResult);
                    }
                };
                boolean useEnumOverload = rndGen.nextBoolean();
                if (useEnumOverload) {
                    log("Calling the overload MobileServiceClient.login(MobileServiceAuthenticationProvider, JsonObject, UserAuthenticationCallback)");
                    client.login(provider, providerToken, authCallback);
                } else {
                    log("Calling the overload MobileServiceClient.login(String, JsonObject, UserAuthenticationCallback)");
                    client.login(provider.toString(), providerToken, authCallback);
                }
            }
        };

        return test;
    }

    @SuppressWarnings("deprecation")
    private TestCase createCRUDWithCallbackTest(final String tableName, final MobileServiceAuthenticationProvider provider, final TablePermission tableType,
                                                final boolean userIsAuthenticated) {
        final TestCase test = new TestCase() {

            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {

                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(this);
                final TestCase testCase = this;

                MobileServiceClient logClient = client.withFilter(new LogServiceFilter());

                final MobileServiceJsonTable table = logClient.getTable(tableName);
                final boolean crudShouldWork = tableType == TablePermission.Public || (tableType == TablePermission.User && userIsAuthenticated);
                final JsonObject item = new JsonObject();
                item.addProperty("name", "John Doe");
                log("insert item");
                table.insert(item, new TableJsonOperationCallback() {

                    @Override
                    public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
                        String id = "1";
                        if (exception == null) {
                            id = jsonEntity.get("id").getAsString();
                        }

                        item.addProperty("id", id);
                        if (!validateExecution(crudShouldWork, exception, result)) {
                            callback.onTestComplete(testCase, result);
                            return;
                        }

                        item.addProperty("name", "Jane Doe");
                        log("update item");
                        table.update(item, new TableJsonOperationCallback() {

                            @Override
                            public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {

                                if (!validateExecution(crudShouldWork, exception, result)) {
                                    callback.onTestComplete(testCase, result);
                                    return;
                                }

                                log("lookup item");
                                table.lookUp(item.get("id").getAsString(), new TableJsonOperationCallback() {

                                    @Override
                                    public void onCompleted(JsonObject jsonEntity, Exception exception, ServiceFilterResponse response) {
                                        if (!validateExecution(crudShouldWork, exception, result)) {
                                            callback.onTestComplete(testCase, result);
                                            return;
                                        }


                                        if (userIsAuthenticated && tableType == TablePermission.User) {
                                            lastUserIdentityObject = parseIdentityObject(jsonEntity);
                                        }

                                        log("delete item");
                                        table.delete(item.get("id").getAsString(), new TableDeleteCallback() {

                                            @Override
                                            public void onCompleted(Exception exception, ServiceFilterResponse response) {
                                                validateExecution(crudShouldWork, exception, result);

                                                callback.onTestComplete(testCase, result);
                                                return;
                                            }
                                        });
                                    }
                                });
                            }
                        });
                    }
                });
            }

            private boolean validateExecution(boolean crudShouldWork, Exception exception, TestResult result) {
                if (crudShouldWork && exception != null || !crudShouldWork && exception == null) {
                    createResultFromException(result, exception);
                    result.setStatus(TestStatus.Failed);
                    return false;
                } else {
                    return true;
                }
            }
        };

        String providerString;
        if (userIsAuthenticated) {
            providerString = provider.toString();
        } else {
            providerString = "(Neg) Unauthenticated";
        }

        test.setName(providerString + " CRUD With Callback");

        return test;
    }

    enum TablePermission {
        Public, User
    }

    JsonObject parseIdentityObject(JsonObject jsonEntity) {
        if (isNetBackend) {
            return new JsonParser().parse(jsonEntity.get("identities").getAsString()).getAsJsonObject();
        } else {
            return jsonEntity.getAsJsonObject("identities");
        }
    }

    JsonObject getProviderToken(JsonObject lastIdentityObject, MobileServiceAuthenticationProvider provider) {
        return lastIdentityObject.getAsJsonObject(provider.toString().toLowerCase(Locale.US));
    }
}