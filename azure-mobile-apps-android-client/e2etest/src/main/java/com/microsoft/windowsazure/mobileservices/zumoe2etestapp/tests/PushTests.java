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

import android.support.annotation.NonNull;
import android.util.Pair;

import com.google.common.util.concurrent.FutureCallback;
import com.google.common.util.concurrent.Futures;
import com.google.common.util.concurrent.ListenableFuture;
import com.google.common.util.concurrent.MoreExecutors;
import com.google.common.util.concurrent.SettableFuture;
import com.google.firebase.iid.FirebaseInstanceId;
import com.google.gson.JsonArray;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;
import com.microsoft.windowsazure.mobileservices.MobileServiceApplication;
import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.authentication.MobileServiceUser;
import com.microsoft.windowsazure.mobileservices.http.HttpConstants;
import com.microsoft.windowsazure.mobileservices.notifications.Installation;
import com.microsoft.windowsazure.mobileservices.notifications.InstallationTemplate;
import com.microsoft.windowsazure.mobileservices.notifications.MobileServicePush;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.MainActivity;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.PushMessageManager;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestCase;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestExecutionCallback;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestGroup;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestResult;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.TestStatus;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;

public class PushTests extends TestGroup {

    public static MainActivity mainActivity;
    private static String registrationId;

    private final static String TOPIC_SPORTS = "topic:Sports";
    private final static String TOPIC_NEWS = "topic:News";
    private final static String PLATFORM =  "gcm";
    private final static String TEMPLATE_NAME = "FcmTemplate";

    public PushTests() {
        super("Push tests");

        // registration tests
        this.addTest(createInitialDeleteRegistrationTest("Initial DeleteRegistration Test"));

        this.addTest(createRegistrationTest("Registration Test"));

        this.addTest(createLoginRegistrationTest("Login Registration Test"));

        this.addTest(createUnregistrationTest("Unregistration Test"));

        // registration tests combined with push tests
        this.addTest(createPushWithNoTemplateOrTagsTest("Push Test with no template or tags"));

        this.addTest(createPushWithTagsNoTemplateTest("Push Test with tags but no template"));

        this.addTest(createPushWithTemplateNoTagsTest("Push Test with template but no tags"));

        this.addTest(createPushWithTagsAndTemplateTest("Push Test with template and tags"));
    }

    private ListenableFuture<JsonElement> deleteRegistrationsForChannel(final MobileServiceClient client, String registrationId) {

        final SettableFuture<JsonElement> resultFuture = SettableFuture.create();
        ArrayList parameters = new ArrayList<>();

        parameters.add(new Pair<>("channelUri", registrationId));
        ListenableFuture<JsonElement> serviceFilterFuture = client.invokeApi("deleteRegistrationsForChannel", HttpConstants.DeleteMethod, parameters);

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                resultFuture.set(response);
            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }

    private TestCase createInitialDeleteRegistrationTest(final String testName) {
        final TestCase deleteChannelTest = new TestCase() {
            @Override
            protected void executeTest(MobileServiceClient client, final TestExecutionCallback callback) {
                this.log("Starting InitialDeleteRegistrationTest");
                final TestResult result = new TestResult();
                result.setStatus(TestStatus.Passed);
                result.setTestCase(this);

                try {
                    String registrationId = getRegistrationId(client);
                    this.log("Acquired registrationId:" + registrationId);

                    JsonElement deleteChannelResult = deleteRegistrationsForChannel(client, registrationId).get();

                    if (deleteChannelResult.isJsonNull() || !deleteChannelResult.isJsonObject() || deleteChannelResult.getAsJsonObject().get("result").getAsBoolean() == false ) {
                        this.log("deleteRegistrationsForChannel failed: " + deleteChannelResult.toString());
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    this.log("deleteRegistrationsForChannel successful");
                    clearRegistrationId();
                    this.log("Instance id cleared");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, this.createResultFromException(e));

                }
            }
        };

        deleteChannelTest.setName(testName);
        return deleteChannelTest;
    }

    private TestCase createRegistrationTest(String testName) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    this.log("Starting Registration Test");
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final MobileServicePush mobileServicePush = client.getPush();
                    String registrationId = getRegistrationId(client);
                    this.log("Acquired registrationId:" + registrationId);

                    mobileServicePush.register(registrationId).get();

                    ArrayList<Pair<String, String>> parameters = new ArrayList<>();
                    parameters.add(new Pair<>("channelUri", registrationId));
                    JsonElement registerResult = verifyRegisterInstallationResult(client, parameters).get();
                    if (!registerResult.getAsBoolean()) {
                        this.log("Register failed");
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    this.log("Verified registration");

                    mobileServicePush.unregister().get();
                    this.log("Unregistration done");
                    this.log("Test complete");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                }
            }
        };

        test.setName(testName);
        return test;
    }

    private TestCase createLoginRegistrationTest(String testName) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    this.log("Starting Login Registration Test");
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    String registrationId = getRegistrationId(client);

                    this.log("Acquired registrationId:" + registrationId);

                    final MobileServicePush mobileServicePush = client.getPush();
                    MobileServiceUser dummyUser = getDummyUser(client).get();
                    client.setCurrentUser(dummyUser);

                    this.log("Setting current user to:" + dummyUser.getUserId());

                    mobileServicePush.register(registrationId).get();
                    ArrayList<Pair<String, String>> parameters = new ArrayList<>();
                    parameters.add(new Pair<>("channelUri", registrationId));
                    JsonElement registerResult = verifyRegisterInstallationResult(client, parameters).get();

                    if (!registerResult.getAsBoolean()) {
                        this.log("Register failed");
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    this.log("Verified registration");

                    mobileServicePush.unregister().get();

                    this.log("Unregistration done");
                    client.setCurrentUser(null);
                    this.log("Test complete");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                }
            }
        };

        test.setName(testName);
        return test;
    }

    private TestCase createUnregistrationTest(String testName) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    this.log("Starting UnRegistration Test");
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);
                    final MobileServicePush mobileServicePush = client.getPush();
                    mobileServicePush.unregister().get();

                    JsonElement unregisterResult = verifyUnregisterInstallationResult(client).get();

                    if (!unregisterResult.getAsBoolean()) {
                        this.log("Unregister failed");
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return;
                    }

                    this.log("Unregister complete");
                    this.log("Test complete");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                }
            }
        };

        test.setName(testName);
        return test;
    }

    private TestCase createPushWithNoTemplateOrTagsTest(String testName) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    this.log("Starting createPushWithNoTemplateOrTagsTest");
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    final MobileServicePush mobileServicePush = client.getPush();
                    String registrationId = getRegistrationId(client);
                    this.log("Acquired registrationId:" + registrationId);

                    // register with pnsHandle
                    mobileServicePush.register(registrationId).get();
                    this.log("registration complete");

                    // send push notification
                    if (!sendPushNotification(client, callback, result)) {
                        return;
                    }

                    // cleanup
                    mobileServicePush.unregister().get();
                    this.log("OnCompleted.");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                }
            }

            private boolean sendPushNotification(MobileServiceClient client, TestExecutionCallback callback, TestResult result) throws InterruptedException, java.util.concurrent.ExecutionException {

                JsonObject item = new JsonObject();
                item.addProperty("method", "send");
                item.addProperty("token", "dummy");
                item.addProperty("type", PLATFORM);

                JsonObject sentPayload = new JsonObject();

                JsonObject pushContent = new JsonObject();
                pushContent.addProperty("sample", "PushTest");

                JsonObject payload = new JsonObject();
                payload.add("message", pushContent);

                sentPayload.add("data", payload);
                item.add("payload", sentPayload);

                this.log("sending push message:" + sentPayload.toString());
                client.invokeApi("Push", item).get();
                if (PushMessageManager.instance.isPushMessageReceived(60000, pushContent).get()) {
                    this.log(sentPayload.toString() + " message received");
                    return true;
                } else {
                    result.setStatus(TestStatus.Failed);
                    callback.onTestComplete(this, result);
                    return false;
                }
            }
        };

        test.setName(testName);

        return test;
    }

    private TestCase createPushWithTagsNoTemplateTest(String testName) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    this.log("Starting createPushWithTagsNoTemplateTest");
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    MobileServicePush mobileServicePush = client.getPush();

                    // Register "topic:Sports" tag, not "topic:News"
                    ArrayList<String> tags = new ArrayList<>();
                    tags.add(TOPIC_SPORTS);

                    Installation installation = createInstallation(client, tags);

                    // register with installation
                    mobileServicePush.register(installation).get();
                    this.log("registration complete");

                    // Send push notification to device with "topic:Sports" tag,
                    // we should expect to get the notification
                    if (!sendPushNotification(TOPIC_SPORTS, client, callback, result)) {
                        return;
                    }

                    // Send push notification to device with "topic:News" tag,
                    // we won't get the notification because we didn't register for it
                    if (!sendPushNotification(TOPIC_NEWS, client, callback, result)) {
                        return;
                    }

                    mobileServicePush.unregister().get();
                    this.log("OnCompleted.");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                }
            }

            @NonNull
            private Installation createInstallation(MobileServiceClient client, ArrayList<String> tags) throws IOException {
                String registrationId = getRegistrationId(client);
                this.log("Acquired registrationId:" + registrationId);

                String installationId = MobileServiceApplication.getInstallationId(client.getContext());
                HashMap<String, String> pushVariables = null;

                return new Installation(installationId, PLATFORM, registrationId, pushVariables, tags, null);
            }

            private boolean sendPushNotification(String tag, MobileServiceClient client, TestExecutionCallback callback, TestResult result) throws InterruptedException, java.util.concurrent.ExecutionException {

                JsonObject item = new JsonObject();
                item.addProperty("method", "send");
                item.addProperty("token", "dummy");
                item.addProperty("type", PLATFORM);

                JsonObject sentPayload = new JsonObject();

                JsonObject pushContent = new JsonObject();
                pushContent.addProperty("sample", tag + " message for your interest");

                JsonObject payload = new JsonObject();
                payload.add("message", pushContent);

                sentPayload.add("data", payload);
                item.add("payload", sentPayload);
                item.addProperty("tag", tag);

                this.log("sending push message:" + sentPayload.toString() + " with tag " + tag);

                client.invokeApi("Push", item).get();

                if (tag.equals(TOPIC_SPORTS)) {
                    if (PushMessageManager.instance.isPushMessageReceived(60000, pushContent).get()) {
                        this.log(sentPayload.toString() + " message received because we registered tag " + tag);
                        return true;
                    } else {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return false;
                    }
                } else {
                    if (!PushMessageManager.instance.isPushMessageReceived(60000, pushContent).get()) {
                        this.log("We shouldn't receive message " + sentPayload.toString() + " because we didn't register tag " + tag);
                        return true;
                    } else {
                        result.setStatus(TestStatus.Failed);
                        callback.onTestComplete(this, result);
                        return false;
                    }
                }
            }
        };

        test.setName(testName);

        return test;
    }

    private TestCase createPushWithTemplateNoTagsTest(String testName) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    this.log("Starting createPushWithTemplateNoTagsTest");

                    final MobileServicePush mobileServicePush = client.getPush();

                    Installation installation = createInstallation(client);

                    // register with installation
                    mobileServicePush.register(installation);

                    PushMessageManager.instance.clearMessages();

                    // send push notification
                    if (!sendPushNotification(client, callback, result)) {
                        return;
                    }

                    mobileServicePush.unregister().get();
                    this.log("OnCompleted.");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                }
            }

            @NonNull
            private Installation createInstallation(MobileServiceClient client) throws IOException {
                String registrationId = getRegistrationId(client);
                this.log("Acquired registrationId:" + registrationId);

                HashMap<String, String> pushVariables = null;
                HashMap<String, InstallationTemplate> templates = new HashMap<>();
                InstallationTemplate template = new InstallationTemplate("{\"data\":{\"message\":{\"user\":\"$(fullName)\"}}}", null);
                templates.put(TEMPLATE_NAME, template);

                String installationId = MobileServiceApplication.getInstallationId(client.getContext());

                return new Installation(installationId, PLATFORM, registrationId, pushVariables, null, templates);
            }

            private boolean sendPushNotification(MobileServiceClient client, TestExecutionCallback callback, TestResult result) throws InterruptedException, java.util.concurrent.ExecutionException {
                JsonObject pushMessage = new JsonObject();
                pushMessage.addProperty("fullName", "John Doe");

                JsonObject item = new JsonObject();
                item.addProperty("method", "send");
                item.addProperty("token", "dummy");
                item.addProperty("type", "template");
                item.add("payload", pushMessage);

                JsonObject expectedPushMessage = new JsonObject();
                expectedPushMessage.addProperty("user", "John Doe");

                this.log("sending push message:" + item.toString());
                client.invokeApi("Push", item).get();

                if (PushMessageManager.instance.isPushMessageReceived(60000, expectedPushMessage).get()) {
                    this.log("push received:" + expectedPushMessage.toString());
                    return true;
                } else {
                    result.setStatus(TestStatus.Failed);
                    callback.onTestComplete(this, result);
                    return false;
                }
            }
        };

        test.setName(testName);

        return test;
    }

    private TestCase createPushWithTagsAndTemplateTest(String testName) {

        TestCase test = new TestCase(testName) {

            @Override
            protected void executeTest(final MobileServiceClient client, final TestExecutionCallback callback) {

                try {
                    final TestResult result = new TestResult();
                    result.setStatus(TestStatus.Passed);
                    result.setTestCase(this);

                    this.log("Starting createPushWithTagsAndTemplateTest");

                    final MobileServicePush mobileServicePush = client.getPush();

                    Installation installation = createInstallation(client);

                    // register with installation
                    mobileServicePush.register(installation);

                    this.log("registration complete");

                    // send push notification
                    if (!sendPushNotification(client, callback, result)) {
                        return;
                    }

                    mobileServicePush.unregister().get();
                    this.log("OnCompleted.");
                    callback.onTestComplete(this, result);
                } catch (Exception e) {
                    callback.onTestComplete(this, createResultFromException(e));
                }
            }

            @NonNull
            private Installation createInstallation(MobileServiceClient client) throws IOException {
                String registrationId = getRegistrationId(client);
                this.log("Acquired registrationId:" + registrationId);

                ArrayList<String> tags = new ArrayList<>();
                tags.add(TOPIC_SPORTS);

                HashMap<String, String> pushVariables = null;
                HashMap<String, InstallationTemplate> templates = new HashMap<>();
                InstallationTemplate template = new InstallationTemplate("{\"data\":{\"message\":{\"user\":\"$(fullName)\"}}}", tags);
                templates.put(TEMPLATE_NAME, template);

                String installationId = MobileServiceApplication.getInstallationId(client.getContext());

                return new Installation(installationId, PLATFORM, registrationId, pushVariables, tags, templates);
            }

            private boolean sendPushNotification(MobileServiceClient client, TestExecutionCallback callback, TestResult result) throws InterruptedException, java.util.concurrent.ExecutionException {
                JsonObject item = new JsonObject();
                item.addProperty("method", "send");
                item.addProperty("token", "dummy");
                item.addProperty("type", PLATFORM);

                JsonObject sentPayload = new JsonObject();

                JsonObject pushContent = new JsonObject();
                pushContent.addProperty("sample", "message of sports");

                JsonObject payload = new JsonObject();
                payload.add("message", pushContent);

                sentPayload.add("data", payload);
                item.add("payload", sentPayload);
                item.addProperty("tag", TOPIC_SPORTS);

                this.log("sending push message:" + sentPayload.toString() + " with tag " + TOPIC_SPORTS);
                client.invokeApi("Push", item).get();
                if (PushMessageManager.instance.isPushMessageReceived(60000, pushContent).get()) {
                    this.log(sentPayload.toString() + " message received because we registered tag " + TOPIC_SPORTS);
                    return true;
                } else {
                    result.setStatus(TestStatus.Failed);
                    callback.onTestComplete(this, result);
                    return false;
                }
            }
        };

        test.setName(testName);

        return test;
    }

    private ListenableFuture<JsonElement> verifyRegisterInstallationResult(MobileServiceClient client, ArrayList<Pair<String, String>> parameters) throws IOException {

        final SettableFuture<JsonElement> resultFuture = SettableFuture.create();
        ListenableFuture<JsonElement> serviceFilterFuture = client.invokeApi("verifyRegisterInstallationResult", HttpConstants.GetMethod, parameters);

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                resultFuture.set(response);
            }
        });

        return resultFuture;
    }

    private ListenableFuture<MobileServiceUser> getDummyUser(MobileServiceClient client) {

        final SettableFuture<MobileServiceUser> resultFuture = SettableFuture.create();
        ListenableFuture<JsonElement> serviceFilterFuture = client.invokeApi("JwtTokenGenerator", HttpConstants.GetMethod, (ArrayList) null);

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                JsonObject userJsonObject = response.getAsJsonObject();
                String userId = userJsonObject.getAsJsonPrimitive("userId").getAsString();
                String authenticationToken = userJsonObject.getAsJsonPrimitive("authenticationToken").getAsString();
                MobileServiceUser user = new MobileServiceUser(userId);
                user.setAuthenticationToken(authenticationToken);
                resultFuture.set(user);

            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }

    private String getRegistrationId(MobileServiceClient client) throws IOException {
        if (registrationId == null) {
            registrationId = FirebaseInstanceId.getInstance().getToken();
        }
        return registrationId;
    }

    private void clearRegistrationId() {
        registrationId = null;
    }

    private ListenableFuture<JsonElement> verifyUnregisterInstallationResult(final MobileServiceClient client) {

        final SettableFuture<JsonElement> resultFuture = SettableFuture.create();

        ListenableFuture<JsonElement> serviceFilterFuture = client.invokeApi("verifyUnregisterInstallationResult", HttpConstants.GetMethod, new ArrayList<Pair<String, String>>());

        Futures.addCallback(serviceFilterFuture, new FutureCallback<JsonElement>() {
            @Override
            public void onFailure(Throwable exception) {
                resultFuture.setException(exception);
            }

            @Override
            public void onSuccess(JsonElement response) {
                resultFuture.set(response);
            }
        }, MoreExecutors.directExecutor());

        return resultFuture;
    }

    private JsonObject GetTemplate() {
        String templateName = TEMPLATE_NAME;

        JsonObject userJson = new JsonObject();
        userJson.addProperty("user", "$(fullName)");

        JsonObject messageJson = new JsonObject();
        messageJson.add("message", userJson);

        JsonObject dataJson = new JsonObject();
        dataJson.add("data", messageJson);

        JsonObject bodyJson = new JsonObject();
        bodyJson.add("body", dataJson);

        JsonArray tagsJsonArray = new JsonArray();
        tagsJsonArray.add(new JsonPrimitive(TOPIC_SPORTS));

        bodyJson.add("tags", tagsJsonArray);

        JsonObject templateJson = new JsonObject();
        templateJson.add(templateName, bodyJson);
        return templateJson;
    }
}